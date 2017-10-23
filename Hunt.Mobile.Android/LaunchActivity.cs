using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Hunt.Mobile.Common;

namespace Hunt.Mobile.Android
{
	[Activity(MainLauncher = true, NoHistory = true, Icon = "@drawable/icon", Theme = "@style/LaunchTheme",
			 ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class LaunchActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			try
			{
				Window.DecorView.SystemUiVisibility = StatusBarVisibility.Hidden;
				ActionBar?.Hide();
			}
			catch(Exception e)
			{
				Log.Instance.LogException(e);
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			StartActivity(typeof(MainActivity));
		}
	}
}