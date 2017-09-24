using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
	public class MobileCenterNotification
	{
		[JsonProperty("notification_target")]
		public NotificationTarget Target { get; set; } = new NotificationTarget();

		[JsonProperty("notification_content")]
		public NotificationContent Content { get; set; } = new NotificationContent();
	}

	public class NotificationTarget
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("devices")]
		public string[] Devices { get; set; }

		[JsonProperty("audiences")]
		public List<string> Audiences { get; set; } = new List<string>();
	}

	public class NotificationContent
	{
		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("body")]
		public string Body { get; set; }

		[JsonProperty("custom_data")]
		public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
	}

	public class Audience
	{
		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("definition")]
		public string Definition { get; set; }

		[JsonProperty("custom_properties")]
		public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
	}

	public class TargetType
	{
		public static readonly string Audience = "audiences_target";
		public static readonly string Device = "devices_target";
	}
}
