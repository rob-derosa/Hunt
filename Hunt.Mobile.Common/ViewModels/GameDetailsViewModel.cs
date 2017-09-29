using System.Threading.Tasks;
using Hunt.Common;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Hunt.Mobile.Common
{
	public class GameDetailsViewModel : BaseGameViewModel
	{
		#region Properties

		public bool CanAddTreasure => Game.IsCoordinator() && Game.IsPrepping;
		public bool CanShareGame => !Game.HasEnded;
		public bool CanLeaveGame => (Game.IsCoordinator() || Game.GetTeam() != null && !Game.HasEnded);
		public bool CanStartGame => Game.IsPrepping && Game.IsCoordinator() && Game.Treasures.Count() > 1 && Game.HasMinimumPlayers();
		public bool CanEndGame => Game.IsCoordinator() && Game.IsRunning;
		public int PlayerCount => Game == null ? 0 : Game.Teams.Sum(t => t.Players.Count);

		List<TreasureViewModel> _treasureViewModels;
		public List<TreasureViewModel> TreasureViewModels
		{
			get
			{
				if (_treasureViewModels == null && Game != null)
				{
					_treasureViewModels = new List<TreasureViewModel>();
					foreach (var treasure in Game.Treasures)
						_treasureViewModels.Add(new TreasureViewModel(Game, treasure));
				}

				return _treasureViewModels;
			}
		}

		public Team MyTeam
		{
			get { return Game.GetTeam(); }
		}

		public Team WinningTeam
		{
			get
			{
				if(Game.WinnningTeamId == null)
					return null;
			
				return Game.Teams.SingleOrDefault(t => t.Id == Game.WinnningTeamId);
			}
		}

		public string EmptyTreasureMessage
		{
			get
			{
				return Game.IsCoordinator() ? "Once you add some treasures to hunt, " +
						   "they will appear here.\n\nYou can get started by " +
						   "clicking the + button to the bottom right." :
						   "The coordinator of this game has not added any treasures to " +
						   "hunt for quite yet.";
			}
		}

		public string EndGameResult
		{
			get
			{
				if(WinningTeam != null)
					return $"{WinningTeam.Name} is victorious!";

				return "This game ended in a draw.";
			}
		}

		public string TimeRemaining
		{
			get
			{
				if(Game == null)
					return null;

                var gameDuration = TimeSpan.FromMinutes(Game.DurationInMinutes);
                if(Game.HasStarted)
				{
                    if(Game.HasEnded)
						return "0:00:00";

					var diff = DateTime.UtcNow.Subtract(Game.StartDate.Value);
                    var remaining = gameDuration.Subtract(diff);

					if(remaining.Ticks < 0)
						remaining = TimeSpan.FromTicks(0);

                    return remaining.ToString(@"hh\:mm\:ss");
				}
				else
				{
                    return gameDuration.ToString(@"hh\:mm\:ss");
				}
			}
		}

		public int PointsPercentage
		{
			get
			{
				if(!Game.HasEnded && Game.IsCoordinator())
					return 100;

				var team = Game.HasEnded ? Game.GetWinningTeam() : Game.GetTeam();

				if(team == null)
					team = Game.GetHighestScoringTeam();

				if(team == null)
					return 0;

				float p = (float)team.TotalPoints / (float)Game.TotalPoints;
				return (int)(p * 100);
			}
		}

		public int TreasurePercentage
		{
			get
			{
				if(!Game.HasEnded && Game.IsCoordinator())
					return 100;

				var team = Game.HasEnded ? Game.GetWinningTeam() : Game.GetTeam();

				if(team == null)
					team = Game.GetHighestScoringTeam();

				if(team == null)
					return 0;

				float p = (float)team.AcquiredTreasure.Count / (float)Game.Treasures.Count();
				return (int)(p * 100);
			}
		}

		public string EndGameMessage
		{
			get
			{
				if(!Game.HasEnded)
					return null;
				
				if(Game.IsCoordinator())
					return "Thanks for hosting!";

				if(Game.GetTeam().Id == Game.WinnningTeamId)
					return "Well Played!";

				return "Better luck next time.";
			}
		}

		#endregion

		public override void SetGame(Game game)
		{
			_treasureViewModels = null;
			base.SetGame(game);
			InvokeRefreshedGame();
		}

		#region Start/Stop Game

		async public Task<bool> StartGame()
        {
			if(Game.HasStarted)
				return true;

			if(Game.Treasures.Count == 0)
			{
				Hud.Instance.ShowToast("Please ensure you have added at least one treasure to hunt for.");
				return false;
			}

			if(!Game.HasMinimumPlayers())
			{
				Hud.Instance.ShowToast("Please ensure there are at least at least 2 teams with players.");
				return false;
			}

			using(var busy = new Busy(this, "Starting game"))
			{
				Func<Game, Game> action = (refreshedGame) =>
				{
					refreshedGame = refreshedGame ?? Game;
					var clone = refreshedGame.Clone();
					return clone;
				};

				var task = new Task<Game>(() => { return SaveGameSafe(action, GameUpdateAction.StartGame).Result; });
				await task.RunProtected();

				if(!task.WasSuccessful())
					return false;

				SetGame(task.Result);
				return true;
			}
		}

		async public Task<bool> EndGame()
		{
			if(Game.HasEnded)
				return true;

			using(var busy = new Busy(this, "Ending game"))
			{
				Func<Game, Game> action = (refreshedGame) =>
				{
					refreshedGame = refreshedGame ?? Game;
					var clone = refreshedGame.Clone();
					return clone;
				};

				var task = new Task<Game>(() => { return SaveGameSafe(action, GameUpdateAction.EndGame).Result; });
				await task.RunProtected();

				if(!task.WasSuccessful())
					return false;

				SetGame(task.Result);
				App.Instance.CurrentGame = null;
				return true;
			}
		}

		#endregion

		async public Task<bool> LeaveGame()
		{
			using(var busy = new Busy(this, "Leaving game"))
			{
				var isCoordinator = Game.IsCoordinator();
				var teamId = Game.GetTeam()?.Id;
				Game saved = Game;

				Game gameWithoutMe = null;
				Func<Game, Game> action = (refreshedGame) =>
				{
					refreshedGame = refreshedGame ?? Game;
					gameWithoutMe = refreshedGame.Clone();
					gameWithoutMe.AbandonGame();
					return gameWithoutMe;
				};

				Game savedGame = null;
				if(isCoordinator)
				{
					savedGame = await SaveGameSafe(action, GameUpdateAction.EndGame);
				}
				else
				{
					savedGame = await SaveGameSafe(action, GameUpdateAction.LeaveTeam,
						new Dictionary<string, string> { { "playerAlias", App.Instance.Player.Alias }, { "teamId", teamId } });
				}

				if(savedGame == null)
					return false;

				//Verify player has been removed
				var success = savedGame.GetTeam(App.Instance.Player) == null;
				App.Instance.CurrentGame = null;

				return success;
			}
		}

		public override void NotifyPropertiesChanged()
		{
			try
			{
				base.NotifyPropertiesChanged();
				SetPropertyChanged(nameof(WinningTeam));
				SetPropertyChanged(nameof(PlayerCount));
				SetPropertyChanged(nameof(PointsPercentage));
				SetPropertyChanged(nameof(TreasurePercentage));
				SetPropertyChanged(nameof(MyTeam));
				SetPropertyChanged(nameof(CanLeaveGame));
				SetPropertyChanged(nameof(CanShareGame));
				SetPropertyChanged(nameof(CanStartGame));
				SetPropertyChanged(nameof(CanEndGame));
				SetPropertyChanged(nameof(TimeRemaining));
				SetPropertyChanged(nameof(CanAddTreasure));
				SetPropertyChanged(nameof(TreasureViewModels));
				SetPropertyChanged(nameof(EmptyTreasureMessage));
				SetPropertyChanged(nameof(EndGameResult));
				SetPropertyChanged(nameof(EndGameMessage));
			} catch {}
		}
	}
}