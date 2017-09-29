using System;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class ImageButton : ContentView
	{
		public event EventHandler Clicked;
		Button _button;
		Image _image;
		Grid _rootGrid;

		ImageSource _imageSource;
		public ImageSource ImageSource
		{
			get { return _imageSource; }
			set
			{
				_imageSource = value;
				_image.Source = value;
			}
		}

		public ImageButton()
		{
			_rootGrid = new Grid();

			_image = new Image
			{
				Source = ImageSource,
				Aspect = Aspect.AspectFill,
			};

			_button = new Button
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Transparent,
			};
			_button.Clicked += (sender, e) => Clicked?.Invoke(this, new EventArgs());

			_rootGrid.Children.Add(_image);
			_rootGrid.Children.Add(_button);
			Content = _rootGrid;
		}
	}
}
