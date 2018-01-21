using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hunt.Common;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class AddCustomTreasureViewModel : BaseAddTreasureViewModel
	{
		public int MinimumPhotoCount = 5;
		public int MaximumPhotoCount = 10;
		ImageSource _photoImageSource;

		List<byte[]> _photos = new List<byte[]>();
		public List<byte[]> Photos
		{
			get { return _photos; }
			set
			{
				SetPropertyChanged(ref _photos, value);
			}
		}

		string _assignedTag;
		public string AssignedTags
		{
			get { return _assignedTag; }
			set { SetPropertyChanged(ref _assignedTag, value); }
		}

		public ImageSource HeroImageSource
		{
			get
			{
				if(Photos.Count > 0)
					return _photoImageSource ?? (_photoImageSource = ImageSource.FromStream(() => new MemoryStream(Photos[0])));

				return null;
			}
		}

		public async Task<bool> SaveTreasure()
		{
			var imageUrls = new List<string>();
			using(var busy = new Busy(this, "Uploading photo 1"))
			{
				int i = 1;
				foreach(var photo in Photos)
				{
					Hud.Instance.HudMessage = $"Uploading photo {i}";
					var url = await UploadPhotoToAzureStorage(photo);

					if(url == null)
					{
						Hud.Instance.ShowToast("Unable to upload all the photos.", NoticationType.Error);
						return false;
					}

					imageUrls.Add(url);
					i++;
				}

				var tags = new List<string>(AssignedTags.Split(',').ToList());
				while(tags.Count < 2)
					tags.Add("random_tag");

				for(int j = 0; j < tags.Count; j++)
					tags[j] = $"{tags[j].Trim()}{Guid.NewGuid().ToString().Split('-')[0]}";

				Hud.Instance.HudMessage = $"Training the classifier";
				var task = new Task<bool>(() => App.Instance.DataService.TrainClassifier(Game, imageUrls, tags.ToArray()).Result);
                await task.RunProtected(NotifyMode.Throw);

				if(!task.WasSuccessful() || !task.Result)
					return false;

				Hud.Instance.HudMessage = $"Adding the treasure";

				var treasure = new Treasure
				{
					ImageSource = imageUrls[0],
					IsRequired = true,
					Points = Constants.PointsPerAttribute,
					Hint = Hint,
				};

				foreach(var tag in tags)
				{
					var attribute = new Hunt.Common.Attribute
					{
						Name = tag,
						ServiceType = CognitiveServiceType.CustomVision,
					};

					treasure.Attributes.Add(attribute);
				}

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
				}

				return game != null;
			}
		}

		public async Task<string> UploadPhotoToAzureStorage(byte[] photo)
		{
			var url = await App.Instance.StorageService.SaveImage(photo, Game.Id);

			if(url == null)
				return null;

			return url.ToUrlCDN();
		}
	}
}