using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hunt.Common;
using Microsoft.Azure.Mobile.Push;

namespace Hunt.Mobile.Common
{
	public class TeamListViewModel : BaseViewModel
	{
		Game _game;
		public Game Game
		{
			get { return _game; }
			set { SetPropertyChanged(ref _game, value); NotifyPropertiesChanged(); }
		}

		async public Task<Game> JoinTeam(string teamId)
		{
			var toJoin = Game.Teams.SingleOrDefault(t => t.Id == teamId);

			if (toJoin == null)
				return null;

			var fullMsg = $"{toJoin.Name} is full.\nPlease choose another team.";
			if(toJoin.Players.Count >= Game.PlayerCountPerTeam)
			{
				Hud.Instance.ShowToast(fullMsg, NoticationType.Error);
				return null;
			}

			using(var busy = new Busy(this, "Joining team"))
			{
				Team team = null;
				Func<Game, Game> gameUpdateLogic = (game) =>
				{
					game = game ?? Game;
					var clone = game.Clone();
					if(clone.JoinTeam(teamId))
					{
						team = clone.GetTeam();
						return clone;
					}

					Game = game;
					InvokeRefreshedGame();
					Hud.Instance.ShowToast(fullMsg, NoticationType.Error);
					return null;
				};

				var savedGame = await SaveGameSafe(gameUpdateLogic, GameUpdateAction.JoinTeam,
					new Dictionary<string, string> { { "teamId", teamId } });

				if(savedGame == null)
					return null;

				var savedTeam = savedGame.Teams.SingleOrDefault(t => t.Id == teamId);
				if (savedTeam == null)
					return null;
				
				Game = savedGame;
				InvokeRefreshedGame();
				var playerExists = savedTeam.Players.Exists(p => p.Email == App.Instance.Player.Email);

				//if(playerExists)
				//{
				//	Hud.Instance.ShowToast($"You are now part of {savedTeam.Name}", NoticationType.Success);
				//}

				return playerExists ? savedGame : null;
			}
		}

		async public override void OnNotificationReceived(NotificationEventArgs args)
		{
			base.OnNotificationReceived(args);
			Game = await RefreshGame(Game);
		}

		public override void NotifyPropertiesChanged()
		{
			base.NotifyPropertiesChanged();
		}
	}
}