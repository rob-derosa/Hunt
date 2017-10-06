using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public partial class RegistrationPage : BaseContentPage<RegistrationViewModel>
	{
		LoadingDataPage _loadingPage;

		public RegistrationPage()
		{
			InitializeComponent();

			emailEntry.TextChanged += (sender, e) => ViewModel.Avatar = null;
			emailEntry.Unfocused += async(sender, e) => await CheckForAvatar();
		}

		async Task CheckForAvatar()
		{
			if(!string.IsNullOrEmpty(emailEntry.Text))
			{
				var email = emailEntry.Text.Trim();
				var split = email.Split('@');

				if(split.Length == 2)
				{
					if(split[1].ToLower() == "hbo.com") //GoT character
					{
						var url = $"{Keys.Constants.BlobAssetsBaseUrl}/avatars/{split[0].ToLower()}.jpg";
						ViewModel.Avatar = url;
						ViewModel.Alias = split[0].ToTitleCase();
						return;
					}
				}

				var valid = await App.Instance.DataService.IsGravatarValid(email);
				ViewModel.Avatar = valid ? email.ToGravatarUrl(200) : Keys.Constants.DefaultAvatarUrl;
			}
		}

		async protected override void OnAppearing()
		{
			base.OnAppearing();
			await CheckForAvatar();
		}

		async void ContinueClicked(object sender, EventArgs e)
		{
			try
			{
				using(var b = new Busy(ViewModel))
				{
					await Task.Run(() =>
					{
						ViewModel.RegisterPlayer();
						_loadingPage = new LoadingDataPage();
					});

					await Navigation.PushAsync(_loadingPage);
					ViewModel.Reset();
				}
			}
			catch(Exception ex)
			{
				Hud.Instance.ShowToast(ex.Message, NoticationType.Error);
				Logger.Instance.WriteLine(ex);
			}
		}

		void MicrosoftClicked(object sender, EventArgs e)
		{
			Device.OpenUri(new Uri(Keys.Constants.SourceCodeUrl));
		}
	}
}