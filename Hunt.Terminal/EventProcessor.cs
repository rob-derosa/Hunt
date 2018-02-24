using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using Hunt.Common;

namespace Hunt.Terminal
{
	public class EventProcessor : IEventProcessor
	{
		public Task CloseAsync(PartitionContext context, CloseReason reason)
		{
			Console.WriteLine($"EventProcessor shutting down. Partition {context.PartitionId}, Reason: '{reason}'.");
			return Task.CompletedTask;
		}

		public Task OpenAsync(PartitionContext context)
		{
			Console.WriteLine($"EventProcessor initialized. Partition: {context.PartitionId}");
			return Task.CompletedTask;
		}

		public Task ProcessErrorAsync(PartitionContext context, Exception error)
		{
			Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
			return Task.CompletedTask;
		}

		public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
		{
			foreach (var eventData in messages)
			{
				var dt = DateTime.Now;
				var json = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
				var data = JsonConvert.DeserializeObject<Event>(json);
				var output = new StringBuilder();

				if(!string.IsNullOrWhiteSpace(data.Exception))
					output.AppendLine("**********EXCEPTION OCCURRED**********");

				output.AppendLine(data.Title);

				if (!string.IsNullOrWhiteSpace(data.Exception))
					output.AppendLine($"\tStack:\t{data.Exception}");

				foreach (var kvp in data.Metadata)
				{
					if(kvp.Key == "tags")
					{
						output.AppendLine($"\t{kvp.Key}:\t{kvp.Value.ToString().Replace(", ", "\n\t\t")}");
						continue;
					}

					output.AppendLine($"\t{kvp.Key}:\t{kvp.Value}");
				}

				Console.WriteLine($"{dt:MM/dd/yy h:mm:ss.ff}\n{output}");
			}

			return context.CheckpointAsync();
		}
	}
}