using System.Net;
using System.Net.Http;
using System;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;

using Newtonsoft.Json;

using Hunt.Common;
using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
    public static class RegisterDevice
	{
        [FunctionName(nameof(RegisterDevice))]

		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(RegisterDevice))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(AnalyseText)
			}))
            {
				try
				{
					var json = req.Content.ReadAsStringAsync().Result;
					var registration = JsonConvert.DeserializeObject<DeviceRegistration>(json);
					var result = PushService.Instance.Register(registration).Result;
					return req.CreateResponse(HttpStatusCode.OK, result);
				}
				catch (Exception e)
				{
					analytic.TrackException(e.GetBaseException());
					return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.GetBaseException());
				}
            }
		}
	}
}