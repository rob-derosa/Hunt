using System.Net;
using System.Net.Http;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;

using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
	public static class SetEndGame
	{
		[FunctionName(nameof(SetEndGame))]

        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(SetEndGame))]
            HttpRequestMessage req, TraceWriter log)
        {
			using (var analytic = new Analytic(new RequestTelemetry
			{
				Name = nameof(SetEndGame)
			}))
			{
				try
				{
					var gameId = req.GetQueryNameValuePairs().FirstOrDefault(kvp => kvp.Key == "gameId").Value;
					var minutes = req.GetQueryNameValuePairs().FirstOrDefault(kvp => kvp.Key == "minutes").Value;
					var minVal = Convert.ToInt32(minutes);

					using (var client = new QueueService(Keys.ServiceBus.EndGameBusName))
					{
						client.SendBrokeredMessageAsync(minVal, gameId, "endgametime", minVal).Wait();
						return req.CreateResponse(HttpStatusCode.OK);
					}
				}
				catch (Exception e)
				{
					analytic.TrackException(e);

					return req.CreateErrorResponse(HttpStatusCode.BadRequest, e);
				}
			}
		}
	}
}