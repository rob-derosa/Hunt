using System;

namespace Hunt.Mobile.Common
{
	public interface IImageService
	{
		byte[] GenerateQRCode(string content, int width = 250, int height = 250, int margin = 10);
		byte[] LoadAndResizeBitmap(string fileName, int width, int height);
	}
}
