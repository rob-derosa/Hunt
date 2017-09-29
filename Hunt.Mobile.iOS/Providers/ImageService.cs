using System;
using System.IO;
using System.Runtime.InteropServices;
using Hunt.Mobile.Common;
using Xamarin.Forms;

[assembly: Dependency(typeof(Hunt.Mobile.iOS.ImageService))]

namespace Hunt.Mobile.iOS
{
	public class ImageService : IImageService
	{
		public byte[] GenerateQRCode(string content, int width = 250, int height = 250, int margin = 10)
		{
			var barcodeWriter = new ZXing.Mobile.BarcodeWriter
			{
				Format = ZXing.BarcodeFormat.QR_CODE,
				Options = new ZXing.Common.EncodingOptions
				{
					Width = width,
					Height = height,
					Margin = margin,
				}
			};

			var image = barcodeWriter.Write(content);

			using(var imageData = image.AsPNG())
			{
				var bytes = new Byte[imageData.Length];
				Marshal.Copy(imageData.Bytes, bytes, 0, Convert.ToInt32(imageData.Length));
				return bytes;
			}
		}

		public byte[] LoadAndResizeBitmap(string fileName, int width, int height)
		{
			byte[] buffer = new byte[16 * 1024];
			using(var fileStream = new FileStream(fileName, FileMode.Open))
			{
				using(var ms = new MemoryStream())
				{
					int read;
					while((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						ms.Write(buffer, 0, read);
					}

					return ms.ToArray();
				}
			}
		}
	}
}