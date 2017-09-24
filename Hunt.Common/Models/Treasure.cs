using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public class Treasure : BaseModel
	{
		[JsonProperty("hint")]
		public string Hint { get; set; }

		[JsonProperty("imageSource")]
		public string ImageSource { get; set; }

		[JsonProperty("attributes")]
		public List<Attribute> Attributes { get; set; } = new List<Attribute>();

		[JsonProperty("location")]
		public string Location { get; set; }

		[JsonProperty("isRequired")]
		public bool IsRequired { get; set; }

		[JsonProperty("points")]
		public int Points { get; set; }

		public override string ToString()
		{
			var sb = new StringBuilder();
			for(int i = 0; i < Attributes.Count; i++)
			{
				var a = Attributes[i];

				if(i < Attributes.Count - 1)
					sb.Append($", {a.Name}");
				else
					sb.Append($" and {a.Name}");
			}

			var str = sb.ToString().TrimStart(',').Trim();
            return "Find an object that is " + str;
		}
	}
}