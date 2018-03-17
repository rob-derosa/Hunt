#region Usings

using System;
using System.Threading.Tasks;
using Hunt.Common;
using Lottie.Forms;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;
using Newtonsoft.Json;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Plugin.VersionTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#endregion

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace Hunt.Mobile.Common
{
	public partial class App : Application
	{
		#region Properties

		public event EventHandler AppResumed;
		public event EventHandler AppBackgrounded;
		public event EventHandler<NotificationEventArgs> AppNotificationReceived;

		public Size ScreenSize { get; set; } = new Size(380, 540);

		static App _instance;
		public static App Instance => _instance;

		public Player Player { get; set; }
		public Game CurrentGame { get; set; }

		FunctionsService _dataService;
		public FunctionsService DataService => _dataService ?? (_dataService = new FunctionsService());

		StorageService _storageService;
		public StorageService StorageService => _storageService ?? (_storageService = new StorageService());

		public bool IsDesignMode { get; set; }
		public string Assemblies { get; set; }
		public string CurrentVersion { get; set; }

		#endregion

		public App()
		{
			_instance = this;
			CurrentVersion = CrossVersionTracking.Current.CurrentVersion;

			#if DEBUG
			CurrentVersion = $"{CurrentVersion}d";
			#endif

			//Hack to determine if this page is being rendered within the Visual Studio Forms Previewer or at runtime
			IsDesignMode = Type.GetType("MonoTouch.Design.Parser,MonoTouch.Design") != null;
			if(IsDesignMode)
			{
				#region Mock Data
				Instance.CurrentGame = Mocker.GetGame(5, 4, true, true, true, true, true);
				Instance.CurrentGame.StartDate = null;

				//Has game started
				//Instance.CurrentGame.StartDate = DateTime.Now;

				//Has game ended
				//Instance.CurrentGame.EndDate = DateTime.Now;
				Instance.CurrentGame.StartDate = DateTime.Now;
				Instance.CurrentGame.WinnningTeamId = Instance.CurrentGame.Teams[1].Id;

				//Are you a player
				Player = Instance.CurrentGame.Teams[1].Players[0];

				//Are you the coordinator
				Player = Instance.CurrentGame.Coordinator.Clone(); //Jon
				Instance.CurrentGame.Coordinator = Player;
				#endregion
			}
			else
			{
				ConfigManager.Instance.Load();
			}

			Push.Instance.OnNotificationReceived += OnNotificationReceived;
			InitializeComponent();
		}

		#region Lifecycle

		async protected override void OnStart()
		{
			await Distribute.SetEnabledAsync(false);
			Distribute.ReleaseAvailable = OnReleaseAvailable;

			AppCenter.Start($"android={ConfigManager.Instance.AppCenterAndroidToken};ios={ConfigManager.Instance.AppCenteriOSToken}",
				typeof(Analytics), typeof(Crashes), typeof(Distribute));

			CrossConnectivity.Current.ConnectivityChanged += OnConnectivityChanged;
			if(false) { var l = new AnimationView(); }//Warm up the library

			base.OnStart();

			LoadRegisteredPlayer();
			SetMainPage();

			EvaluateConnectivity();
			WakeUpServer();
		}

		//Forms.ContentPage.OnResume does not seem to fire properly across platforms so we gone roll our own
		public void OnHuntResume()
		{
			AppResumed?.Invoke(this, new EventArgs());
			EvaluateConnectivity();
		}

		protected override void OnSleep()
		{
			base.OnSleep();
			AppBackgrounded?.Invoke(this, new EventArgs());
		}

		void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
			EvaluateConnectivity();
		}

		void OnNotificationReceived(object sender, NotificationEventArgs e)
		{
			AppNotificationReceived?.Invoke(this, e);
		}

		public void EvaluateConnectivity()
		{
			if(!CrossConnectivity.Current.IsConnected)
			{
				Hud.Instance.ShowToast(Constants.NoConnectionMessage);
			}
		}

		void WakeUpServer()
		{
			#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			DataService.WakeUpServer();
			#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

			#endregion

		#region New Release Availability

		bool OnReleaseAvailable(ReleaseDetails releaseDetails)
		{
			// Look at releaseDetails public properties to get version information, release notes text or release notes URL
			string versionName = releaseDetails.ShortVersion;
			string versionCodeOrBuildNumber = releaseDetails.Version;
			string releaseNotes = releaseDetails.ReleaseNotes;
			Uri releaseNotesUrl = releaseDetails.ReleaseNotesUrl;

			// custom dialog
			var title = $"Version {versionName} available!";
			Task answer;

			// On mandatory update, user cannot postpone
			if (releaseDetails.MandatoryUpdate)
			{
				answer = Current.MainPage.DisplayAlert(title, releaseNotes, "Download and Install");
			}
			else
			{
				answer = Current.MainPage.DisplayAlert(title, releaseNotes, "Download and Install", "Maybe tomorrow...");
			}
			answer.ContinueWith((task) =>
			{
				// If mandatory or if answer was positive
				if (releaseDetails.MandatoryUpdate || (task as Task<bool>).Result)
				{
					// Notify SDK that user selected update
					Distribute.NotifyUpdateAction(UpdateAction.Update);
				}
				else
				{
					// Notify SDK that user selected postpone (for 1 day)
					// Note that this method call is ignored by the SDK if the update is mandatory
					Distribute.NotifyUpdateAction(UpdateAction.Postpone);
				}
			});

			// Return true if you are using your own dialog, false otherwise
			return true;
		}

		#endregion

		#region Player Related

		public void SetMainPage()
		{
			var regPage = new RegistrationPage();
			MainPage = regPage.ToNav();

			if(Player != null)
			{
				regPage.Navigation.PushAsync(new LoadingDataPage(), false);
			}
		}

		void LoadRegisteredPlayer()
		{
			if(!string.IsNullOrWhiteSpace(Settings.Player))
			{
				var player = JsonConvert.DeserializeObject<Player>(Settings.Player);

				if(player == null)
					return;
	
				Player = player;
			}
		}

		public void SetPlayer(Player player)
		{
			if(player == null)
				Settings.Player = null;
			else
				Settings.Player = JsonConvert.SerializeObject(player);

			Player = player;
		}

		#endregion
	}
}