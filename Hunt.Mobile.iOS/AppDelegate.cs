using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Foundation;
using UIKit;

using Microsoft.Identity.Client;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using Newtonsoft.Json;

using ImageCircle.Forms.Plugin.iOS;

using Lottie.Forms.iOS.Renderers;

using Refractored.XamForms.PullToRefresh.iOS;

using Hunt.Common;

using Hunt.Mobile.Common;

namespace Hunt.Mobile.iOS
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
	{
		public override UIWindow Window
		{
			get;
			set;
		}

		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{
			//#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			//#endif

			ZXing.Net.Mobile.Forms.iOS.Platform.Init();
			Forms.Init();
			AnimationViewRenderer.Init();
			ImageCircleRenderer.Init();
			XFGloss.iOS.Library.Init();

			LoadApplication(new App());

			PullToRefreshLayoutRenderer.Init();

			var w = (int)UIScreen.MainScreen.Bounds.Width;
			var h = (int)UIScreen.MainScreen.Bounds.Height;
			App.Instance.ScreenSize = new Size(w, h);

			UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;

			var settings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, new NSSet());
			UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			UIApplication.SharedApplication.RegisterForRemoteNotifications(); 			UIApplication.SharedApplication.RegisterForRemoteNotifications();

			return base.FinishedLaunching(uiApplication, launchOptions);
		}

		#region Push Notifications

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			Push.Instance.DeviceToken = deviceToken.ToString();
		}

		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, System.Action<UIBackgroundFetchResult> completionHandler)
		{
			Console.WriteLine(userInfo);

			if(!userInfo.TryGetValue(new NSString("aps"), out NSObject aps))
				return;

			var apsHash = aps as NSDictionary;
			var alertHash = apsHash.ObjectForKey(new NSString("alert")) as NSDictionary;

			var badgeValue = alertHash.ObjectForKey(new NSString("badge"));
			if(badgeValue != null)
			{
				if(int.TryParse(new NSString(badgeValue.ToString()), out int count))
				{
					UIApplication.SharedApplication.ApplicationIconBadgeNumber = count;
				}
			}

			var notification = new NotificationEventArgs();

			if(alertHash.TryGetValue(new NSString("payload"), out NSObject payloadValue))
				notification.Payload = JsonConvert.DeserializeObject<Dictionary<string, string>>(payloadValue.ToString());

			if(alertHash.TryGetValue(new NSString("title"), out NSObject titleValue))
				notification.Title = titleValue.ToString();
			
			if(alertHash.TryGetValue(new NSString("body"), out NSObject messageValue))
				notification.Message = messageValue.ToString();

			((PushProvider)Push.Instance).NotifyReceived(notification);
		}

		#endregion

		#region Lifecycle

		public override void OnResignActivation(UIApplication uiApplication)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground(UIApplication uiApplication)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground(UIApplication uiApplication)
		{
			//App.Instance.OnHuntResume();
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated(UIApplication uiApplication)
		{
			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate(UIApplication uiApplication)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations(UIApplication application, UIWindow forWindow)
		{
			return DeviceController.OrientationMask;
		}

        /// <summary>
        /// Opens the URL.
        /// </summary>
        /// <returns><c>true</c>, if URL was opened, <c>false</c> otherwise.</returns>
        /// <param name="app">App.</param>
        /// <param name="url">URL.</param>
        /// <param name="options">Options.</param>
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return true;
        }

		#endregion
	}
}