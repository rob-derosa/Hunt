using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.Generic;

namespace Hunt.Mobile.Common
{
	public interface IPushProvider
	{
		event EventHandler<NotificationEventArgs> OnNotificationReceived;
		string DeviceToken { get; set; }
	}

	public static class Push
	{
		static IPushProvider _instance;
		public static IPushProvider Instance => _instance ?? (_instance = DependencyService.Get<IPushProvider>());
	}

	public class NotificationEventArgs
	{
		public string Title { get; set; }
		public string Message { get; set; }
		public Dictionary<string, string> Payload { get; set; }
	}
}
