using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Plugin.Connectivity;

namespace Hunt.Mobile.Common
{
	public static class TaskRunner
	{
		static public async Task RunProtected(this Task task, NotifyMode notifyMode = NotifyMode.Toast, string errorMessage = null, [CallerMemberName] string caller = null, [CallerLineNumber] long line = 0, [CallerFilePath] string path = null)
		{
			Exception exception = null;

			try
			{
				if(!CrossConnectivity.Current.IsConnected)
				{
					//No connection
					Hud.Instance.ShowToast(Keys.Constants.NoConnectionMessage);
					return;
				}

				await Task.Run(() => {
					task.Start();
					task.Wait();
				});
			}
			catch(TaskCanceledException)
			{
				Logger.Instance.WriteLine("Task Cancelled");
				return;
			}
			catch(Exception e)
			{
				exception = e;
			}

			if(exception != null)
			{
				var inner = exception.InnerException;
				while(inner.InnerException != null)
					inner = inner.InnerException;

				exception = inner;

				var serverDown = exception is WebException ||
					exception.Message.Contains("403") ||
					exception.Message.Contains("503");

				if(serverDown)
					errorMessage = "The server appears to be down";

				if(exception is DocumentVersionConclictException)
					throw exception;

				var msg = errorMessage ?? exception.Message;
				Logger.Instance.WriteLine($"Handled exception:\n{exception.ToString()}");

				switch(notifyMode)
				{
					case NotifyMode.None:
						break;
					case NotifyMode.Toast:
						Hud.Instance.ShowToast(msg);
						break;
					case NotifyMode.Throw:
						throw exception;
				}
			}
		}

		static public async Task<R> RunSafe<R>(this Task<R> task, NotifyMode notifyMode = NotifyMode.Toast, string errorMessage = null, [CallerMemberName] string caller = null, [CallerLineNumber] long line = 0, [CallerFilePath] string path = null)
		{
			await ((Task)task).RunProtected(notifyMode, errorMessage, caller, line, path);
			return task.Result;
		}

		static public bool WasSuccessful(this Task task)
		{
			return task.IsCompleted && !task.IsFaulted;
		}
	}


	public enum NotifyMode
	{
		None,
		Toast,
		Throw,
	}
}