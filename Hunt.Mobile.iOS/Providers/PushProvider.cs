using System;
using Xamarin.Forms;
using Hunt.Mobile.Common;
using System.Diagnostics;

[assembly: Dependency(typeof(Hunt.Mobile.iOS.PushProvider))]

namespace Hunt.Mobile.iOS
{
	public class PushProvider : IPushProvider
	{
		public string DeviceToken { get; set; }
		public event EventHandler<Common.NotificationEventArgs> OnNotificationReceived;

		internal void NotifyReceived(NotificationEventArgs e)
		{
			OnNotificationReceived?.Invoke(this, e);
		}
	}
}