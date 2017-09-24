using System;
using Newtonsoft.Json;

namespace Hunt.Common
{
	public class BaseModel
	{
		public BaseModel()
		{
			Id = Guid.NewGuid().ToString();
		}

		[JsonProperty("id")]
		public string Id { get; set; }

		//public override string ToString()
  //	  {
  //		  return JsonConvert.SerializeObject(this);
  //	  }
	}
}