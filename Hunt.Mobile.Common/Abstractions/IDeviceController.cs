using System;
using System.Threading.Tasks;

namespace Hunt.Mobile.Common
{
	public interface IDeviceController
	{
		void SetOrientation(Orientation orientation);
	}

	public static class DeviceController
	{
		public static IDeviceController Instance { get; set; }
	}

	//public enum Orientation
	//{
	//	Landscape,
	//	Portrait,
	//}
}
