using System;

namespace Hunt.Backend.Functions
{
	public class Keys
	{
		public static class MobileCenter
		{
			public static string Token = "TOKEN_HERE";
			public static string iOSUrl = "https://api.mobile.azure.com/v0.1/apps/Hunt-App/Hunt";
			public static string AndroidUrl = "https://api.mobile.azure.com/v0.1/apps/Hunt-App/Hunt-Android";
		}

        //ServiceBus
        public static class ServiceBus
        {
            public const string Url = "ENDPOINT_HERE";

            //Queues
            public const string ImageBusName = "imageprocess";
            public const string EndGameBusName = "endgame";

            //Account
            public const string AccountName = "RootManageSharedAccessKey";
            public const string AccountKey = "ACCOUNT_KEY";
            public const string AccountNamespace = "huntappbus";
        }

        //Blob
        public static class Blob
		{
			public const string SharedStorageKey = "DefaultEndpointsProtocol=https;AccountName=huntappstorage;AccountKey=rrttFty/b52ET/e8VqpMSN+ZqAUP7hcXVkdekrPX58gsMZyOCrE+igN07t3lyi7tAV0+OrJFBaDtMe06YJ2tFw==;EndpointSuffix=core.windows.net";
            public const string ImageContainer = "images";
        }

        //Vision
        public static class Vision
		{
			public const string Url = "https://southcentralus.api.cognitive.microsoft.com/vision/v1.0";
			public const string ServiceKey = "bbf18bfedf604d558ee44eb974bb5958";
		}

		public static class Cosmos
		{
			public static string Url = @"https://huntapp.documents.azure.com:443/";
			public static string Key = "oTGZaPqa97uvgfNdMFu1r2MvhzZ0Okf2iaaarskJReBKKEAPPKOeZNVNFBETkPddg3UpmTo4gpZWQ6cX7IzjHA==";
		}

        public static class ContentModerator
        {
            public static string Url = @"https://southcentralus.api.cognitive.microsoft.com/contentmoderator/moderate/v1.0/ProcessText/Screen/?language=eng&autocorrect=true&PII=true";
            public static string Key = "KEY_HERE";
        }

		public static class NotificationHub
		{
			public static class Sandbox
			{
				public static string Name = "huntappnotificationhub";
				public static string ConnectionString = "ENDPOINT_HERE";
			}

			public static class Production
			{
				public static string Name = "huntappnotificationhubproduction";
				public static string ConnectionString = "ENDPOINT_HERE";
			}
		}
    }
}
