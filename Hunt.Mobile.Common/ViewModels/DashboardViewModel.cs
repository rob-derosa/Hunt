using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class DashboardViewModel : BaseViewModel
	{
		#region Properties

		public ICommand RefreshCommand { get { return new Command(async () => await GetOngoingGame()); } }
		public bool HasExistingGame => App.Instance.CurrentGame != null;

		string _entryCode;
		public string EntryCode
		{
			get { return _entryCode; }
			set { SetPropertyChanged(ref _entryCode, value); }
		}

		#endregion

		async public Task<Game> GetGameByEntryCode()
		{
			using(var busy = new Busy(this, "Looking up game"))
			{
				try
				{
					NotifyPropertiesChanged();

					var task = new Task<Game>(() => App.Instance.DataService.GetGameByEntryCode(EntryCode).Result);
					await task.RunProtected(NotifyMode.Throw);

					try
					{
						await task.RunProtected(NotifyMode.Throw);
					}
					catch(Exception e)
					{
						if(e.Message.Contains("409"))
						{
							Hud.Instance.ShowToast("You're already involed in another game", NoticationType.Error);
							return null;
						}
					}

					if(task.WasSuccessful())
					{
						if(task.Result == null)
						{
							Hud.Instance.ShowToast("Invalid code", NoticationType.Info);
							return null;
						}
						return task.Result;
					}
					return null;
				}
				finally
				{
					NotifyPropertiesChanged();
				}
			}
		}

		public void Reset()
		{
			EntryCode = null;
		}

		/// <summary>
		/// Gets an ongoing game for a player, if there is one
		/// </summary>
		/// <returns>True of the check was successful and did not err out</returns>
		async public Task<bool> GetOngoingGame()
		{
			App.Instance.CurrentGame = null;

			if(App.Instance.Player == null)
				return false;

			try
			{
				using(var busy = new Busy(this))
				{
					IsRefreshingGame = true;
					NotifyPropertiesChanged();

					var task = new Task<Game>(() => App.Instance.DataService.GetOngoingGame(App.Instance.Player.Email).Result);
					await task.RunProtected().ConfigureAwait(false);

					if(!task.WasSuccessful())
						return false;

					var game = task.Result;
					if(game != null)
					{
						var current = App.Instance.Player;
						var player = game.GetPlayerByEmail();

						if(current.Alias != player.Alias || current.InstallId != player.InstallId)
						{
							player.Alias = current.Alias;
							player.InstallId = current.InstallId;
							game = await App.Instance.DataService.SaveGame(game, GameUpdateAction.UpdatePlayer).ConfigureAwait(false);
						}

						if(player.Id != current.Id)
						{
							current.Id = player.Id;
							App.Instance.SetPlayer(current);
						}
					}

					App.Instance.CurrentGame = game;
					NotifyPropertiesChanged();
				}

				return true;
			}
			finally
			{
				IsRefreshingGame = false;
			}
		}

		public override void NotifyPropertiesChanged()
		{
			base.NotifyPropertiesChanged();
			SetPropertyChanged(nameof(HasExistingGame));
		}
	}
}
