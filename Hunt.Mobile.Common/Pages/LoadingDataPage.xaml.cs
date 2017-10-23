using System;
using System.Threading.Tasks;
using Lottie.Forms;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace Hunt.Mobile.Common
{
	public partial class LoadingDataPage : BaseContentPage<DashboardViewModel>
	{
		AnimationView _animationView;
		public LoadingDataPage() 
		{
			InitializeComponent();

			if(!IsDesignMode)
			{
				_animationView = new AnimationView
				{
					Animation = "progress_circle.json",
					WidthRequest = 130,
					HeightRequest = 130,
				};
				contentHolder.Children.Insert(0, _animationView);
			}
		}

		async protected override void OnAppearing()
		{
			CrossConnectivity.Current.ConnectivityChanged += OnConnectivityChanged;

			base.OnAppearing();
			await CheckForOngoingGame();
		}

		protected override void OnDisappearing()
		{
			CrossConnectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
			StopAnimation();
			base.OnDisappearing();
		}

		async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
			if(e.IsConnected)
			{
				await CheckForOngoingGame();
			}
			else
			{
				StopAnimation();
			}
		}

		async void RetryClicked(object sender, EventArgs e)
		{
			await CheckForOngoingGame();
		}

		async Task CheckForOngoingGame()
		{
			var success = false;
			if(App.Instance.CurrentGame == null)
			{
				StartAnimation();

				await ViewModel.RegisterDevice(App.Instance.Player.Id);
				success = await ViewModel.GetOngoingGame();
			}
			else
			{
				Hud.Instance.ShowToast("GAME IS NOT NULL");
				success = true;
			}

			if(!success)
			{
				StopAnimation();
				return;
			}

			await Navigation.PushAsync(new DashboardPage());
		}

		void StartAnimation()
		{
			if (_animationView == null)
				return;

			_animationView.Loop = true;
			_animationView.Play();;
		}

		void StopAnimation()
		{
			if (_animationView == null)
				return;

			_animationView.Loop = false;
		}
	}
}