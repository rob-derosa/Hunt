
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace Hunt.Mobile.Common
{
	public class ConfigManager
	{
		public string AzureFunctionsUrl { get; set; }
		public string AzureStorageUrl { get; set; }
		public string AzureCDNUrl { get; set; }
		public string AppCenterAndroidToken { get; set; }
		public string AppCenteriOSToken { get; set; }

		public string CdnImagesBaseUrl => $"{AzureCDNUrl}/images";
		public string StorageAssetsBaseUrl => $"{AzureStorageUrl}/assets";
		public string StorageImagesBaseUrl => $"{AzureStorageUrl}/images";
		public string DefaultAvatarUrl => $"{StorageAssetsBaseUrl}/avatars/jon.jpg";

		static ConfigManager _instance;
		public static ConfigManager Instance => _instance ?? (_instance = new ConfigManager());

		public void Load()
		{
			var fileContents = GetFileContents("config.json");
			var props = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContents);

			AzureFunctionsUrl = props[nameof(AzureFunctionsUrl)];
			AzureStorageUrl = props[nameof(AzureStorageUrl)];
			AzureCDNUrl = props[nameof(AzureCDNUrl)];
			AppCenteriOSToken = props[nameof(AppCenteriOSToken)];
			AppCenterAndroidToken = props[nameof(AppCenterAndroidToken)];
		}

		string GetFileContents(string fileName)
		{
			var assembly = typeof(App).GetTypeInfo().Assembly;
			var name = assembly.ManifestModule.Name.Replace(".dll", string.Empty);
			var stream = assembly.GetManifestResourceStream($"{name}.{fileName}");

			if(stream == null)
				return null;

			string content;
			using(var reader = new StreamReader(stream))
				content = reader.ReadToEnd();

			return content;
		}
	}
}