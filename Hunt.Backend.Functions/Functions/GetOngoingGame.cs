using System.Linq;
using System.Net;
using System.Net.Http;
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

using System.Threading.Tasks;
using Newtonsoft.Json;
using Hunt.Common;

namespace Hunt.Backend.Functions
{
	public static class GetOngoingGame
	{
        [FunctionName(nameof(GetOngoingGame))]
		async public static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetOngoingGame))]
		HttpRequestMessage req, TraceWriter log)
		{
            using (var analytic = new AnalyticService(new RequestTelemetry
            {
                Name = nameof(GetOngoingGame)
            }))
            {
                try
                {
                    var email = req.GetQueryNameValuePairs().FirstOrDefault(kvp => kvp.Key == "email").Value;

                    if (string.IsNullOrWhiteSpace(email))
                    {
                        return req.CreateErrorResponse(HttpStatusCode.BadRequest, "email address required");
                    }

                    var json = CosmosDataService.Instance.GetOngoingGame(email);
					var game = json == null ? null : JsonConvert.DeserializeObject<Game>(json.ToString()) as Game;
					var outcome = json == null ? "no games found" : "a game found";
					await EventHubService.Instance.SendEvent($"Looking for an ongoing game resulted in {outcome}\n\tEmail:\t{email}", game, null);
					return req.CreateResponse(HttpStatusCode.OK, game);
                }
                catch (Exception e)
                {
                    // track exceptions that occur
                    analytic.TrackException(e.GetBaseException());
					var msg = e.GetBaseException().Message;
                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.GetBaseException().Message);
                }
            }
		}
	}
}