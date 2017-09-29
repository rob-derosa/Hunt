using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Hunt.Backend.Functions
{
	/// <summary>
	/// Queue service.
	/// </summary>
	public partial class VisionService : IDisposable
	{
		#region Private Properties

		static VisionService defaultInstance = new VisionService();

		private VisionServiceClient _visionClient;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Hunt.Mobile.Common.CosmosDataService"/> class.
        /// </summary>
        public VisionService()
		{
			_visionClient = new VisionServiceClient(Keys.Vision.ServiceKey, Keys.Vision.Url);
		}

		public async Task<AnalysisResult> GetImageDescriptionAsync(string url)
		{
			VisualFeature[] features = 
			{   
				VisualFeature.Adult, 
				VisualFeature.Categories, 
				VisualFeature.Color, 
				VisualFeature.Description, 
				VisualFeature.Faces, 
				VisualFeature.ImageType, 
				VisualFeature.Tags 
			};

            return await _visionClient.AnalyzeImageAsync(url, features.ToList());
		}

		public void Dispose()
		{
			_visionClient = null;
		}
	}
}