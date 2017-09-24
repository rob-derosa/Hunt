using System;
using System.IO;
using System.Threading.Tasks;
using Hunt.Common;
using Lottie.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class AddTreasurePage : BaseContentPage<AddTreasureViewModel>
	{
		bool _isWaitingForLandscape;

		public AddTreasurePage()
		{
			if(IsDesignMode)
			{
				ViewModel.SetGame(App.Instance.CurrentGame);
			}

			InitializeComponent();
		}

		public AddTreasurePage(Game game)
		{
			ViewModel.SetGame(game);
			InitializeComponent();
		}

		void TakePhotoClicked(object sender, EventArgs e)
		{
			#if !DEBUG
			if(Orientation != Orientation.Landscape)
			{
				_isWaitingForLandscape = true;
				Hud.Instance.Show("Please take all photos in landscape mode.\n\nThank you, kindly.");
			}
			else
			#endif
			{
				DisplayCameraView();
			}
		}

		protected override void OnOrientationChanged(Orientation orientation)
		{
			base.OnOrientationChanged(orientation);

			if(orientation == Orientation.Landscape)
			{
				if(_isWaitingForLandscape)
				{
					_isWaitingForLandscape = false;
					Hud.Instance.Dismiss();
					DisplayCameraView();
				}
			}
		}

		async void DisplayCameraView()
		{
			ViewModel.Photo = await TakePhotoAsync();
		}

		async void SubmitClicked(object sender, EventArgs e)
		{
			if(ViewModel.Photo == null)
			{
				Hud.Instance.ShowToast("Please take a photo of an object");
				return;
			}

			if(string.IsNullOrWhiteSpace(ViewModel.Hint))
			{
				Hud.Instance.ShowToast("Please enter a hint");
				return;
			}

			var success = await ViewModel.AnalyzePhotoForAttributes();
			if(success)
			{
				var page = new AssignAttributesPage(ViewModel);
				await Navigation.PushAsync(page);
			}
			else
			{
				Hud.Instance.ShowToast($"No objects were able to be identified in the photo. Please take another photo.");
				ViewModel.Photo = null;
			}
		}

		async public Task<byte[]> TakePhotoAsync()
		{
			if(!CrossMedia.Current.IsCameraAvailable)
			{
				Hud.Instance.ShowToast("This device does not have a supported camera.");
				return null;
			}

			var options = new StoreCameraMediaOptions
			{
				CompressionQuality = 50,
				PhotoSize = PhotoSize.Medium,
			};

			var file = await CrossMedia.Current.TakePhotoAsync(options);

			if(file == null)
				return null;

			var input = file.GetStream();
			byte[] buffer = new byte[16 * 1024];
			using(var ms = new MemoryStream())
			{
				int read;
				while((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}

				return ms.ToArray();
			}
		}
	}
}