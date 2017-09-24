using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public class Team : BaseModel
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("avatar")]
		public string Avatar { get; set; }

		[JsonProperty("players")]
		public List<Player> Players { get; set; } = new List<Player>();

		[JsonProperty("acquiredTreasure")]
		public List<AcquiredTreasure> AcquiredTreasure { get; set; } = new List<AcquiredTreasure>();

		[JsonIgnore]
		public int TotalPoints { get { return AcquiredTreasure.Sum(t => t.ClaimedPoints); } }
	}
}
