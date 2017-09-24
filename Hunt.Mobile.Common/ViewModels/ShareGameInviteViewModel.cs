using System.IO;
using Hunt.Common;
using Xamarin.Forms;

namespace Hunt.Mobile.Common
{
	public class ShareGameInviteViewModel : BaseViewModel
	{
		public Game Game
		{
			get { return App.Instance.CurrentGame; }
		}
		
		ImageSource _qrCodeImage;
		public ImageSource QRCodeImage
		{
			get { return _qrCodeImage; }
			set { SetPropertyChanged(ref _qrCodeImage, value); }
		}

		public void GenerateQRCode()
		{
			if (Game == null)
				return;
			
			using(var busy = new Busy(this, "Generating QR code"))
			{
				var gen = DependencyService.Get<IImageService>();
				var bytes = gen.GenerateQRCode(Game.EntryCode, margin: 0);
				QRCodeImage = ImageSource.FromStream(() => new MemoryStream(bytes));
			}
		}
	}
}