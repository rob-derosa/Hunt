using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;

using Xamarin.Forms;

using Microsoft.Identity.Client;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Hunt.Mobile.Common
{
    public partial class RegistrationPage : BaseContentPage<RegistrationViewModel>
    {
        LoadingDataPage _loadingPage;

        public RegistrationPage()
        {
            InitializeComponent();

            emailEntry.TextChanged += (sender, e) => ViewModel.Avatar = null;
            emailEntry.Completed += (sender, e) => aliasEntry.Focus();
            emailEntry.Unfocused += async (sender, e) => await CheckForAvatar();
        }

        async Task CheckForAvatar()
        {
            if (!string.IsNullOrEmpty(emailEntry.Text))
            {
                var email = emailEntry.Text.Trim();
                var split = email.Split('@');

                if (split.Length == 2)
                {
                    if (split[1].ToLower() == "hbo.com") //GoT character
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
                using (var b = new Busy(ViewModel, "One moment, please"))
                {
                    var success = await ViewModel.RegisterPlayer();

                    if (!success)
                        return;

                    _loadingPage = new LoadingDataPage();

                    await Navigation.PushAsync(_loadingPage);
                    ViewModel.Reset();
                }
            }
            catch (Exception ex)
            {
                Hud.Instance.ShowToast(ex.Message, NoticationType.Error);
                Log.Instance.LogException(ex);
            }
        }

        void MicrosoftClicked(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri(Keys.Constants.SourceCodeUrl));
        }

        #region Active Directory

        async void OnSignInSignOut(object sender, EventArgs e)
        {
            try
            {
                var ar = await App.PCA.AcquireTokenAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, App.PolicySignUpSignIn), App.UiParent);
            }
            catch (Exception ex)
            {
                // Checking the exception message 
                // should ONLY be done for B2C
                // reset and not any other error.
                if (ex.Message.Contains("AADB2C90118"))
                    OnPasswordReset();
                
                // Alert if any exception excludig user cancelling sign-in dialog
                else if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        private IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);

                if (userIdentifier.EndsWith(policy.ToLower())) 
                    return user;
            }

            return null;
        }

        string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        public void UpdateUserInfo(AuthenticationResult ar)
        {
            JObject user = ParseIdToken(ar.IdToken);
            //lblName.Text = user["name"]?.ToString();
            //lblId.Text = user["oid"]?.ToString();
        }

        JObject ParseIdToken(string idToken)
        {
            // Get the piece with actual user info
            idToken = idToken.Split('.')[1];
            idToken = Base64UrlDecode(idToken);
            return JObject.Parse(idToken);
        }

        /// <summary>
        /// Ons the edit profile.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        async void OnEditProfile(object sender, EventArgs e)
        {
            try
            {
                // KNOWN ISSUE:
                // User will get prompted 
                // to pick an IdP again.
                var ar = await App.PCA.AcquireTokenAsync(App.Scopes, GetUserByPolicy(App.PCA.Users, 
                                                                                     App.PolicyEditProfile), 
                                                         UIBehavior.SelectAccount, string.Empty, null, App.AuthorityEditProfile, App.UiParent);
                UpdateUserInfo(ar);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        /// <summary>
        /// Ons the password reset.
        /// </summary>
        async void OnPasswordReset()
        {
            try
            {
                var ar = await App.PCA.AcquireTokenAsync(App.Scopes, (IUser)null, 
                                                         UIBehavior.SelectAccount, string.Empty, 
                                                         null, App.AuthorityPasswordReset, App.UiParent);
                UpdateUserInfo(ar);
            }
            catch (Exception ex)
            {
                // Alert if any exception excludig user cancelling sign-in dialog
                if (((ex as MsalException)?.ErrorCode != "authentication_canceled"))
                    await DisplayAlert($"Exception:", ex.ToString(), "Dismiss");
            }
        }

        #endregion
    }
}