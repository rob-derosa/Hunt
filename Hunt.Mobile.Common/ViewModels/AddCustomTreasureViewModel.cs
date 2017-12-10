using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class AddCustomTreasureViewModel : BaseAddTreasureViewModel
	{
		public int MinimumPhotoCount = 3;
		string _treasureImageUrl;
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
		public string AssignedTag
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