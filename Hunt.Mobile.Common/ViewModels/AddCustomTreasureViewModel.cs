using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class AddCustomTreasureViewModel : BaseGameViewModel
	{
		string _treasureImageUrl;
		public Action<Game> OnTreasureAdded { get; set; }

		string _hint;
		public string Hint
		{
			get { return _hint; }
			set { SetPropertyChanged(ref _hint, value); SetPropertyChanged(nameof(CanContinue)); }
		}

		List<byte[]> _photos;
		public List<byte[]> Photos
		{
			get { return _photos; }
			set
			{
				SetPropertyChanged(ref _photos, value);
			}
		}

		List<string> _assignedTags = new List<string>();
		public List<string> AssignedTags
		{
			get { return _assignedTags; }
			set { SetPropertyChanged(ref _assignedTags, value); }
		}

		public ImageSource HeroImageSource
		{
			get
			{
				//if(Photo != null)
					//return _photoImageSource ?? (_photoImageSource = ImageSource.FromStream(() => new MemoryStream(Photo)));

				return null;
			}
		}

		public bool CanContinue
		{
			get { return Photos.Count > 0 && !string.IsNullOrWhiteSpace(Hint); }
		}

		public async Task<bool> AnalyzePhotoForAttributes()
		{
			using(var busy = new Busy(this, "Uploading photo"))
			{
				//var url = await App.Instance.StorageService.SaveImage(Photo, Game.Id);

				//if(url == null)
				//	return false;

				//url = url.ToUrlCDN();
				//Hud.Instance.HudMessage = "Analyzing photo";
				//var task = new Task<string[]>(() => App.Instance.DataService.AnalyseImage(new [] { url }).Result);
				//await task.RunProtected();

				//if(!task.WasSuccessful() || task.Result == null)
				//	return false;

				//var availableAttributes = task.Result;
				//foreach(var a in availableAttributes)
				//	Log.Instance.WriteLine(a);

				//_treasureImageUrl = url;
				//AvailableAttributes = availableAttributes;
				//return AvailableAttributes.Length > 0;
				return true;
			}
		}

		public async Task<bool> SaveTreasure()
		{
			var treasure = new Treasure
			{
				ImageSource = _treasureImageUrl,
				IsRequired = true,
				Points = Constants.PointsPerAttribute,
				Hint = Hint,
			};

			//foreach(var attributeString in SelectedAttributes)
			//{
			//	var attribute = new Hunt.Common.Attribute
			//	{
			//		 Name = attributeString,
			//		 ServiceType = CognitiveServiceType.Vision
			//	};

			//	treasure.Attributes.Add(attribute);
			//}

			using(var busy = new Busy(this, "Adding treasure"))
			{
				Func<Game, Game> action = (refreshedGame) =>
				{
					refreshedGame = refreshedGame ?? Game;
					var clone = refreshedGame.Clone();
					clone.Treasures.Add(treasure);
					return clone;
				};

				var game = await SaveGameSafe(action, GameUpdateAction.AddTreasure);

				if(game != null)
				{
					SetGame(game);
					OnTreasureAdded?.Invoke(Game);
					Hud.Instance.ShowToast("Treasure successfully added.");
				}

				return game != null;
			}
		}
	}
}