using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

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
				var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
				Console.WriteLine($"{dt:MM/dd/yy h:mm:ss.ff}\n{data}\n");
			}

			return context.CheckpointAsync();
		}
	}
}