using System;
using Xamarin.Forms;
using XFGloss;

namespace Hunt.Mobile.Common
{
	public partial class FullSizeImagePage : BaseContentPage<BaseViewModel>
	{
		public FullSizeImagePage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			DeviceController.Instance.SetOrientation(Orientation.Landscape);
			base.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			DeviceController.Instance.SetOrientation(Orientation.Portrait);
			base.OnDisappearing();
		}

		public FullSizeImagePage(ImageSource source)
		{
			InitializeComponent();
			image.Source = source;
		}
	}
}