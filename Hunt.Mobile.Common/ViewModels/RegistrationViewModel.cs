using System;
using System.Threading.Tasks;
using Hunt.Common;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;

namespace Hunt.Mobile.Common
{
	public class RegistrationViewModel : BaseViewModel
	{
		string _email;// = $"{Device.RuntimePlatform.ToLower()}@microsoft.com";
		public string Email
		{
			get { return _email; }
			set { SetPropertyChanged(ref _email, value); SetPropertyChanged(nameof(CanContinue)); }
		}

		string _alias;// = $"{Device.RuntimePlatform} Player";
		public string Alias
		{
			get { return _alias; }
			set { SetPropertyChanged(ref _alias, value); SetPropertyChanged(nameof(CanContinue)); }
		}

		string _avatar;// = "https://s.gravatar.com/avatar/fc29d876d1ae49003cbc76a43f456d9b?s=200";
		public string Avatar
		{
			get { return _avatar; }
			set { SetPropertyChanged(ref _avatar, value); }
		}

		public bool CanContinue { get { return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Alias); } }

		async public Task<bool> RegisterPlayer()
		{
			if(string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Alias))
				throw new Exception("Please specify an email and first name.");

			var email = Email; //We toy with a copy because the two-way binding will cause the TextChanged event to fire
			var split = email.Split('@');
			if(split.Length == 2)
			{
				if(split[1].ToLower() == "hbo.com") //GoT character
				{
					//Randomize the email (which serves as the primary key) so it doesn't conflict w/ other demo games
					var rand = Guid.NewGuid().ToString().Substring(0, 7).ToLower();
					email = $"{split[0]}-{rand}@{split[1]}";
				}
			}

			bool isAppropriate = false;
			var task = new Task(() =>
			{
				var aliasValid = App.Instance.DataService.IsTextValidAppropriate(Alias.Trim()).Result;
				var emailValid = App.Instance.DataService.IsTextValidAppropriate(Email.Trim()).Result;

				isAppropriate = aliasValid && emailValid;
			});

			await task.RunProtected();

			if(!task.WasSuccessful())
				return false;

			if(!isAppropriate)
			{
				Hud.Instance.ShowToast("Inappropriate content was detected");
				return false;
			}

			var player = new Player
			{
				Avatar = Avatar,
				Email = email.Trim(),
				Alias = Alias.Trim(),
				DeviceToken = App.Instance.DeviceToken,
			};

			var args = new KVP { { "email", player.Email }, { "firstName", player.Alias }, { "avatar", player.Avatar } };
			Analytics.TrackEvent("Player registered", args);
			App.Instance.SetPlayer(player);
			return true;
		}

		public void Reset()
		{
			Alias = null;
			Email = null;
		}
	}
}