using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

using Hunt.Backend.Functions.Models;
using Hunt.Backend.Analytics;

namespace Hunt.Backend.Functions
{
	public static class GetStorageToken
	{
        static CloudStorageAccount _storageAccount;
		static CloudBlobClient _blobClient;
		static CloudBlobContainer _container;

		[FunctionName(nameof(GetStorageToken))]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = nameof(GetStorageToken))] 
                                                          HttpRequestMessage req, TraceWriter log)
		{
            using (var analytic = new Analytic(new RequestTelemetry
            {
                Name = nameof(GetStorageToken)
            }))
            {
                try
                {
                    var kvps = req.GetQueryNameValuePairs();
                    var blobName = kvps.FirstOrDefault(kvp => kvp.Key == "blobName").Value;

                    if (string.IsNullOrWhiteSpace(blobName))
                    {
                        var e = new ArgumentNullException(nameof(blobName));

                        // track exceptions that occur
                        analytic.TrackException(e);

                        throw e;
                    }

                    if (_storageAccount == null)
                        _storageAccount = CloudStorageAccount.Parse(Keys.Blob.SharedStorageKey);

                    if (_blobClient == null)
                        _blobClient = _storageAccount.CreateCloudBlobClient();

                    _container = _blobClient.GetContainerReference(Keys.Blob.ImageContainer);
                    await _container.CreateIfNotExistsAsync();

                    var sasUri = await GetSASToken(blobName);
                    var token = new StorageToken(blobName, sasUri);

                    return req.CreateResponse(HttpStatusCode.OK, token);
                }
                catch (Exception e)
                {
                    // track exceptions that occur
                    analytic.TrackException(e);

                    return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message, e);
                }
            }
        }

		/// <summary>
		/// Returns SAS token for a particular blob
		/// </summary>
		/// <param name="blobName"></param>
		/// <param name="policyName"></param>
		/// <returns></returns>
		public static async Task<string> GetSASToken(string blobName, string policyName = null)
		{
			await _container.CreateIfNotExistsAsync();
			string sasBlobToken = string.Empty;
			var blob = _container.GetBlockBlobReference(blobName);

			if (policyName == null)
			{
				SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
				{
					SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
					Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
				};

				sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);
			}
			else
			{
				sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
			}

			return  blob.Uri + sasBlobToken;
		}
	}
}