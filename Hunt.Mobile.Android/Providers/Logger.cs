using System;
using Xamarin.Forms;
using Hunt.Mobile.Common;
using System.Diagnostics;

[assembly: Dependency(typeof(Hunt.Mobile.Android.Logger))]

namespace Hunt.Mobile.Android
{
	public class Logger : ILogger
	{
		public void Write(object data)
		{
			#if DEBUG
				Debug.WriteLine(data);
			#else
	  			Console.Write(data);
			#endif
		}

		public void WriteLine(object data)
		{
			#if DEBUG
				Debug.WriteLine(data);
			#else
				Console.Write(data);
			#endif
		}
	}
}