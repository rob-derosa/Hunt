using System;
using Xamarin.Forms;
using XFGloss;

namespace Hunt.Mobile.Common
{
	public partial class ShareGameInvitePage : BaseContentPage<ShareGameInviteViewModel>
	{
		public ShareGameInvitePage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if(ViewModel.QRCodeImage == null)
			{
				ViewModel.GenerateQRCode();
			}
		}
	}
}