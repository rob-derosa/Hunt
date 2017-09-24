using Xamarin.Forms;
using Hunt.Mobile.Common;
using Android.Content.PM;
using Android.App;

[assembly: Dependency(typeof(Hunt.Mobile.Android.DeviceController))]

namespace Hunt.Mobile.Android
{
	public class DeviceController : IDeviceController
	{
		public void SetOrientation(Orientation orientation)
		{
			var value = orientation == Orientation.Landscape ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;
			((Activity)Forms.Context).RequestedOrientation = value;
		}
	}
}