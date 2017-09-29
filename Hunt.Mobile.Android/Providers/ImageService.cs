using System;
using System.IO;
using Android.Graphics;
using Android.Media;
using Hunt.Mobile.Common;
using Xamarin.Forms;

[assembly: Dependency(typeof(Hunt.Mobile.Android.ImageService))]

namespace Hunt.Mobile.Android
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
			using(var stream = new MemoryStream())
			{
				image.Compress(Bitmap.CompressFormat.Png, 0, stream);
				image = null;
				return stream.ToArray();
			}
		}

		public byte[] LoadAndResizeBitmap(string fileName, int width, int height)
		{
			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
			var original = BitmapFactory.DecodeFile(fileName, options);

			// Next we calculate the ratio that we need to resize the image by
			// in order to fit the requested dimensions.
			int outHeight = options.OutHeight;
			int outWidth = options.OutWidth;
			int inSampleSize = 1;

			if(outHeight > height || outWidth > width)
			{
				inSampleSize = outWidth > outHeight
					? outHeight / height
						: outWidth / width;
			}

			// Now we will load the image and have BitmapFactory resize it for us.
			options.InSampleSize = inSampleSize;
			options.InJustDecodeBounds = false;
			Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

			// Images are being saved in landscape, so rotate them back to portrait if they were taken in portrait
			Matrix mtx = new Matrix();
			ExifInterface exif = new ExifInterface(fileName);
			string orientation = exif.GetAttribute(ExifInterface.TagOrientation);
			bool wasResized = true;

			switch(orientation)
			{
				case "6": // portrait
					mtx.PreRotate(90);
					resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					mtx.Dispose();
					mtx = null;
					break;
				case "1": // landscape
					wasResized = false;
					break;
				default:
					mtx.PreRotate(90);
					resizedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
					mtx.Dispose();
					mtx = null;
					break;
			}

			if(!wasResized)
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

			byte[] bitmapData;
			using(var stream = new MemoryStream())
			{
				resizedBitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
				bitmapData = stream.ToArray();
			}

			return bitmapData;
		}
	}
}