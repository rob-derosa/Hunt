using System;

namespace Hunt.Backend.Functions
{
	public class Config
	{
        //ServiceBus
        public static class ServiceBus
        {
			//Queues
			public const string EndGameBusName = "endgame";

            //Account
            public static string AccountName;
            public static string AccountKey;
            public static string AccountNamespace;
        }

        //Blob
        public static class Blob
		{
			public static string SharedStorageKey;
            public static string ImageContainer;
        }

        //Vision
        public static class Vision
		{
			public static string Url;
			public static string ServiceKey;
		}

		public static class Cosmos
		{
			public static string Url;
			public static string Key;
		}

        public static class ContentModerator
        {
			public static string Url;
			public static string Key;
		}

		public static class NotificationHub
		{
			public static class Sandbox
			{
				public static string Name;
				public static string ConnectionString;
			}

			public static class Production
			{
				public static string Name;
				public static string ConnectionString;
			}
		}
    }
}
