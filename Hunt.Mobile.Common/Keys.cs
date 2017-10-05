using System;

namespace Hunt.Mobile.Common
{
	public class Keys
	{
		//Constant values
		public static class Constants
		{
			public static string SourceCodeUrl = "http://github.com/rob-derosa/hunt";
			public static string NoConnectionMessage = "You don't seem to be connected to the internet right now.";
			public static string DefaultAvatar = "https://huntapp.azureedge.net/images/avatars/jon.jpg";
			public static int PointsPerAttribute = 100;
		}

		//Mobile Center
		public static class MobileCenter
		{
			public static string iOSToken = "cc9ce6e3-a12d-427a-af56-7745ad9d0218";
			public static string AndroidToken = "4f5a3f94-5d09-4dd0-b4ba-de5e8c83bee2";
		}

		//Azure
		public static class Azure
		{
			public static string FunctionsUrl = "https://huntapp.azurewebsites.net/api";
			//public static string AzureFunctionsUrl = "http://localhost:7071/api";
		}
	}
}