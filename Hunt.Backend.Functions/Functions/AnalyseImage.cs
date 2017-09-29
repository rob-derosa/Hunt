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

namespace Hunt.Backend.Functions
{
    public static class AnalyseImage
	{
        [FunctionName(nameof(AnalyseImage))]

		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(AnalyseImage))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new Analytic(new RequestTelemetry
			{
				Name = nameof(AnalyseImage)
			}))
            {
				try
				{
					var allTags = new List<Tag>();
					var json = req.Content.ReadAsStringAsync().Result;
					var photoUrls = JsonConvert.DeserializeObject<string[]>(json);

					using (var visionClient = new VisionService())
					{
						foreach (var url in photoUrls)
						{
							var result = visionClient.GetImageDescriptionAsync(url).Result;
							allTags.AddRange(result.Tags);
						}
					}

					var attributes = new Dictionary<string, int>();
					foreach (var tag in allTags)
					{
						if (tag.Confidence > .5)
						{
							if (attributes.ContainsKey(tag.Name))
							{
								attributes[tag.Name]++;
							}
							else
							{
								attributes.Add(tag.Name, 1);
							}
						}
					}

					var topAttributes = attributes.OrderByDescending(k => k.Value);
					var toReturn = topAttributes.Select(k => k.Key).Take(10).ToArray();

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