using System.Net;
using System.Net.Http;
using System;
using System.Collections.Generic;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;

using Hunt.Backend.Analytics;
using Newtonsoft.Json.Linq;
using Hunt.Common;
using System.Linq;

namespace Hunt.Backend.Functions
{
	public static class AnalyseCustomImage
	{
        [FunctionName(nameof(AnalyseCustomImage))]

		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(AnalyseCustomImage))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(AnalyseCustomImage)
			}))
            {
				try
				{
					var allTags = new List<string>();
					var j = JObject.Parse(req.Content.ReadAsStringAsync().Result);
					var imageUrl = (string)j["imageUrl"];
					var treasureId = (string)j["treasureId"];
					var gameId = (string)j["gameId"];

					var game = CosmosDataService.Instance.GetItemAsync<Game>(gameId).Result;
					if (game == null)
						return req.CreateErrorResponse(HttpStatusCode.NotFound, "Game not found");

					var treasure = game.Treasures.SingleOrDefault(t => t.Id == treasureId);
					if(treasure == null)
						return req.CreateErrorResponse(HttpStatusCode.NotFound, "Treasure not found");

                    var endpoint = new PredictionEndpoint { ApiKey = ConfigManager.Instance.CustomVisionPredictionKey };

					//This is where we run our prediction against the default iteration
					var result = endpoint.PredictImageUrl(new Guid(game.CustomVisionProjectId), new ImageUrl(imageUrl));

					bool toReturn = false;
					// Loop over each prediction and write out the results
					foreach(var outcome in result.Predictions)
					{
						if(treasure.Attributes.Any(a => a.Name.Equals(outcome.Tag, StringComparison.InvariantCultureIgnoreCase)))
						{
							if(outcome.Probability >= ConfigManager.Instance.CustomVisionMinimumPredictionProbability)
							{
								toReturn = true;
								break;
							}
						}
					}

					return req.CreateResponse(HttpStatusCode.OK, toReturn);
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