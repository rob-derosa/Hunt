using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hunt.Common;
using Newtonsoft.Json;
using PCLCrypto;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public static class Extensions
	{
		#region Generic

		public static string ToUrlCDN(this string url)
		{
			return url.Replace(Keys.Constants.BlobBaseUrl, Keys.Constants.CdnBaseUrl);
		}

		public static void Notify(this Exception e)
		{
			Logger.Instance.WriteLine(e.Message);
			Hud.Instance.ShowToast(e.Message, NoticationType.Error);
		}

		public static T Clone<T>(this T model) where T : BaseModel
		{
			return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(model));
		}

		public static string GenerateHash(this string email)
		{
			var str = email.Trim().ToLower();
			//convert string to byte array
			var bytestr = System.Text.Encoding.UTF8.GetBytes(str);

			//has byte string
			var algorithm = HashAlgorithm.Md5;
			var sha = WinRTCrypto.HashAlgorithmProvider.OpenAlgorithm((algorithm));
			var hash = sha.HashData(bytestr);

			return ByteArrayToString(hash);
		}

		public static string ByteArrayToString(byte[] arr)
		{
			//convert hash result into usable string
			var sbuilder = new StringBuilder();
			for(int i = 0; i < arr.Length; i++)
			{
				sbuilder.Append(arr[i].ToString("x2"));
			}

			return sbuilder.ToString();
		}

		public static string ToTitleCase(this string value)
		{
			return $"{value.Substring(0, 1).ToUpper()}{value.Substring(1).ToLower()}";
		}

		public static string ToGravatarUrl(this string email, int? size = null)
		{
			var split = email.Split('@');
			if(split.Length == 2)
			{
				if(split[1].ToLower() == "hbo.com")
					return $"https://huntapp.azureedge.net/images/avatars/{split[0].ToLower()}.jpg";
			}

			var hashStr = email.GenerateHash();
			var url = $"https://www.gravatar.com/avatar/{hashStr}.jpg";

			if(size != null)
				url = $"{url}?s={size}";

			Logger.Instance.WriteLine(url);
			return url;
		}

		public static string GetFileContents(this string fileName)
		{
			var assembly = typeof(App).GetTypeInfo().Assembly;
			var name = assembly.ManifestModule.Name.Replace(".dll", string.Empty);
			var stream = assembly.GetManifestResourceStream($"{name}.Resources.{fileName}");

			if(stream == null)
				return null;

			string content;
			using(var reader = new StreamReader(stream))
			{
				content = reader.ReadToEnd();
			}

			return content;
		}

		#endregion

		async public static Task PopAsyncAndNotify(this INavigation nav)
		{
			if(nav.NavigationStack.Count >= 2)
			{
				var prevPage = nav.NavigationStack[nav.NavigationStack.Count - 2];

				if(prevPage is IHuntContentPage)
				{
					var bcp = prevPage as IHuntContentPage;
					bcp.OnBeforePoppedTo();
				}
			}

			await nav.PopAsync();
		}

		#region Page

		public static void RemoveSecondToLastPage(this INavigation page)
		{
			page.RemovePage(page.NavigationStack[page.NavigationStack.Count - 2]);
		}

		async public static Task PopToDashboard(this INavigation page)
		{
			while(!(page.NavigationStack[page.NavigationStack.Count - 2] is DashboardPage))
			{
				page.RemoveSecondToLastPage();
			}

			await page.PopAsyncAndNotify();
		}

		async public static Task SignOutPlayer(this Page page)
		{
			Settings.Player = null;
			App.Instance.Player = null;
			App.Instance.CurrentGame = null;
			await page.Navigation.PopToRootAsync();
		}

		public static NavigationPage ToNav(this ContentPage page)
		{
			return new NavigationPage(page)
			{
				BarTextColor = Color.White,
			};
		}

		static string[] _allColors;
		public static void DebugLayout(this ContentPage page)
		{
			if(_allColors == null)
			{
				_allColors = new string[] {"#ffc0cb","#008080","#ffe4e1","#ff0000","#ffd700","#d3ffce","#00ffff","#40e0d0","#ff7373","#0000ff","#e6e6fa","#ffa500","#eeeeee","#f0f8ff","#b0e0e6","#cccccc","#7fffd4","#333333","#faebd7","#c0c0c0","#003366","#fa8072","#20b2aa","#ffb6c1","#800080","#00ff00","#f6546a","#c6e2ff","#666666","#f08080","#ffff00","#468499","#fff68f","#088da5","#ff6666","#ffc3a0","#00ced1","#66cdaa","#800000","#f5f5f5","#660066","#ff00ff","#008000","#c39797","#ff7f50","#c0d6e4","#ffdab9","#990000","#cbbeb5","#dddddd","#0e2f44","#daa520","#808080","#8b0000","#b4eeb4","#afeeee","#ffff66","#f5f5dc","#81d8d0","#66cccc","#00ff7f","#ff4040","#999999","#b6fcd5","#cc0000","#8a2be2","#ccff00","#3399ff","#a0db8e","#794044","#3b5998","#f7f7f7","#0099cc","#ff4444","#6897bb","#31698a","#6dc066","#000080","#191970","#191919","#404040","#4169e1"};
			}

			ColorView(page.Content);
		}

		static int _index;
		static void ColorView(View view)
		{
			if(view == null)
				return;

			try
			{
				_index++;

				if(_index >= _allColors.Length)
					_index = 0;
				
				var color = Color.FromHex(_allColors[_index]);
				//view.BackgroundColor = color.MultiplyAlpha(.1);

				if(view.BackgroundColor == Color.Default)
					view.BackgroundColor = Color.FromHex("#11FFFFFF");

				var layout = view as ILayoutController;
				if(layout == null)
					return;

				foreach(var child in layout.Children)
				{
					var childView = child as View;
					if(childView == null)
						continue;

					ColorView(childView);
				}
			}catch{}
		}

		#endregion

		#region Game

		public static Team GetHighestScoringTeam(this Game game)
		{
			return game.Teams.OrderByDescending(t => t.TotalPoints).FirstOrDefault();
		}

		public static bool AbandonGame(this Game game)
		{
			return game.AbandonGame(App.Instance.Player);
		}

		/// <summary>
		/// Removes the player from their team, if one is found (local only)
		/// </summary>
		/// <returns><c>true</c>, if game was abandoned, <c>false</c> otherwise.</returns>
		/// <param name="game">Game.</param>
		/// <param name="player">Player to remove.</param>
		public static bool AbandonGame(this Game game, Player player)
		{
			if(game.IsCoordinator(player))
			{
				if(player != App.Instance.Player)
					throw new Exception("Only coordinator can delete the game");

				game.EndDate = DateTime.UtcNow;
				return true;
			}

			var team = game.GetTeam(player);

			if(team == null)
				return true;

			var toRemove = team.Players.Single(p => p.Email == player.Email);
			team.Players.Remove(toRemove);
			return true;
		}

		public static bool JoinTeam(this Game game, string teamId)
		{
			return game.JoinTeam(teamId, App.Instance.Player);
		}

		public static bool JoinTeam(this Game game, string teamId, Player player)
		{
			if(!game.CanJoinTeam(teamId, player))
				return false;

			var team = game.Teams.SingleOrDefault(t => t.Id == teamId);
			team.Players.Add(player);
			return true;
		}

		public static Team GetWinningTeam(this Game game)
		{
			if(game == null || game.Teams == null)
				return null;
			
			return game.Teams.SingleOrDefault(t => t.Id == game.WinnningTeamId);
		}

		public static Team GetTeam(this Game game)
		{
			return game.GetTeam(App.Instance.Player);
		}

		public static Team GetTeam(this Game game, Player player)
		{
			if(game == null || player == null)
				return null;

			return game.Teams.SingleOrDefault(t => t.Players.Exists(p => p.Email == player.Email));
		}

		public static Player GetPlayer(this Game game)
		{
			return game.GetPlayer(App.Instance.Player?.Id);
		}

		public static Player GetPlayer(this Game game, string id)
		{
			if(game.Coordinator?.Id.Equals(id, StringComparison.OrdinalIgnoreCase) == true)
				return game.Coordinator;

			foreach(var team in game.Teams)
				foreach(var player in team.Players)
					if(player.Id.Equals(id, StringComparison.OrdinalIgnoreCase) == true)
						return player;

			return null;
		}

		public static Player GetPlayerByEmail(this Game game)
		{
			return game.GetPlayerByEmail(App.Instance.Player?.Email);
		}

		public static Player GetPlayerByEmail(this Game game, string email)
		{
			if(game.Coordinator?.Email.Equals(email, StringComparison.OrdinalIgnoreCase) == true)
				return game.Coordinator;

			foreach(var team in game.Teams)
				foreach(var player in team.Players)
					if(player.Email.Equals(email, StringComparison.OrdinalIgnoreCase) == true)
						return player;

			return null;
		}

		public static bool IsCoordinator(this Game game)
		{
			return game.IsCoordinator(App.Instance.Player);
		}

		public static bool CanJoinTeam(this Game game, string teamId)
		{
			return game.CanJoinTeam(teamId, App.Instance.Player);
		}

		public static string GetCannotJoinReason(this Game game, string teamId)
		{
			return GetCannotJoinReason(game, teamId, App.Instance.Player);
		}

		public static string GetCannotJoinReason(this Game game, string teamId, Player player)
		{
			var existingTeam = game.GetTeam(player);

            if (existingTeam != null) //Player already belongs to a team
                return "You already belong to another team";

			var team = game.Teams.SingleOrDefault(t => t.Id == teamId);
			if(team == null)
				return "Team no longer exists";

			if(team.Players.Count == game.PlayerCountPerTeam)
				return "Team is already full";

			return null;
		}

		public static bool CanJoinTeam(this Game game, string teamId, Player player)
		{
			return game.GetCannotJoinReason(teamId, player) == null;
		}

		#endregion

		#region Treasure

		public static bool IsAcquired(this Game game, Treasure treasure)
		{
			return game.GetAcquiredTreasure(treasure) != null;
		}

        public static bool HasMinimumPlayers(this Game game)
        {
			return game.Teams.Where(t => t.Players.Count > 0).Count() >= 2;
        }

		public static AcquiredTreasure GetAcquiredTreasure(this Game game, Treasure treasure)
		{
			if(game == null)
				return null;
			
			if(game.HasEnded)
			{
				//Return the winning team's acquired treasure
				var team = game.GetWinningTeam();

				if(team != null)
					return team.AcquiredTreasure.SingleOrDefault(t => t.TreasureId == treasure.Id);
			}

			if(game.IsCoordinator())
				return null;

			return game.GetTeam().AcquiredTreasure.SingleOrDefault(t => t.TreasureId == treasure.Id);
		}

		#endregion
	}
}