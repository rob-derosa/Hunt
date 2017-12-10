using System.Net;
using System.Net.Http;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;

using Newtonsoft.Json;

using Hunt.Common;
using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
    public static class AnalyseText
	{
        [FunctionName(nameof(AnalyseText))]

		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(AnalyseText))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(AnalyseText)
			}))
            {
				try
				{
                    var text = req.GetQueryNameValuePairs().FirstOrDefault(kvp => kvp.Key == "text").Value;

                    using (var client = new HttpClient())
                    {
                        var queryString = HttpUtility.ParseQueryString(string.Empty);

                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigManager.Instance.ContentModeratorKey);

                        queryString["autocorrect"] = "{boolean}";
                        queryString["PII"] = "{boolean}";
                        queryString["listId"] = "{string}";

                        var uri = ConfigManager.Instance.ContentModeratorUrl + queryString;
                        HttpResponseMessage response;
                        byte[] byteData = Encoding.UTF8.GetBytes(text);

                        using (var content = new ByteArrayContent(byteData))
                        {
                            content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                            response = client.PostAsync(uri, content).Result;

                            var json = response.Content.ReadAsStringAsync().Result;
                            var textModeration = JsonConvert.DeserializeObject<TextModeration>(json);

                            return req.CreateResponse(HttpStatusCode.OK, textModeration.Terms == null);
                        }
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