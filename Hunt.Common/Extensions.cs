using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public static class Extensions
	{
		public static T Get<T>(this IList<T> list, string id) where T : BaseModel
		{
			return list.SingleOrDefault(t => t.Id == id);
		}

		public static string ToJson(this object o)
		{
			if(o == null)
				return null;
			
			return JsonConvert.SerializeObject(o);
		}

		public static T ToObject<T>(this string json)
		{
			if(string.IsNullOrWhiteSpace(json))
				return default(T);
			
			return JsonConvert.DeserializeObject<T>(json);
		}

		/// <summary>
		/// Checks to see if the current player's team has acquired all treasures
		/// </summary>
		public static bool EvaluateGameForWinner(this Game game, string teamId)
		{
			var team = game.Teams.SingleOrDefault(t => t.Id == teamId);
			if(team == null)
				return false;

			foreach(var t in game.Treasures)
			{
				var exists = team.AcquiredTreasure.Any(at => at.TreasureId == t.Id);
				if(!exists)
					return false;
			}

			return true;
		}

		public static Player[] GetAllPlayers(this Game game)
		{
			var list = new List<Player>();
			list.Add(game.Coordinator);

			var players = (from t in game.Teams
						   from p in t.Players
						   select p).ToArray();

			list.AddRange(players);
			return list.ToArray();
		}

		public static Player GetPlayer(this Game game, string id)
		{
			if (game.Coordinator?.Id.Equals(id, StringComparison.OrdinalIgnoreCase) == true)
				return game.Coordinator;

			foreach (var team in game.Teams)
				foreach (var player in team.Players)
					if (player.Id.Equals(id, StringComparison.OrdinalIgnoreCase) == true)
						return player;

			return null;
		}

		public static Team GetTeam(this Game game, Player player)
		{
			if (game == null || player == null)
				return null;

			return game.Teams.SingleOrDefault(t => t.Players.Exists(p => p.Id == player.Id));
		}

		public static bool IsCoordinator(this Game game, Player player)
		{
			if (game == null || player == null || player.Id == null)
				return false;

			return game.Coordinator.Id.Equals(player.Id, StringComparison.CurrentCultureIgnoreCase);
		}
	}
}