using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class BaseGameViewModel : BaseViewModel
	{
		#region Properties

		public ICommand RefreshCommand { get { return new Command(async () => await RefreshGame()); } }

		public bool IsCoordinator => Game.IsCoordinator();
		Game _game;
		public Game Game
		{
			get { return _game; }
		}

		public string TeamName
		{
			get
			{
				if(Game == null)
					return "no game";

				if(Game.IsCoordinator())
					return "coordinator";

				return Game.GetTeam(App.Instance.Player)?.Name;
			}
		}

		#endregion

		async public Task RefreshGame()
		{
			var result = await RefreshGame(Game);
			SetGame(result);
			InvokeRefreshedGame();
		}

		async public override void OnNotificationReceived(Microsoft.Azure.Mobile.Push.PushNotificationReceivedEventArgs args)
		{
			base.OnNotificationReceived(args);

			if(Game.HasEnded || IsBusy)
				return;

			await RefreshGame();
		}

		async public override void OnResume()
		{
			base.OnResume();

			if(!Game.HasEnded)
				await RefreshGame();
		}

		protected override void OnIsBusyChanged()
		{
			base.OnIsBusyChanged();
			NotifyPropertiesChanged();
		}

		public override void NotifyPropertiesChanged()
		{
			base.NotifyPropertiesChanged();
			SetPropertyChanged(nameof(Game));
			SetPropertyChanged(nameof(TeamName));
		}

		public virtual void SetGame(Game game)
		{
			_game = game;
			NotifyPropertiesChanged();
		}
	}
}
