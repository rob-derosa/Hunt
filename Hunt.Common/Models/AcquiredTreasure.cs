using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public class AcquiredTreasure : BaseModel
	{
		[JsonProperty("treasureId")]
		public string TreasureId { get; set; }

		[JsonProperty("playerId")]
		public string PlayerId { get; set; }

		[JsonProperty("location")]
		public string Location { get; set; }

		[JsonProperty("imageSource")]
		public string ImageSource { get; set; }

		[JsonProperty("claimedTimeStamp")]
		public DateTime ClaimedTimeStamp { get; set; }

		[JsonProperty("claimedPoints")]
		public int ClaimedPoints { get; set; }

		[JsonIgnore]
		public DateTime ClaimedTimeStampLocal => ClaimedTimeStamp.ToLocalTime();
	}
}