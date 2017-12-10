using System.Net;
using System.Net.Http;
using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ProjectOxford.Vision.Contract;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

using Newtonsoft.Json;

using Hunt.Backend.Analytics;
using Microsoft.Cognitive.CustomVision;
using Newtonsoft.Json.Linq;
using Hunt.Common;
using Microsoft.Cognitive.CustomVision.Models;

namespace Hunt.Backend.Functions
{
    public static class TrainClassifier
	{
        [FunctionName(nameof(TrainClassifier))]

		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(TrainClassifier))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(TrainClassifier)
			}))
            {
				try
				{
					var allTags = new List<string>();
					var json = req.Content.ReadAsStringAsync().Result;
					var j = JObject.Parse(json);
					var gameId = (string)j["gameId"];
					var imageUrls = j["imageUrls"].ToObject<List<string>>();
					var tags = (JArray)j["tags"];

					var game = CosmosDataService.Instance.GetItemAsync<Game>(gameId).Result;

					var api = new TrainingApi(new TrainingApiCredentials(ConfigManager.Instance.CustomVisionTrainingKey));
					ProjectModel project = null;
					
					//Get the existing project for this game if there is one
					if(!string.IsNullOrEmpty(game.CustomVisionProjectId))
						project = api.GetProject(Guid.Parse(game.CustomVisionProjectId));
					
					//Otherwise create a new project and associate it with the game
					if(project == null)
					{
						project = api.CreateProject(game.Name, game.Id);
						game.CustomVisionProjectId = project.Id.ToString();
						CosmosDataService.Instance.UpdateItemAsync<Game>(game).Wait();
					}

					//Generate tag models for training
					var tagModels = new List<ImageTagModel>();
					foreach(string tag in tags)
					{
						var model = api.CreateTag(project.Id, tag);
						tagModels.Add(model);
					}

					//Batch the image urls that were sent up from Azure Storage (blob)
					var batch = new ImageUrlCreateBatch(tagModels.Select(m => m.Id).ToList(), imageUrls);
					var summary = api.CreateImagesFromUrls(project.Id, batch);

					if(!summary.IsBatchSuccessful)
						return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Image batch was unsuccessful");

					//Traing the classifier and generate a new iteration, that we'll set as the default
					var iteration = api.TrainProject(project.Id);
					iteration.IsDefault = true;
					api.UpdateIteration(project.Id, iteration.Id, iteration);

					return req.CreateResponse(HttpStatusCode.OK, true);
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