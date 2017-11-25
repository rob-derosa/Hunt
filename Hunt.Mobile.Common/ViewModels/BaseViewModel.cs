using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hunt.Common;

namespace Hunt.Mobile.Common
{
	public class BaseViewModel : BaseNotify
	{
		public BaseViewModel()
		{
			//Log.Instance.WriteLine($"{GetType().Name} created");
		}

		CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public event EventHandler IsBusyChanged;
		public event EventHandler RefreshedGame;

		bool _isBusy;
		public virtual bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				if(SetPropertyChanged(ref _isBusy, value))
				{
					SetPropertyChanged(nameof(IsNotBusy));
					OnIsBusyChanged();
					IsBusyChanged?.Invoke(this, new EventArgs());
				}
			}
		}
		public bool IsNotBusy => !IsBusy;

		bool _isRefreshingGame;
		public bool IsRefreshingGame
		{
			get { return _isRefreshingGame; }
			set { SetPropertyChanged(ref _isRefreshingGame, value); }
		}

		string _currentState;
		public string CurrentState
		{
			get { return _currentState; }
			set { SetPropertyChanged(ref _currentState, value); }
		}

		public bool WasCancelledAndReset
		{
			get
			{
				var cancelled = _cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested;

				if(cancelled)
					ResetCancellationToken();

				return cancelled;
			}
		}

		protected virtual void OnIsBusyChanged() { }

		public CancellationToken CancellationToken
		{
			get
			{
				if(_cancellationTokenSource == null)
					_cancellationTokenSource = new CancellationTokenSource();

				return _cancellationTokenSource.Token;
			}
		}

		public void ResetCancellationToken()
		{
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public virtual void CancelTasks()
		{
			if(!_cancellationTokenSource.IsCancellationRequested && CancellationToken.CanBeCanceled)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		public virtual void OnNotificationReceived(NotificationEventArgs args)
		{
			Log.Instance.WriteLine($"Notification received on {GetType().Name}");
		}

		public virtual void OnResume()
		{
			Log.Instance.WriteLine($"App resumed on {GetType().Name}");
		}

		public virtual void OnSleep()
		{
			Log.Instance.WriteLine($"App slept on {GetType().Name}");
		}

		async public virtual Task<Game> RefreshGame(Game game)
		{
			if(game == null)
				return null;

			using(var busy = new Busy(this))
			{
				try
				{
					IsRefreshingGame = true;
					Log.Instance.WriteLine("Refreshing game...");

					var task = new Task<Game>(() => App.Instance.DataService.GetGame(game?.Id).Result);
					await task.RunProtected();

					Log.Instance.WriteLine("Refreshing game complete");

					if(!task.WasSuccessful() || task.Result == null)
						return null;

					if(task.Result.Id == App.Instance.CurrentGame?.Id)
					{
						App.Instance.CurrentGame = task.Result;

						if(task.Result.HasEnded)
							App.Instance.CurrentGame = null;
					}

					return task.Result;
				}
				catch(Exception e)
				{
					e.Notify();
					return null;
				}
				finally
				{
					IsRefreshingGame = false;
				}
			}
		}

		protected async Task<Game> SaveGameSafe(Func<Game, Game> gameUpdateLogic, string updateAction, IDictionary<string, string> args = null, Game refreshedGame = null)
		{
			Game game = null;
			try
			{
				game = gameUpdateLogic(refreshedGame);

				if(game == null)
					return null;

				var task = new Task<Game>(() => App.Instance.DataService.SaveGame(game, updateAction, args).Result);
				await task.RunProtected();

				if(!task.WasSuccessful())
					return null;
				
				if(task.Result != null)
					App.Instance.CurrentGame = task.Result;

				return task.Result;
			}
			catch(DocumentVersionConclictException)
			{
				var refreshed = await RefreshGame(game).ConfigureAwait(false);

				if(refreshed != null)
					return await SaveGameSafe(gameUpdateLogic, updateAction, args, refreshed);

				return null;
			}
		}

		protected void InvokeRefreshedGame()
		{
			RefreshedGame?.Invoke(this, new EventArgs());
		}

		public virtual void NotifyPropertiesChanged()
		{
		}
	}

	public class SendResponse<T>
	{
		public SendResponse() { }

		public SendResponse(T result)
		{
			Result = result;
			Status = ResponseStatus.Success;
		}

		public T Result { get; set; }
		public ResponseStatus Status { get; set; }
		public Exception Exception { get; set; }
	}

	public enum ResponseStatus
	{
		None,
		Success,
		Fail,
		NoConnection,
	}
}