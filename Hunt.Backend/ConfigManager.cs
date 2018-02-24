using System;

namespace Hunt.Backend.Functions
{
	public class ConfigManager
	{
		static ConfigManager _instance;
		public static ConfigManager Instance => _instance ?? (_instance = Load());
		public string ServiceBusAccountKey;
		public string ServiceBusAccountName;
		public string ServiceBusAccountNamespace;
		public string BlobSharedStorageKey;
		public string ContentModeratorKey;
		public string ContentModeratorUrl;
		public string CosmosKey;
		public string CosmosUrl;
		public string NotificationHubSandboxConnectionString;
		public string NotificationHubSandboxName;
		public string NotificationHubProductionConnectionString;
		public string NotificationHubProductionName;
		public string VisionUrl;
		public string VisionServiceKey;
		public string CustomVisionTrainingKey;
		public string EventHubEntity;
		public string EventHubEndpoint;
		public string CustomVisionPredictionKey;
		public double CustomVisionMinimumPredictionProbability = .6;

		public const string EndGameBusName = "endgame";
		public const string BlobImageContainer = "images";
		public const string CosmosCollectionId = "items";
		public const string CosmosDatabaseId = "games";

		public static ConfigManager Load()
		{
			var config = new ConfigManager
			{
				ServiceBusAccountKey = Environment.GetEnvironmentVariable("SERVICEBUS_ACCOUNTKEY"),
				ServiceBusAccountName = Environment.GetEnvironmentVariable("SERVICEBUS_ACCOUNTNAME"),
				ServiceBusAccountNamespace = Environment.GetEnvironmentVariable("SERVICEBUS_NAMESPACE"),

				BlobSharedStorageKey = Environment.GetEnvironmentVariable("BLOB_SHAREDSTORAGEKEY"),

				ContentModeratorKey = Environment.GetEnvironmentVariable("CONTENTMODERATOR_KEY"),
				ContentModeratorUrl = Environment.GetEnvironmentVariable("CONTENTMODERATOR_URL"),

				CosmosKey = Environment.GetEnvironmentVariable("COSMOS_KEY"),
				CosmosUrl = Environment.GetEnvironmentVariable("COSMOS_URL"),

				NotificationHubSandboxConnectionString = Environment.GetEnvironmentVariable("HUB_SANDBOX_CONNECTIONSTRING"),
				NotificationHubSandboxName = Environment.GetEnvironmentVariable("HUB_SANDBOX_NAME"),
				NotificationHubProductionConnectionString = Environment.GetEnvironmentVariable("HUB_PRODUCTION_CONNECTIONSTRING"),
				NotificationHubProductionName = Environment.GetEnvironmentVariable("HUB_PRODUCTION_NAME"),

				VisionUrl = Environment.GetEnvironmentVariable("VISION_URL"),
				VisionServiceKey = Environment.GetEnvironmentVariable("VISION_SERVICEKEY"),

				CustomVisionTrainingKey = Environment.GetEnvironmentVariable("CUSTOMVISION_TRAININGKEY"),
				CustomVisionPredictionKey = Environment.GetEnvironmentVariable("CUSTOMVISION_PREDICTIONKEY"),

				EventHubEndpoint = Environment.GetEnvironmentVariable("EVENTHUB_ENDPOINT"),
				EventHubEntity = Environment.GetEnvironmentVariable("EVENTHUB_ENTITY"),
			};

			var probability = Environment.GetEnvironmentVariable("CUSTOMVISION_MINIMUM_PREDICTION_PROBABILITY");
			if (double.TryParse(probability, out var i)) config.CustomVisionMinimumPredictionProbability = i;

			return config;
		}
	}
}
