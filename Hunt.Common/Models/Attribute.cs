using Newtonsoft.Json;

namespace Hunt.Common
{
	public class Attribute : BaseModel
	{
		public Attribute()
		{}

		public Attribute(string name)
		{
			Name = name;
		}

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("serviceType")]
		public CognitiveServiceType ServiceType { get; set; } = CognitiveServiceType.Vision;
	}
}
