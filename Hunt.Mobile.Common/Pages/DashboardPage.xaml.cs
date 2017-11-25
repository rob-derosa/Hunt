using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace Hunt.Mobile.Common
{
	public partial class DashboardPage : BaseContentPage<DashboardViewModel>
	{
		bool _isScanning;

		public DashboardPage()
		{
			InitializeComponent();
			entryCodeEntry.TextChanged += async (sender, e) =>
			{
				if(ViewModel.EntryCode?.Trim().Length == 6)
				{
					entryCodeEntry.Unfocus();
					await Task.Delay(100);
					await SubmitEntryCode();
				}
			};
		}

		public override void OnBeforePoppedTo()
		{
			ViewModel.NotifyPropertiesChanged();
			base.OnBeforePoppedTo();
		}

		#region Event Handlers

		protected override void OnNotificationReceived(NotificationEventArgs args)
		{
			base.OnNotificationReceived(args);
		}

		async void CreateGameClicked(object sender, EventArgs e)
		{
			ViewModel.Reset();
			var page = new CreateGamePage();
			page.OnGameCreated = async () => await Navigation.PushAsync(new GameDetailsPage(App.Instance.CurrentGame)); 
			await Navigation.PushModalAsync(page);
		}

		async void ContinueToGameClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new GameDetailsPage(App.Instance.CurrentGame));
		}

		async void SignOutClicked(object sender, EventArgs e)
		{
			var response = await DisplayAlert("Sure you want to sign out?", null, "Yes", "No");

			if(!response)
				return;

			await this.SignOutPlayer();
		}

		void PastGamesClicked(object sender, EventArgs e)
		{
			Hud.Instance.ShowToast("This feature has not been implemented yet.");
		}

		async void ScanCodeClicked(object sender, EventArgs e)
		{
			await LaunchScanPage();
		}

		#endregion

		#region Join Game

		async Task LaunchScanPage()
		{
			if(_isScanning)
				return;

			_isScanning = true;
			if(_scanPage == null)
			{
				var overlay = new ZXingDefaultOverlay { ShowFlashButton = false };
				overlay.BindingContext = overlay;
				_scanPage = new ZXingScannerPage(null, overlay);
				_scanPage.OnScanResult += HandleScanResult;
			}

			var cancel = new ToolbarItem
			{
				Text = "Cancel",
				IsDestructive = true,
				Command = new Command(() => { _isScanning = false; Navigation.PopModalAsync(); }),
			};

			var nav = _scanPage.ToNav();
			nav.BarBackgroundColor = Color.Black;
			nav.ToolbarItems.Add(cancel);
			await Navigation.PushModalAsync(nav);
		}

		ZXingScannerPage _scanPage;
		void HandleScanResult(ZXing.Result result)
		{
			_scanPage.IsScanning = false;
			Device.BeginInvokeOnMainThread(() =>
			{
				ViewModel.EntryCode = result.Text;
				_isScanning = false;
				Navigation.PopModalAsync();
			});
		}

		async Task SubmitEntryCode()
		{
			var game = await ViewModel.GetGameByEntryCode();

			if(game != null)
			{
				ViewModel.Reset();
				await Navigation.PushAsync(new TeamListPage(game));
			}
		}

		#endregion
	}
}