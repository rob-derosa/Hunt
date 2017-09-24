using System;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public interface ILogger
	{
		void Write(object data);
		void WriteLine(object data);
	}

	public static class Logger
	{
		static ILogger _instance;
		public static ILogger Instance => _instance ?? (_instance = DependencyService.Get<ILogger>());
	}
}