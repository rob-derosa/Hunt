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

using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
	public static class GetOngoingGame
	{
        [FunctionName(nameof(GetOngoingGame))]
		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetOngoingGame))]
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

                    var game = CosmosDataService.Instance.GetOngoingGame(email);
                    return req.CreateResponse(HttpStatusCode.OK, game as Object);
                }
                catch (Exception e)
                {
                    // track exceptions that occur
                    analytic.TrackException(e);

                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, e);
                }
            }
		}
	}
}