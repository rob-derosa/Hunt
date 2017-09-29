using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public class Player : BaseModel
	{
		[JsonProperty("email")]
		public string Email { get; set; }
		
		[JsonProperty("firstName")]
		public string FirstName { get; set; }

		[JsonProperty("lastName")]
		public string LastName { get; set; }

		[JsonProperty("avatar")]
		public string Avatar { get; set; }

		[JsonProperty("alias")]
		public string Alias { get; set; }

		[JsonProperty("installId")]
		public string InstallId { get; set; }
	}
}
