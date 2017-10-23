using System;
using Xamarin.Forms;
using Hunt.Mobile.Common;
using System.Diagnostics;

[assembly: Dependency(typeof(Hunt.Mobile.Android.LogProvider))]

namespace Hunt.Mobile.Android
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

			//TODO: log to Mobile Center as events until caught exceptions are supported
		}

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