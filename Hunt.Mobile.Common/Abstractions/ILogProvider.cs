using System;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public interface ILogProvider
	{
		void Write(object data);
		void WriteLine(object data);
		void LogException(Exception exception);
	}

	public static class Log
	{
		static ILogProvider _instance;
		public static ILogProvider Instance => _instance ?? (_instance = DependencyService.Get<ILogProvider>());
	}
}