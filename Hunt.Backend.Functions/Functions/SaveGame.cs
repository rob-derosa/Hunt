 using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Hunt.Common;
using System.Text;

namespace Hunt.Backend.Functions
{
	public static class SaveGame
	{
		[FunctionName(nameof(SaveGame))]

		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(SaveGame))]
		HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(SaveGame)
			}))
			{
				var json = req.Content.ReadAsStringAsync().Result;
				var jobject = JsonConvert.DeserializeObject<JObject>(json);

				var action = jobject["action"].ToString();
				var arguments = jobject["arguments"].ToObject<Dictionary<string, string>>();
				var game = jobject["game"].ToObject<Game>();
				arguments = arguments ?? new Dictionary<string, string>();

				Player player = null;
				if (arguments.ContainsKey("playerId"))
					player = game.GetPlayer(arguments["playerId"]);				

				//Need to validate this player is not already part of another ongoing game or the coordinator of this game
				if (game.EntryCode == null)
				{
					//Let's hope this is generally random. Best to confirm the code is not already in used but I'm lazy
					game.EntryCode = Math.Abs(game.Id.GetHashCode()).ToString().Substring(0, 6);
				}

				Game savedGame = null;
				bool isEndOfgame = false;
				try
				{
					if (!game.IsPersisted)
					{
						await EventHubService.Instance.SendEvent($"New game created\n{JsonConvert.SerializeObject(game, Formatting.Indented)}", game, player);
						await CosmosDataService.Instance.InsertItemAsync(game);
					}
					else
					{
						var existingGame = await CosmosDataService.Instance.GetItemAsync<Game>(game.Id);
						if (existingGame.TS != game.TS)
						{
							await EventHubService.Instance.SendEvent("Game save attempt resulted in version collision", game, player);
							return req.CreateErrorResponse(HttpStatusCode.Conflict, "Unable to save game - version conflict. Please pull the latest version and reapply your changes.");
						}

						if (action == GameUpdateAction.EndGame && existingGame.HasEnded)
							return req.CreateResponse(HttpStatusCode.OK);

						if (action == GameUpdateAction.StartGame)
							game.StartDate = DateTime.UtcNow;

						if (action == GameUpdateAction.EndGame)
							isEndOfgame = true;

						bool isWinningAcquisition = false;
						if (action == GameUpdateAction.AcquireTreasure)
						{
							//Need to evaluate the game first before we save as there might be a winner
							var teamId = arguments["teamId"];
							var team = game.Teams.Single(t => t.Id == teamId);
							isWinningAcquisition = game.EvaluateGameForWinner(teamId);

							if (isWinningAcquisition)
								isEndOfgame = true;
						}

						if (isEndOfgame)
						{
							game.EndDate = DateTime.UtcNow;
							var teams = game.Teams.OrderByDescending(t => t.TotalPoints).ToArray();

							if (teams[0].TotalPoints == teams[1].TotalPoints)
								game.WinnningTeamId = null; //Draw
							else
								game.WinnningTeamId = teams[0].Id;
						}

						CosmosDataService.Instance.UpdateItemAsync(game).Wait();
						await EventHubService.Instance.SendEvent($"Game saved successfully", game, player);

						if (action == GameUpdateAction.StartGame)
						{
							SetEndGameTimer(game, analytic);
						}

						if (isEndOfgame)
						{
							SendTargetedNotifications(game, GameUpdateAction.EndGame, arguments);
						}
						else
						{
							SendTargetedNotifications(game, action, arguments);
						}
					}

					savedGame = CosmosDataService.Instance.GetItemAsync<Game>(game.Id).Result; //Comment out at some point if not needed

					return req.CreateResponse(HttpStatusCode.OK, savedGame);
				}
				catch (Exception e)
				{
					// track exceptions that occur
					analytic.TrackException(e);
					return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message, e);
				}
			}
		}

		#region Game Timer

		static void SetEndGameTimer(Game game, AnalyticService analytic)
		{
			try
			{
				var client = new QueueService(ConfigManager.EndGameBusName);
				client.SendBrokeredMessageAsync(game.DurationInMinutes, game.Id, "endgametime", (int)game.DurationInMinutes).Wait();
			}
			catch (Exception e)
			{
				analytic.TrackException(e);
			}
		}

		#endregion

		#region Push Notifications

		static async Task SendTargetedNotifications(Game game, string action, Dictionary<string, string> args)
		{
			string title = null;
			string message = null;
			List<Player> players = new List<Player>();
			bool silentNotifyAllPlayers = false;

			Player player = null;
			if (args.ContainsKey("playerId"))
				player = game.GetPlayer(args["playerId"]);

			switch (action)
			{
				case GameUpdateAction.StartGame:
					{
						//Notify all game players
						title = "Hunt Game has started!";
						message = $"Your hunt game has started! You have {game.DurationInMinutes}min to acquire all treasures - good luck and godspeed!";

						players.AddRange(game.GetAllPlayers());
						await EventHubService.Instance.SendEvent($"Game started at {game.StartDate.Value.ToLongTimeString()}", game, player);
						break;
					}
				case GameUpdateAction.EndGame:
					{
						//Notify all players + coordinator
						var team = game.Teams.Get(game.WinnningTeamId);
						title = "Your hunt game has ended";
						message = "Game over. This game ended in a draw.";

						if (team != null)
							message = $"Game Over. Team {team.Name} is the winner.";

						players.AddRange(game.GetAllPlayers());

						await EventHubService.Instance.SendEvent($"Game ended: {message}", game, player);
						break;
					}
				case GameUpdateAction.JoinTeam:
					{
						silentNotifyAllPlayers = true;
						var team = game.Teams.Get(args["teamId"]);

						if (team == null || player == null)
							break;

						//Notify team players
						title = "New teammate :)";
						message = $"{player.Alias} has joined your team. You should say hello.";

						players.AddRange(team.Players);
						players.Remove(player);

						await EventHubService.Instance.SendEvent($"Player joined team {team.Name}", game, player);
						break;
					}
				case GameUpdateAction.LeaveTeam:
					{
						silentNotifyAllPlayers = true;
						var playerAlias = args["playerAlias"];
						var team = game.Teams.Get(args["teamId"]);

						if (team == null || playerAlias == null)
							break;

						//Notify team players
						title = "Someone left your team :(";
						message = $"{playerAlias} had to leave your team - they're sorry.";
						players.AddRange(team.Players);

						await EventHubService.Instance.SendEvent($"Player {playerAlias} left team {team.Name}", game, player);
						break;
					}
				case GameUpdateAction.AcquireTreasure:
					{
						var team = game.Teams.Get(args["teamId"]);
						var acquiredTreasure = team.AcquiredTreasure.Get(args["acquiredTreasureId"]);
						var p = game.GetPlayer(acquiredTreasure.PlayerId);
						var treasure = game.Treasures.Get(acquiredTreasure.TreasureId);

						if (team == null || p == null || acquiredTreasure == null)
							break;

						//Notify team players
						title = $"Treasure acquired for {treasure.Points} points!";
						message = $"{player.Alias} just acquired the '{treasure.Hint}' treasure";
						players.AddRange(team.Players);
						players.Remove(p);
						
						silentNotifyAllPlayers = true;

						var evnt = $"Team {team.Name} acquired treasure\n\tHint:\t\t\"{treasure.Hint}\"\n\tPoints:\t\t{acquiredTreasure.ClaimedPoints}\n\tSubmitted:\t{acquiredTreasure.ImageSource}";
						await EventHubService.Instance.SendEvent(evnt, game, player);
						break;
					}
				case GameUpdateAction.AddTreasure:
					{
						if (!args.ContainsKey("treasureId"))
							break;

						var treasure = game.Treasures.Single(t => t.Id == args["treasureId"]);
						var tags = string.Join(", ", treasure.Attributes.Select(t => t.Name).ToArray());
						await EventHubService.Instance.SendEvent($"Treasure added to game\n\tHint:\t\"{treasure.Hint}\"\n\tTag(s):\t{tags}\n\tPoints:\t{treasure.Points}\n\tImage:\t{treasure.ImageSource}", game, player);
						break;
					}

				default:
					silentNotifyAllPlayers = true;
					break;
			}

			var devices = new List<string>();
			if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(message) && players.Count > 0)
			{
				devices = players.Where(pl => pl.Id != null).Select(pl => pl.Id).ToList();
				var payload = new Dictionary<string, string> { { "gameId", game.Id } };

				if (player != null)
				{
					payload.Add("playerId", player.Id);
					devices.Remove(player.Id);
				}

				if (devices.Count > 0)
				{
					var instance = game.AppMode == AppMode.Dev ? PushService.Dev : PushService.Production;
					await instance.SendNotification(message, devices.ToArray(), payload);
				}
			}

			if (silentNotifyAllPlayers)
			{
				var allPlayers = game.GetAllPlayers();
				var allDevices = allPlayers.Where(pl => pl.Id != null && !devices.Contains(pl.Id)).Select(pl => pl.Id).ToList();
				var payload = new Dictionary<string, string> { { "gameId", game.Id } };

				if (player != null)
				{
					payload.Add("playerId", player.Id);
					allDevices.Remove(player.Id);
				}

				if (allDevices.Count > 0)
				{
					var instance = game.AppMode == AppMode.Dev ? PushService.Dev : PushService.Production;
					instance.SendSilentNotification(devices.ToArray(), payload);
				}
			}
		}

		#endregion
	}
}