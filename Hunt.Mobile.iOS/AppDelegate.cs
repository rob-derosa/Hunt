using System.IO;
using System.Linq;
using Foundation;
using Hunt.Common;
using Hunt.Mobile.Common;
using ImageCircle.Forms.Plugin.iOS;
using Lottie.Forms.iOS.Renderers;
using Refractored.XamForms.PullToRefresh.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

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
			return base.FinishedLaunching(uiApplication, launchOptions);
		}

		#region Lifecycle

		public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
		{
			System.Console.WriteLine(deviceToken);
			App.Instance.DeviceToken = deviceToken.ToString();
		}

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
			App.Instance.OnHuntResume();
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

		#endregion
	}
}