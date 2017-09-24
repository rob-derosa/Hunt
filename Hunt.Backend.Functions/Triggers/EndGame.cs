using System.Net;
using System.Net.Http;
using System;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ServiceBus.Messaging;

using Hunt.Backend.Analytics;
using Hunt.Backend.Functions;
using Hunt.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Hunt.Backend.Triggers
{
	public static class EndGame
	{
		[FunctionName(nameof(EndGame))]
		public static void Run([ServiceBusTrigger("endgame")] BrokeredMessage message, TraceWriter log)
		{
			using (var analytic = new Analytic(new RequestTelemetry
			{
				Name = nameof(EndGame)
			}))
			{
				try
				{
					var gameId = (string)message.Properties["gameId"];

					using (var client = new CosmosDataService())
					{
						var game = client.GetItemAsync<Game>(gameId).Result;
						game.EndDate = DateTime.UtcNow;

						var http = new HttpClient();
						var url = $"https://huntapp.azurewebsites.net/api/SaveGame";

						dynamic payload = new JObject();
						payload.action = GameUpdateAction.EndGame;
						payload.game = JObject.FromObject(game);
						payload.arguments = null;

						var json = JsonConvert.SerializeObject(payload);
						var content = new StringContent(json);
						var response = http.PostAsync(url, content).Result;
					}
				}
				catch (Exception e)
				{
					analytic.TrackException(e);
				}
			}
		}
	}
}