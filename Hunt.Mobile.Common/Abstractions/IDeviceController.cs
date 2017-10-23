using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public interface IDeviceController
	{
		void SetOrientation(Orientation orientation);
	}

	public static class DeviceController
	{
		static IDeviceController _instance;
		public static IDeviceController Instance => _instance ?? (_instance = DependencyService.Get<IDeviceController>());
	}

	//public enum Orientation
	//{
	//	Landscape,
	//	Portrait,
	//}
}
