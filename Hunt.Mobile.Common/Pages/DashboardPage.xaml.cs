using System;
using System.Threading.Tasks;
using Microsoft.Azure.Mobile.Push;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace Hunt.Mobile.Common
{
	public partial class DashboardPage : BaseContentPage<DashboardViewModel>
	{
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

		#region Event Handlers

		protected override void OnNotificationReceived(PushNotificationReceivedEventArgs args)
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

		#endregion

		#region Join Game

		async void ScanCodeClicked(object sender, EventArgs e)
		{
			if(_scanPage == null)
			{
				_scanPage = new ZXingScannerPage();
				_scanPage.OnScanResult += HandleScanResult;
			}

			var cancel = new ToolbarItem
			{
				Text = "Cancel",
				IsDestructive = true,
				Command = new Command(() => { Navigation.PopModalAsync(); }),
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