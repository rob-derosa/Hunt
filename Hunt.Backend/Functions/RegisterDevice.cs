using System.Net;
using System.Net.Http;
using System;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;

using Newtonsoft.Json;

using Hunt.Common;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
    public static class RegisterDevice
	{
        [FunctionName(nameof(RegisterDevice))]

		async public static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = nameof(RegisterDevice))]
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

					var instance = registration.AppMode == AppMode.Dev ? PushService.Dev : PushService.Production;
					var result = instance.Register(registration).Result;

					var data = new Event("Registering device");
					data.Add("mode", registration.AppMode).Add("OS", registration.Platform).Add("handle", registration.Handle).Add("tags", string.Join(", ", registration.Tags).Trim());
					await EventHubService.Instance.SendEvent(data);

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