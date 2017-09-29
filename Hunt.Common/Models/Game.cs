using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public class Game : BaseDocument
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("entryCode")]
		public string EntryCode { get; set; }

		[JsonProperty("teamCount")]
		public int TeamCount { get; set; }

		[JsonProperty("playerCountPerTeam")]
		public int PlayerCountPerTeam { get; set; }
		
		[JsonProperty("startDate")]
		public DateTime? StartDate { get; set; } = null;

		[JsonProperty("endDate")]
		public DateTime? EndDate { get; set; } = null;

		[JsonProperty("createDate")]
		public DateTime? CreateDate { get; set; } = null;

		[JsonProperty("durationInMinutes")]
		public long DurationInMinutes { get; set; }

		[JsonProperty("coordinator")]
		public Player Coordinator { get; set; }

		[JsonProperty("teams")]
		public List<Team> Teams { get; set; } = new List<Team>();

		[JsonProperty("winningTeamId")]
		public string WinnningTeamId { get; set; }

		[JsonProperty("treasures")]
		public List<Treasure> Treasures { get; set; } = new List<Treasure>();

		[JsonIgnore]
		public bool HasEnded { get { return EndDate != null; } }

		[JsonIgnore]
		public bool HasStarted { get { return StartDate != null; } }

		[JsonIgnore]
		public bool IsRunning { get { return HasStarted && !HasEnded; } }

		[JsonIgnore]
		public bool IsPrepping { get { return !HasStarted && !HasEnded; } }

		[JsonIgnore]
		public int TotalPoints { get { return Treasures.Sum(t => t.Points); } }
	}
}