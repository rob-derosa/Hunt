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
	public partial class AddCustomTreasureImagesPage : BaseContentPage<AddCustomTreasureViewModel>
	{
		public AddCustomTreasureImagesPage()
		{
			if(IsDesignMode)
			{
				ViewModel.SetGame(App.Instance.CurrentGame);
			}

			Initialize();
		}

		public AddCustomTreasureImagesPage(Game game)
		{
			ViewModel.SetGame(game);
			Initialize();
		}

		void Initialize()
		{
			InitializeComponent();
			DrawPhotoGrid();
		}

		#region Photo Grid

		void DrawPhotoGrid()
		{
			rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
			rootGrid.ColumnDefinitions.Add(new ColumnDefinition());
			var rowIndex = rootGrid.RowDefinitions.Count;
			var height = ((App.Instance.ScreenSize.Width / 2) - 8) * 1.333333333333;

			for(int i = 0; i < ViewModel.MaximumPhotoCount; i++)
			{
				if(i % 2 == 0)
					rootGrid.RowDefinitions.Add(new RowDefinition { Height = height });

				var isLeft = i % 2 == 0;

				var grid = new Grid
				{
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Margin = isLeft ? new Thickness(6, 0, 0, 0) : new Thickness(0, 0, 6, 0),
				};
				Grid.SetRow(grid, rowIndex);
				Grid.SetColumn(grid, i % 2);

				var camera = new SvgImage
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Source = "camera.svg",
					Opacity = .15f,
				};

				var button = new Button
				{
					FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
					FontAttributes = FontAttributes.Bold,
					CommandParameter = i,
					BackgroundColor = i < ViewModel.MinimumPhotoCount ? Color.FromHex("#03FFFFFF") : Color.FromHex("#2000"),
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					Margin = new Thickness(0),
					AutomationId = $"tag{i}",
					BorderColor = Color.FromHex("#4FFF"),
					BorderWidth = i < ViewModel.MinimumPhotoCount ? .5 : 0,
				};

				button.Clicked += TakePhotoClicked;
				grid.Children.Add(camera);
				grid.Children.Add(button);
				rootGrid.Children.Add(grid);

				if(i % 2 == 1)
					rowIndex++;
			}

			var submit = new Button
			{
				Text = "Submit",
				VerticalOptions = LayoutOptions.End,
			};

			rootGrid.RowDefinitions.Add(new RowDefinition { Height = submit.HeightRequest });
			rootGrid.Children.Add(submit);
			Grid.SetRow(submit, rootGrid.RowDefinitions.Count - 1);
			Grid.SetColumnSpan(submit, 2);

			submit.Clicked += SubmitClicked;
		}

		#endregion

		async void SubmitClicked(object sender, EventArgs e)
		{
			if(ViewModel.Photos.Count < ViewModel.MinimumPhotoCount)
			{
				Hud.Instance.ShowToast($"Please snap at least {ViewModel.MinimumPhotoCount} photos and up to {ViewModel.MaximumPhotoCount}");
				return;
			}

			var page = new AddCustomTreasurePage(ViewModel);
			await Navigation.PushAsync(page);
		}

		#region Take Photo

		async void TakePhotoClicked(object sender, EventArgs e)
		{
			var btn = sender as Button;
			var photo = await TakePhotoAsync();

			if(photo == null)
				return;

			var image = new Image
			{
				HeightRequest = btn.Height,
				WidthRequest = btn.Width,
				Source = ImageSource.FromStream(() => new MemoryStream(photo)),
			};

			var index = (int)btn.CommandParameter;
			Grid.SetRow(image, (int)Math.Floor((decimal)(index / 2)));
			Grid.SetColumn(image, index % 2);
			rootGrid.Children.Add(image);
			ViewModel.Photos.Add(photo);
		}

		async public Task<byte[]> TakePhotoAsync()
		{
			if(!CrossMedia.Current.IsCameraAvailable)
			{
				//Consider allowing selection from photo library, which is simulator-friendly
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

		#endregion
	}
}