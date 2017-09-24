using System;

namespace Hunt.Backend.Functions
{
	public class Keys
	{
		public static class MobileCenter
		{
			public static string Token = "bf3a075b9ffbe6fda680801b49ecdf4d163e74e4";
			public static string iOSUrl = "https://api.mobile.azure.com/v0.1/apps/Hunt-App/Hunt";
			public static string AndroidUrl = "https://api.mobile.azure.com/v0.1/apps/Hunt-App/Hunt-Android";
		}

        //ServiceBus
        public static class ServiceBus
        {
            public const string Url = @"Endpoint=sb://huntapp.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=KGkiZqcbFsaOsvSfDTuertGHyNkxjYBPP7Hv2JOG/6o=";

            //Queues
            public const string ImageBusName = "imageprocess";
            public const string EndGameBusName = "endgame";

            //Account
            public const string AccountName = "RootManageSharedAccessKey";
            public const string AccountKey = "KGkiZqcbFsaOsvSfDTuertGHyNkxjYBPP7Hv2JOG/6o=";
            public const string AccountNamespace = "huntapp";
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
			public const string Url = "https://westeurope.api.cognitive.microsoft.com/vision/v1.0";
			public const string ServiceKey = "183a86f557c2407ea52fdf1894335e29";
		}

		public static class Cosmos
		{
			public static string Url = @"https://huntapp.documents.azure.com:443/";
			public static string Key = "oTGZaPqa97uvgfNdMFu1r2MvhzZ0Okf2iaaarskJReBKKEAPPKOeZNVNFBETkPddg3UpmTo4gpZWQ6cX7IzjHA==";
		}

        public static class ContentModerator
        {
            public static string Url = @"https://westeurope.api.cognitive.microsoft.com/contentmoderator/moderate/v1.0/ProcessText/Screen/?language=eng&autocorrect=true&PII=true";
            public static string Key = "31262fb7c53b435a9d702dc43d53731b";
        }
    }
}
