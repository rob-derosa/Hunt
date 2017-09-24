using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class TreasureDetailsViewModel : BaseGameViewModel
	{
		public Action<Game> OnTreasureAcquired { get; set; }

		public bool CanSubmit => CanAcquire && Photo != null;
		public bool CanRemove => Game.IsCoordinator() && !Game.HasStarted && !IsBusy;
		public bool IsAcquired => AcquiredTreasure != null;
		public bool CanAcquire => !Game.IsCoordinator() && AcquiredTreasure == null && Game.IsRunning;

		Treasure _treasure;
		public Treasure Treasure
		{
			get { return _treasure; }
			set { SetPropertyChanged(ref _treasure, value); }
		}

		AcquiredTreasure _acquiredTreasure;
		public AcquiredTreasure AcquiredTreasure
		{
			get { return _acquiredTreasure ?? (_acquiredTreasure = Game.GetAcquiredTreasure(Treasure)); }
		}

		Player _player;
		public Player Player
		{
			get
			{
				if(AcquiredTreasure == null)
					return null;
				
				return _player ?? (_player = Game.GetPlayer(AcquiredTreasure.PlayerId));
			}
		}

		byte[] _photo;
		ImageSource _photoImageSource;
		public byte[] Photo
		{
			get { return _photo; }
			set
			{
				_photoImageSource = null;
				SetPropertyChanged(ref _photo, value);
				SetPropertyChanged(nameof(CanSubmit));
				SetPropertyChanged(nameof(HeroImageSource));
			}
		}

		public string AcquiredBy
		{
			get
			{
				if(AcquiredTreasure == null)
					return "Not acquired";

				return $"acquired {AcquiredTreasure.ClaimedTimeStampLocal.ToString("M/d 'at' h:mmtt ")} by {Player.Alias}";
			}
		}

		ImageSource _acquiredImageSource;
		ImageSource _treasureImageSource;
		public ImageSource HeroImageSource
		{
			get
			{
				//Either the player's teams picture or the winning team's picture
				if(AcquiredTreasure != null)
				{
					return _acquiredImageSource ?? (_acquiredImageSource = new UriImageSource
					{
						Uri = new Uri(AcquiredTreasure.ImageSource),
						CachingEnabled = true,
					});
				}

				//Draw so we show the original source image
				if(Game.IsCoordinator() || (Game.HasEnded && AcquiredTreasure == null))
				{
					return _treasureImageSource ?? (_treasureImageSource = new UriImageSource
					{
						Uri = new Uri(Treasure.ImageSource),
						CachingEnabled = true,
					});
				}

				//Player is attempting to acquire from camera so show those bytes
				if(Photo != null)
					return _photoImageSource ?? (_photoImageSource = ImageSource.FromStream(() => new MemoryStream(Photo)));

				return null;
			}
		}

		async public override Task<Game> RefreshGame(Game game)
		{
			var id = Treasure.Id;
			var refreshed = await base.RefreshGame(game);
			SetGame(refreshed);
			Treasure = refreshed.Treasures.SingleOrDefault(t => t.Id == id);

			if(Game.HasEnded || IsAcquired)
			{
				OnTreasureAcquired?.Invoke(Game);
			}

			return refreshed;
		}

		public override void NotifyPropertiesChanged()
		{
			base.NotifyPropertiesChanged();
			SetPropertyChanged(nameof(Treasure));
			SetPropertyChanged(nameof(Player));
			SetPropertyChanged(nameof(HeroImageSource));
			SetPropertyChanged(nameof(CanAcquire));
			SetPropertyChanged(nameof(CanSubmit));
			SetPropertyChanged(nameof(AcquiredTreasure));
			SetPropertyChanged(nameof(IsAcquired));
			SetPropertyChanged(nameof(AcquiredBy));
		}

		#region Remove / Edit

		public async Task<bool> RemoveTreasure()
		{
			using(var busy = new Busy(this, "Removing treasure from game"))
			{
				Func<Game, Game> action = (game) =>
				{
					game = game ?? Game;
					var clone = game.Clone();
					var tr = clone.Treasures.SingleOrDefault(t => t.Id == Treasure.Id);
					if(tr != null)
					{
						clone.Treasures.Remove(tr);
						return clone;
					}
					return null;
				};

				var success = await SaveGameSafe(action, GameUpdateAction.RemoveTreasure);
				return success != null;
			}
		}

		#endregion

		#region Acquisition

		string _treasureImageUrl;

		string[] _attributeResults = new string[0];
		public string[] AttributeResults
		{
			get { return _attributeResults; }
			set { SetPropertyChanged(ref _attributeResults, value); }
		}

		public bool CanContinue => !IsBusy && Photo != null;

		public async Task<bool> AnalyzePhotoForAcquisition()
		{
			if(Treasure == null)
				throw new Exception("Please specify a treasure");

			using(var busy = new Busy(this, "Uploading photo"))
			{
				var url = await App.Instance.StorageService.SaveImage(Photo, Game.Id);
				Logger.Instance.WriteLine(url);

				if(url == null)
					throw new Exception("There was an issue uploading the image. Please try again.");

				Hud.Instance.HudMessage = "Analyzing photo";
				var task = new Task<string[]>(() => App.Instance.DataService.AnalyseImage(new string[] { url }).Result);
				await task.RunProtected();

				if(!task.WasSuccessful() || task.Result == null)
					return false;

				AttributeResults = task.Result;
				foreach(var a in AttributeResults)
					Logger.Instance.WriteLine(a);

				_treasureImageUrl = url;
				var success = Treasure.Attributes.Count == GetMatchCount();

				if(success)
				{
					
					await AquireTreasureAndSaveGame(Treasure.Attributes.Count);
				}

				NotifyPropertiesChanged();
				return success;
			}
		}

		async public Task<bool> AquireTreasureAndSaveGame(int claimedAttributes)
		{
			var acquiredTreasure = new AcquiredTreasure
			{
				TreasureId = Treasure.Id,
				PlayerId = App.Instance.Player.Id,
				ImageSource = _treasureImageUrl,
				ClaimedTimeStamp = DateTime.UtcNow,
				ClaimedPoints = claimedAttributes * Keys.Constants.PointsPerAttribute,
			};

			Func<Game, Game> action = (refreshedGame) =>
			{
				refreshedGame = refreshedGame ?? Game;
				var clone = refreshedGame.Clone();
				var team = clone.GetTeam();
				if(team.AcquiredTreasure.Any(at => at.TreasureId == Treasure.Id))
					throw new TreasureAlreadyAcquiredException(Treasure);

				clone.GetTeam().AcquiredTreasure.Add(acquiredTreasure);
				return clone;
			};

			var args = new Dictionary<string, string> {
						{ "teamId", Game.GetTeam().Id },
						{ "acquiredTreasureId", acquiredTreasure.Id }};

			var game = await SaveGameSafe(action, GameUpdateAction.AcquireTreasure, args);
			SetGame(game);

			return game != null;
		}

		public int GetMatchCount()
		{
			int matches = 0;
			foreach(var att in Treasure.Attributes)
			{
				Logger.Instance.WriteLine(att.Name);
				var found = AttributeResults.Any(a => att.Name.Equals(a, StringComparison.OrdinalIgnoreCase));
				if(found)
					matches++;
			}

			return matches;
		}

		public void Reset()
		{
			Photo = null;
			AttributeResults = null;
		}

		#endregion
	}

	public class TreasureAlreadyAcquiredException : Exception
	{
		public Treasure Treasure { get; set; }
		public TreasureAlreadyAcquiredException(Treasure t) : base("Someone on your team already acquired this treasure. You can move onto the next one.")
		{
			Treasure = t;
		}
	}
}