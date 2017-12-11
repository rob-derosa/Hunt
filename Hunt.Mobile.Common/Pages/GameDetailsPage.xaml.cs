using System;
using System.Threading.Tasks;
using Hunt.Common;
using Lottie.Forms;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class GameDetailsPage : BaseContentPage<GameDetailsViewModel>
	{
		public GameDetailsPage()
		{
			if(App.Instance.IsDesignMode)
				ViewModel.SetGame(App.Instance.CurrentGame);
	
			Initialize();
		}

		public GameDetailsPage(Game game)
		{
			ViewModel.SetGame(game);
			Initialize();
		}

		void Initialize()
		{
			InitializeComponent();
			ViewModel.RefreshedGame += (sender, e) =>
			{
				StartTimer();
			};
		}

		protected override void OnResume()
		{
			base.OnResume();
			StartTimer();
		}

        protected override void OnAppearing()
        {
			base.OnAppearing();
			StartTimer();
		}

		protected override void OnDisappearing()
		{
			StopTimer();
			base.OnDisappearing();
		}

		protected override void OnSleep()
		{
			StopTimer();
			base.OnSleep();
		}

		bool _shouldTimerTick;
		bool _isTimerTicking;
		void StartTimer()
		{
			if(!ViewModel.Game.IsRunning || _isTimerTicking)
				return;

			_shouldTimerTick = true;
			Device.StartTimer(TimeSpan.FromSeconds(1), OnTimerTick);
		}

		public void StopTimer()
		{
			_shouldTimerTick = false;
		}

		bool OnTimerTick()
		{
			_isTimerTicking = _shouldTimerTick && ViewModel.Game.IsRunning;
			Log.Instance.WriteLine($"Timer ticking: {_isTimerTicking}");
			ViewModel.SetPropertyChanged(nameof(ViewModel.TimeRemaining));
			return _isTimerTicking;
		}

		async void ShareGameClicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new ShareGameInvitePage());
		}

		async void ViewTeamsClicked(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new TeamListPage(ViewModel.Game)); 
		}

		async void LeaveGameClicked(object sender, EventArgs e)
		{
			await LeaveGame();
		}

		async void AddTreasureClicked(object sender, EventArgs e)
		{
			var page = new TreasureTypePage(ViewModel.Game);
			page.ViewModel.OnTreasureAdded = (game) => ViewModel.SetGame(game);
			await Navigation.PushModalAsync(page.ToNav());
		}

		async Task LeaveGame()
		{
			var desc = ViewModel.Game.IsCoordinator() ? "This will immediately end the game for all players." : "";
			var ok = await DisplayAlert("Are you sure you want to leave the game?", desc, "Yes", "No");

			if(!ok)
				return;

			var success = await ViewModel.LeaveGame();
			if (success)
			{
				await Navigation.PopToPageType(typeof(DashboardPage));
			}
		}

		async void LogOut()
		{
			await this.SignOutPlayer();
		}

		async void TreasureItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if(e.SelectedItem == null)
				return;
			
			var item = treasureList.SelectedItem as TreasureViewModel;
			treasureList.SelectedItem = null;

			if(!ViewModel.Game.IsCoordinator() && !ViewModel.Game.HasStarted)
			{
				Hud.Instance.ShowToast("Hints are hidden until the game starts - try clicking on it then.");
				return;
			}

			var page = new TreasureDetailsPage(ViewModel.Game, item.Treasure);
			page.ViewModel.OnTreasureAcquired = async(game) =>
			{
				ViewModel.SetGame(game);

				var ac = game.GetAcquiredTreasure(page.ViewModel.Treasure);

				if(ac.PlayerId != App.Instance.Player.Id)
					await Task.Delay(2000);
	
				await page.PlayAnimation();
				Device.BeginInvokeOnMainThread(async () => await Navigation.PopAsyncAndNotify());

				if(ViewModel.Game.HasEnded)
				{
					if(ViewModel.Game.WinnningTeamId == ViewModel.Game.GetTeam().Id)
						Device.BeginInvokeOnMainThread(async() => await PlayWinningAnimation());

					App.Instance.CurrentGame = null;
				}
			};

			await Navigation.PushAsync(page);
		}

		async Task PlayWinningAnimation()
		{
			try
			{
				var view = new ContentView
				{
					BackgroundColor = Color.FromHex("#7000"),
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand,
				};

				var animation = new AnimationView
				{
					Animation = "trophy.json",
					WidthRequest = 360,
					HeightRequest = 360,
					Loop = false,
					AutoPlay = false,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
				};

				view.Content = animation;
				view.Opacity = 0;

				var layout = Content as AbsoluteLayout;

				AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
				AbsoluteLayout.SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
				layout.Children.Add(view);

				await view.FadeTo(1, 250);
				animation.Play();

				await Task.Delay(3000);
				await view.FadeTo(0, 300);
				layout.Children.Remove(view);
			}
			catch(Exception e)
			{
				Log.Instance.LogException(e);
			}
		}

		async void StartGameClicked(object sender, EventArgs e)
		{
			var result = await DisplayAlert("Are you sure you want to start this game?", string.Empty, "Yes", "No");

			if(!result)
				return;

			await ViewModel.StartGame();
		}

		async void EndGameClicked(object sender, EventArgs e)
		{
			var result = await DisplayAlert("Are you sure you want to end this game prematurely?", string.Empty, "Yes", "No");

			if(!result)
				return;

			await ViewModel.EndGame();
		}
	}
}