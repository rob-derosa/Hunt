using Newtonsoft.Json;

namespace Hunt.Common
{
	public class BaseDocument : BaseModel
	{
		[JsonProperty("_rid")]
		public string RID { get; set; }

		[JsonProperty("_etag")]
		public string Etag { get; set; }

		[JsonProperty("_attachments")]
		public string Attachments { get; set; }

		[JsonProperty("_ts")]
		public long TS { get; set; }

		[JsonProperty("_self")]
		public string SelfLink { get; set; }

		[JsonIgnore]
		public bool IsPersisted => !string.IsNullOrWhiteSpace(SelfLink);
	}
}