using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace Hunt.Backend.Functions
{
	public partial class VisionService
	{
		private VisionServiceClient _visionClient;

		static VisionService _instance;
		public static VisionService Instance
		{
			get { return _instance ?? (_instance = new VisionService()); }
		}

        public VisionService()
		{
			_visionClient = new VisionServiceClient(ConfigManager.Instance.VisionServiceKey, ConfigManager.Instance.VisionUrl);
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
	}
}