using System.Linq;
using System.Net;
using System.Net.Http;

using System;

using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

using Hunt.Common;

namespace Hunt.Backend.Functions
{
	public static class GetGame
	{
        [FunctionName(nameof(GetGame))]
		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetGame))]
		    HttpRequestMessage req, TraceWriter log)
		{
            using (var analytic = new AnalyticService(new RequestTelemetry
            {
                Name = nameof(GetGame)
            }))
            {
                try
                {
                    var gameId = req.GetQueryNameValuePairs().FirstOrDefault(kvp => kvp.Key == "id").Value;
                    var game = CosmosDataService.Instance.GetItemAsync<Game>(gameId).Result;
                    return req.CreateResponse(HttpStatusCode.OK, game);
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