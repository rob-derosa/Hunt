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
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(AnalyseImage)
			}))
            {
				try
				{
					var allTags = new List<string>();
					var json = req.Content.ReadAsStringAsync().Result;
					var photoUrls = JsonConvert.DeserializeObject<string[]>(json);

					foreach (var url in photoUrls)
					{
						var result = VisionService.Instance.GetImageDescriptionAsync(url).Result;
						allTags.AddRange(result.Tags.Select(t => t.Name).ToArray());
						allTags.AddRange(result.Description.Tags);
					}

					var attributes = new Dictionary<string, int>();
					foreach (var tag in allTags)
					{
						if (tag.Equals("indoor", StringComparison.InvariantCultureIgnoreCase) ||
							tag.Equals("outdoor", StringComparison.InvariantCultureIgnoreCase))
							continue;

						if (attributes.ContainsKey(tag))
							attributes[tag]++;
						else
							attributes.Add(tag, 1);
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