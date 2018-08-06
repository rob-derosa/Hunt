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
			var storageAccountKey = "0ZMsQe9PdwKzP9y/d8cxcWn9nGH/2zlATiBsRYVmyCCiw84Nn4BSYtZ6Cc+kLLDkVGlcrv2x2DA8ndkK+JoqMQ==";
			var storageAccountConnectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}";

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
