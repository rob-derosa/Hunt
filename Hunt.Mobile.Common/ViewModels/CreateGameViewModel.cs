using System.Threading.Tasks;
using Hunt.Common;
using System;
using System.Linq;

namespace Hunt.Mobile.Common
{
	public class CreateGameViewModel : BaseViewModel
	{
		#region Properties

		int _teamCount = 5;
		public int TeamCount
		{
			get { return _teamCount; }
			set { SetPropertyChanged(ref _teamCount, value); }
		}

		int _playerCount = 4;
		public int PlayerCount
		{
			get { return _playerCount; }
			set { SetPropertyChanged(ref _playerCount, value); }
		}

		int _gameLength = 60;
		public int GameLength
		{
			get { return _gameLength; }
			set { SetPropertyChanged(ref _gameLength, value); }
		}

		bool _startGame;
		public bool StartGame
		{
			get { return _startGame; }
			set { SetPropertyChanged(ref _startGame, value); }
		}

		int _gameLengthMinute = 60;
		int _minimumDuration = 5;
		int _increment = 5;
		public int GameLengthMinutes
		{
			get { return _gameLengthMinute; }
			set
			{
				SetPropertyChanged(ref _gameLengthMinute, value);
				double r = _gameLengthMinute / _increment;
				var near = Math.Round(r, 0);
				var length = (int)(near * _increment);
				GameLength = length >= _minimumDuration ? length : _minimumDuration;
			}
		}

		bool _isCoordinator = true;
		public bool IsCoordinator {
			get { return _isCoordinator; }
			set { SetPropertyChanged(ref _isCoordinator, value); } }

		bool _populateTreasure = true;
		public bool PopulateTreasure
		{
			get { return _populateTreasure; }
			set
			{
				SetPropertyChanged(ref _populateTreasure, value);
				PopulateAcquiredTreasure &= value;
			}
		}

		bool _populateAcquiredTreasure;
		public bool PopulateAcquiredTreasure
		{
			get { return _populateAcquiredTreasure; }
			set { SetPropertyChanged(ref _populateAcquiredTreasure, value); } }

		bool _populateTeams = true;
		public bool PopulateTeams
		{
			get { return _populateTeams; }
			set
			{
				SetPropertyChanged(ref _populateTeams, value);
				PopulateAcquiredTreasure &= value;
			}
		}

		#endregion

		async public Task<Game> CreateGameAsync()
		{
			var doStartGame = false;
			using(var busy = new Busy(this, $"Creating new game"))
			{
				var game = Mocker.GetGame(TeamCount, PlayerCount, !IsCoordinator, PopulateTeams, PopulateTreasure, PopulateAcquiredTreasure);
				game.CreateDate = DateTime.UtcNow;
				game.TeamCount = TeamCount;
				game.PlayerCountPerTeam = PlayerCount;
				game.Name = $"Hunt Game by {App.Instance.Player.Alias}";
				game.DurationInMinutes = GameLength;
				game.AppMode = AppMode.Production;

#if DEBUG
				game.AppMode = AppMode.Dev;
#endif

				if(IsCoordinator)
				{
					game.Coordinator = App.Instance.Player;
				}
				else
				{
					doStartGame = true;
					game.Teams.Single(t => t.Name.Equals("house stark", StringComparison.OrdinalIgnoreCase)).Players.Add(App.Instance.Player);
				}

				var task = new Task<Game>(() => App.Instance.DataService.SaveGame(game, GameUpdateAction.Create).Result);
				await task.RunProtected();

				if(!task.WasSuccessful())
					return null;

				if(doStartGame)
				{
					game = task.Result;
					task = new Task<Game>(() => App.Instance.DataService.SaveGame(game, GameUpdateAction.StartGame).Result);
					await task.RunProtected();
				}

				App.Instance.CurrentGame = task.Result;
				Log.Instance.WriteLine(task.Result.EntryCode);
				return task.Result;
			}
		}
	}
}