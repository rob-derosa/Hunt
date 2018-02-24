using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hunt.Common
{
	public class Event
	{
		public Event(string title)
		{
			Title = title;
		}

		public Event Add(string key, object value)
		{
			Metadata.Add(key, value);
			return this;
		}

		public string Title { get; set; }
		public string Exception { get; set; }
		public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
	}
}
