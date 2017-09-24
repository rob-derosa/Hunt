using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;

using Microsoft.ApplicationInsights.DataContracts;

using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
	public static class PostMessageToQueue
	{
        [FunctionName(nameof(PostMessageToQueue))]
		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post",
														   Route = nameof(PostMessageToQueue))]
			HttpRequestMessage req, TraceWriter log)
		{
            using (var analytic = new Analytic(new RequestTelemetry
            {
                Name = nameof(PostMessageToQueue)
            }))
            {
                var image = req.GetObject<byte[]>();

                try
                {
                    using (var client = new QueueService("imageprocess"))
                    {
                        client.SendBrokeredMessageAsync(image).Wait();

                        return req.CreateResponse(HttpStatusCode.OK);
                    }
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