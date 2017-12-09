using Android.App;
using Android.Content.PM;
using Android.OS;
using Hunt.Mobile.Common;
using ImageCircle.Forms.Plugin.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Hunt.Mobile.Android
{
	[Activity(Label = "Hunt", Icon = "@drawable/icon", Theme = "@style/DefaultTheme", MainLauncher = false,
			  ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			ZXing.Net.Mobile.Forms.Android.Platform.Init();
			ImageCircleRenderer.Init();
			Forms.Init(this, savedInstanceState);
			LoadApplication(new App());

			var w = (int)(Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density);
			var h = (int)(Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density);
			App.Instance.ScreenSize = new Size(w, h);

			try { XFGloss.Droid.Library.Init(this, savedInstanceState); }catch{}
			Window.SetStatusBarColor(Color.FromHex("#282827").ToAndroid());
		}

		protected override void OnResume()
		{
			base.OnResume();
			App.Instance.OnHuntResume();
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			global::ZXing.Net.Mobile.Forms.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}