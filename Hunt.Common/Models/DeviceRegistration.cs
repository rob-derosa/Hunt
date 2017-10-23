using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hunt.Common
{
	public class DeviceRegistration
	{
		public string Platform { get; set; }
		public string Handle { get; set; }
		public string[] Tags { get; set; }
		public AppMode AppMode { get; set; }
	}
}
