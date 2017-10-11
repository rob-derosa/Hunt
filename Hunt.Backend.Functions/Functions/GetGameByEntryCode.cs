using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
	public static class GetGameByEntryCode
	{
        [FunctionName(nameof(GetGameByEntryCode))]
		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetGameByEntryCode))]
		HttpRequestMessage req, TraceWriter log)
		{
            using (var analytic = new AnalyticService(new RequestTelemetry
            {
                Name = nameof(GetGameByEntryCode)
            }))
            {
                try
                {
                    var kvps = req.GetQueryNameValuePairs();
                    var email = kvps.FirstOrDefault(kvp => kvp.Key == "email").Value;
                    var entryCode = kvps.FirstOrDefault(kvp => kvp.Key == "entryCode").Value;

                    using (var client = new CosmosDataService())
                    {
                        var existingGame = client.GetOngoingGame(email);

                        if (existingGame != null)
							return req.CreateErrorResponse(HttpStatusCode.Conflict, "User already has an ongoing game");

						var openGame = client.GetGameByEntryCode(entryCode);

                        return req.CreateResponse(HttpStatusCode.OK, openGame);
                    }
                }
                catch (Exception e)
                {
                    // track exceptions that occur
                    analytic.TrackException(e);
                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message, e);
                }
            }
		}
	}
}