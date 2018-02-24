using System;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Hunt.Terminal
{
	class Program
	{
		static void Main(string[] args)
		{
			MainAsync(args).GetAwaiter().GetResult();
		}

		static async Task MainAsync(string[] args)
		{
			Console.WriteLine("Registering EventProcessor...");

			var connectionString = "Endpoint=sb://huntappeventhub.servicebus.windows.net/;SharedAccessKeyName=SendListen;SharedAccessKey=5ZAynJMui0LEfsIkZEQTdmRzZQv1C3iHw2XLziS68AI=";
			var entityPath = "games";
			var storageAccountName = "huntappstorage";
			var storageAccountContainer = "events";
			var storageAccountKey = "rrttFty/b52ET/e8VqpMSN+ZqAUP7hcXVkdekrPX58gsMZyOCrE+igN07t3lyi7tAV0+OrJFBaDtMe06YJ2tFw==";
			var storageAccountConnectionString = string.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}");

			var eventProcessorHost = new EventProcessorHost(
				entityPath,
				PartitionReceiver.DefaultConsumerGroupName,
				connectionString,
				storageAccountConnectionString,
				storageAccountContainer);

			// Registers the Event Processor Host and starts receiving messages
			await eventProcessorHost.RegisterEventProcessorAsync<EventProcessor>();

			Console.WriteLine("Receiving. Press ENTER to stop worker.");
			Console.ReadLine();

			// Disposes of the Event Processor Host
			await eventProcessorHost.UnregisterEventProcessorAsync();
		}
	}
}
