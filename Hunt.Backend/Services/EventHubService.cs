using System;
using System.Text;
using System.Threading.Tasks;
using Hunt.Common;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ServiceBus.Messaging;

namespace Hunt.Backend.Functions
{
	public class EventHubService
	{
		static EventHubService _instance;
		public static EventHubService Instance { get => _instance ?? (_instance = new EventHubService()); }

		public async Task SendEvent(string message, Game game = null, Player player = null)
		{
			try
			{
				var factory = MessagingFactory.CreateFromConnectionString($"{ConfigManager.Instance.EventHubEndpoint};TransportType=Amqp");
				var client = factory.CreateEventHubClient(ConfigManager.Instance.EventHubEntity);

				if (game != null)
					message = $"{message}\n\tGame:\t{game.Name}";

				if (player != null)
					message = $"{message}\n\tPlayer:\t{player.Alias}";

				await client.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
				await client.CloseAsync();
			}
			catch (Exception e)
			{
				var analytic = new AnalyticService(new RequestTelemetry());
				analytic.TrackException(e.GetBaseException());
			}
		}
	}
}