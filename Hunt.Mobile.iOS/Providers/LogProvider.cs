using System;
using Xamarin.Forms;
using Hunt.Mobile.Common;
using System.Diagnostics;
using Microsoft.AppCenter.Crashes;

[assembly: Dependency(typeof(Hunt.Mobile.iOS.LogProvider))]

namespace Hunt.Mobile.iOS
{
	public class LogProvider : ILogProvider
	{
		public void LogException(Exception exception)
		{
			#if DEBUG
				Debug.WriteLine(exception);
			#else
				Console.Write(exception);
			#endif

			Crashes.TrackError(exception);
		}


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