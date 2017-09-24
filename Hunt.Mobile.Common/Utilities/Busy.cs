using System;

namespace Hunt.Mobile.Common
{
	public class Busy : IDisposable
	{
		readonly object _sync = new object();
		readonly BaseViewModel _viewModel;

		string _hudMessage;
		public string HudMessage
		{
			get { return _hudMessage; }
			set 
			{
				if(value != _hudMessage && value != null)
				{
					_hudMessage = value;
				}
			}
		}

		public Busy(BaseViewModel viewModel, string message)
		{
			_viewModel = viewModel;
			StartProcess(true, message);
		}

		public Busy(BaseViewModel viewModel, bool show = false, string message = null)
		{
			_viewModel = viewModel;
			StartProcess(show, message);
		}

		void StartProcess(bool show, string message)
		{
			lock(_sync)
			{
				_viewModel.IsBusy = true;

				if(show)
				{
					HudMessage = message;
					Hud.Instance.ShowProgress(HudMessage);
				}
			}
		}

		public void Dispose()
		{
			lock(_sync)
			{
				_viewModel.IsBusy = false;
				_viewModel.CurrentState = null;

				if(HudMessage != null)
				{
					Hud.Instance.Dismiss();
				}
			}
		}
	}
}