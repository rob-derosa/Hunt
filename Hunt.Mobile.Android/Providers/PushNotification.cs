using System;
using Android.App;
using Firebase.Iid;
using Android.Util;
using Firebase.Messaging;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Hunt.Mobile.Common;
using Newtonsoft.Json;
using System.Collections.Generic;
using Xamarin.Forms;

[assembly: Dependency(typeof(Hunt.Mobile.Android.PushProvider))]

namespace Hunt.Mobile.Android
{
	public class PushProvider : IPushProvider
	{
		public PushProvider()
		{
			if(MyFirebaseInstanceIdService._loggedToken != null)
				DeviceToken = MyFirebaseInstanceIdService._loggedToken;
		}

		public string DeviceToken { get; set; }
		public event EventHandler<Common.NotificationEventArgs> OnNotificationReceived;

		internal void NotifyReceived(NotificationEventArgs e)
		{
			OnNotificationReceived?.Invoke(this, e);
		}
	}

	[Service]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class MyFirebaseInstanceIdService : FirebaseInstanceIdService
	{
		internal static string _loggedToken;
		public override void OnTokenRefresh()
		{
			base.OnTokenRefresh();

			if(FirebaseInstanceId.Instance == null)
				return;

			var refreshedToken = FirebaseInstanceId.Instance.Token;

			if(Forms.IsInitialized)
				Push.Instance.DeviceToken = refreshedToken;
			else
				_loggedToken = refreshedToken;

			Console.Write($"Token: {refreshedToken}");
		}
	}

	[Service]
	[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
	public class MyFirebaseMessagingService : FirebaseMessagingService
	{
		public override void OnMessageReceived(RemoteMessage message)
		{
			var msg = message.Data["message"];
			var title = message.Data.ContainsKey("title") ? message.Data["title"] : "";

			var notification = new Common.NotificationEventArgs
			{
				Title = message.Data["title"],
				Message = message.Data["message"],
			};

			var payload = message.Data["payload"];

			if(!string.IsNullOrWhiteSpace("payload"))
			{
				notification.Payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(payload);
			}

			((PushProvider)Push.Instance).NotifyReceived(notification);

			if(!string.IsNullOrEmpty(notification.Title) || !string.IsNullOrWhiteSpace(notification.Message))
				SendNotification(title, msg);
		}

		void SendNotification(string title, string message)
		{
			var activityIntent = new Intent(this, typeof(MainActivity));
			activityIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
			var pendingIntent = PendingIntent.GetActivity(this, 0, activityIntent, PendingIntentFlags.UpdateCurrent);
			var defaultSoundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

			var n = new NotificationCompat.Builder(this)
				.SetLights(global::Android.Graphics.Color.Green, 300, 1000)
				.SetContentIntent(pendingIntent)
				.SetContentTitle(title)
				.SetTicker(message)
				.SetContentText(message)
				.SetSound(defaultSoundUri)
				.SetVibrate(new long[] { 200, 200, 100 });

			var nm = NotificationManager.FromContext(this);
			nm.Notify(0, n.Build());
		}
	}
}