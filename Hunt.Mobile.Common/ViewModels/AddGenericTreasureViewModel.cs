using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class AddGenericTreasureViewModel : BaseAddTreasureViewModel
	{
		string _treasureImageUrl;
		byte[] _photo;
		ImageSource _photoImageSource;
		public byte[] Photo
		{
			get { return _photo; }
			set
			{
				_photoImageSource = null;
				SetPropertyChanged(ref _photo, value);
				SetPropertyChanged(nameof(HeroImageSource));
			}
		}

		string[] _availableAttributes = new string[0];
		public string[] AvailableAttributes
		{
			get { return _availableAttributes; }
			set { SetPropertyChanged(ref _availableAttributes, value); }
		}

		List<string> _selectedAttributes = new List<string>();
		public List<string> SelectedAttributes
		{
			get { return _selectedAttributes; }
			set { SetPropertyChanged(ref _selectedAttributes, value); }
		}

		public ImageSource HeroImageSource
		{
			get
			{
				if(Photo != null)
					return _photoImageSource ?? (_photoImageSource = ImageSource.FromStream(() => new MemoryStream(Photo)));

				return null;
			}
		}

		public async Task<bool> AnalyzePhotoForAttributes()
		{
			using(var busy = new Busy(this, "Uploading photo"))
			{
				var url = await App.Instance.StorageService.SaveImage(Photo, Game.Id);

				if(url == null)
					return false;
				
				url = url.ToUrlCDN();
				Hud.Instance.HudMessage = "Analyzing photo";
				var task = new Task<string[]>(() => App.Instance.DataService.AnalyseImage(new [] { url }).Result);
				await task.RunProtected();

				if(!task.WasSuccessful() || task.Result == null)
					return false;
				
				var availableAttributes = task.Result;
				foreach(var a in availableAttributes)
					Log.Instance.WriteLine(a);

				_treasureImageUrl = url;
				AvailableAttributes = availableAttributes;
				return AvailableAttributes.Length > 0;
			}
		}

		public void AddAttribute(string text)
		{
			SelectedAttributes.Add(text);
		}

		public void RemoveAttribute(string text)
		{
			SelectedAttributes.Remove(text);
		}

		public async Task<bool> SaveTreasure()
		{
			var treasure = new Treasure
			{
				ImageSource = _treasureImageUrl,
				IsRequired = true,
				Points = Constants.PointsPerAttribute / SelectedAttributes.Count,
				Hint = Hint,
			};

			foreach(var attributeString in SelectedAttributes)
			{
				var attribute = new Hunt.Common.Attribute
				{
					 Name = attributeString,
					 ServiceType = CognitiveServiceType.Vision
				};

				treasure.Attributes.Add(attribute);
			}

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