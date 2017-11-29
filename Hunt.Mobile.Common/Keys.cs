using System;

namespace Hunt.Mobile.Common
{
	public class Keys
	{
		//Constant values
		public static class Constants
		{
			public static string SourceCodeUrl = "http://github.com/rob-derosa/hunt";
			public static string BlobBaseUrl = "PUT_URL_HERE";
			public static string CdnBaseUrl = "PUT_URL_HERE";

			public static string CdnImagesBaseUrl = $"{CdnBaseUrl}/images";
			public static string BlobAssetsBaseUrl = $"{BlobBaseUrl}/assets";
			public static string BlobImagesBaseUrl = $"{BlobBaseUrl}/images";
			public static string DefaultAvatarUrl = $"{BlobAssetsBaseUrl}/avatars/jon.jpg";

			public static string NoConnectionMessage = "You don't seem to be connected to the internet right now.";
			public static int PointsPerAttribute = 100;
		}

		//Mobile Center
		public static class MobileCenter
		{
			public static string iOSToken = "PUT_TOKEN_HERE";
			public static string AndroidToken = "PUT_TOKEN_HERE";
		}

		//Azure
		public static class Azure
		{
			public static string FunctionsUrl = "PUT_URL_HERE/api";
			//public static string AzureFunctionsUrl = "http://localhost:7071/api";
		}
	}
}