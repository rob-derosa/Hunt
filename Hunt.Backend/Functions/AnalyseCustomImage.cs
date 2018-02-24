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

using Newtonsoft.Json.Linq;
using Hunt.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
	public static class AnalyseCustomImage
	{
        [FunctionName(nameof(AnalyseCustomImage))]

		async public static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(AnalyseCustomImage))]
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
					{
						await EventHubService.Instance.SendEvent($"Custom image analyzed for treasure failed\n\tHint:\t\"{treasure.Hint}\"\n\tSource:\t{treasure.ImageSource}\n\tSent:\t{imageUrl.First()}");
						return req.CreateErrorResponse(HttpStatusCode.NotFound, "Treasure not found");
					}

					var endpoint = new PredictionEndpoint { ApiKey = ConfigManager.Instance.CustomVisionPredictionKey };

					//This is where we run our prediction against the default iteration
					var result = endpoint.PredictImageUrl(new Guid(game.CustomVisionProjectId), new ImageUrl(imageUrl));


					ImageTagPredictionModel goodTag = null;
					// Loop over each prediction and write out the results

					foreach (var prediction in result.Predictions)
					{
						if(treasure.Attributes.Any(a => a.Name.Equals(prediction.Tag, StringComparison.InvariantCultureIgnoreCase)))
						{
							if(prediction.Probability >= ConfigManager.Instance.CustomVisionMinimumPredictionProbability)
							{
								goodTag = prediction;
								break;
							}
						}
					}

					var outcome = goodTag == null ? "failed" : $"succeeded\n\tTag(s):\t\t{goodTag.Tag}\n\tProbability:\t{goodTag.Probability.ToString("P")}";
					await EventHubService.Instance.SendEvent($"Custom image analyzed for treasure {outcome}\n\tHint:\t\"{treasure.Hint}\"\n\tSource:\t{treasure.ImageSource}\n\tSent:\t{imageUrl}");
					return req.CreateResponse(HttpStatusCode.OK, goodTag != null);
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