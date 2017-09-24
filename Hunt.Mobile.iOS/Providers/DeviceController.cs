using Xamarin.Forms;
using Hunt.Mobile.Common;
using UIKit;
using Foundation;

[assembly: Dependency(typeof(Hunt.Mobile.iOS.DeviceController))]

namespace Hunt.Mobile.iOS
{
	public class DeviceController : IDeviceController
	{
		public static UIInterfaceOrientationMask OrientationMask;

		public void SetOrientation(Orientation orientation)
		{
			UIInterfaceOrientation io = UIInterfaceOrientation.Unknown;
			switch(orientation)
			{
				case Orientation.Portrait:
					io = UIInterfaceOrientation.Portrait;
					OrientationMask = UIInterfaceOrientationMask.Portrait;
					break;

				case Orientation.Landscape:
					io = UIInterfaceOrientation.LandscapeRight;
					OrientationMask = UIInterfaceOrientationMask.LandscapeRight;
					break;

				case Orientation.All:
					OrientationMask = UIInterfaceOrientationMask.All;
					break;

			}

			UIApplication.SharedApplication.StatusBarOrientation = io;
			UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)io), new NSString("orientation"));
		}
	}
}