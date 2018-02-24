using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ApplicationInsights.DataContracts;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
	public static class WakeUp
	{
		[FunctionName(nameof(WakeUp))]
		async public static Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(WakeUp))]
			HttpRequestMessage req, TraceWriter log)
		{
			using (var analytic = new AnalyticService(new RequestTelemetry
			{
				Name = nameof(WakeUp)
			}))
			{
				await EventHubService.Instance.SendEvent($"Wake request received");
				return req.CreateResponse(HttpStatusCode.OK);
			}
		}
	}
}