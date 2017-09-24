using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;
using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
	public static class WakeUp
	{
		[FunctionName(nameof(WakeUp))]
		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(WakeUp))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new Analytic(new RequestTelemetry
			{
				Name = nameof(WakeUp)
			}))
			{
				return req.CreateResponse(HttpStatusCode.OK);
			}
		}
	}
}