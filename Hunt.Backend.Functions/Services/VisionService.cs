using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Hunt.Backend.Functions
{
	public partial class VisionService : IDisposable
	{
		#region Private Properties

		static VisionService defaultInstance = new VisionService();

		private VisionServiceClient _visionClient;

        #endregion

        public VisionService()
		{
			_visionClient = new VisionServiceClient(Keys.Vision.ServiceKey, Keys.Vision.Url);
		}

		public async Task<AnalysisResult> GetImageDescriptionAsync(string url)
		{
			VisualFeature[] features = 
			{   
				VisualFeature.Description, 
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