using System;
using Android.App;
using Firebase.Iid;
using Android.Util;
using System.Diagnostics;
using System.Globalization;
using Hunt.Mobile.Common;

namespace Hunt.Mobile.Android
{
	[Service]
	[IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
	public class MyFirebaseInstanceIdService : FirebaseInstanceIdService
	{
		//public const string ConnectionString = "Endpoint=sb://robs-sandbox-hub-namespace.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=k3Ob/NvrgBPSMRUbEhdjgmpiI+YFH0A0T+N9sljNSUI=";
		//public const string NotificationHubPath = "robs-sandbox-hub";
		const string TAG = "HuntFirebaseInstanceIdService";

		public override void OnTokenRefresh()
		{
			base.OnTokenRefresh();
			Console.WriteLine("TOKEN: " + FirebaseInstanceId.Instance.Token);
			App.Instance.DeviceToken = FirebaseInstanceId.Instance.Token.ToString();
			//SendRegistrationToServer(refreshedToken);
		}

		//void SendRegistrationToServer(string token)
		//{
		//	const string templateBodyFCM = "{\"data\":{\"message\":\"$(messageParam)\"}}";

		//	var tags = new[] { "pen", "pineapple", "android" };
		//	var expire = DateTime.Now.AddDays(90).ToString(CultureInfo.CreateSpecificCulture("en-US"));

		//	var hub = new NotificationHub(NotificationHubPath, ConnectionString, this);
		//	var outcome = hub.RegisterTemplate(token, "firebase", templateBodyFCM, tags);
		//}
	}
}