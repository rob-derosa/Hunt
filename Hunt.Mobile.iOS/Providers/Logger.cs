using System;
using Xamarin.Forms;
using Hunt.Mobile.Common;
using System.Diagnostics;

[assembly: Dependency(typeof(Hunt.Mobile.iOS.Logger))]

namespace Hunt.Mobile.iOS
{
	public class Logger : ILogger
	{
		public void Write(object data)
		{
			if(data == null)
				data = "NULL";
			
			#if DEBUG
				Debug.WriteLine(data);
			#else
	  			Console.Write(data);
			#endif
		}

		public void WriteLine(object data)
		{
			if(data == null)
				data = "NULL";

			#if DEBUG
				Debug.WriteLine(data);
			#else
				Console.Write(data);
			#endif
		}
	}
}