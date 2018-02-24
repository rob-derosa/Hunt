using Hunt.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Hunt.Backend.Functions
{
	public class Http
	{
		static System.Net.Http.HttpClient _client;
		public static HttpClient Client
		{
			get
			{
				return _client ?? (_client = GetHttpClient());
			}
		}

		static HttpClient GetHttpClient()
		{
			return new HttpClient().AddJsonHeader();
		}
		}

	public static class Extensions
	{
		public static async Task<T> Put<T>(this HttpClient client, string url, object body)
		{
			try
			{
				var jsonIn = JsonConvert.SerializeObject(body);
				Debug.WriteLine($"URL:\n{url}");
				Debug.WriteLine($"INPUT:\n{jsonIn}");

				var content = new StringContent(jsonIn, Encoding.UTF8, "application/json");
				var resp = await client.PutAsync(url, content);

				var jsonOut = await resp.Content.ReadAsStringAsync();
				Debug.WriteLine($"OUTPUT:\n{jsonOut}");

				var returnValue = JsonConvert.DeserializeObject<T>(jsonOut);
				return returnValue;
			}
			catch (Exception e)
			{
				//TODO: log error
				Debug.WriteLine(e);
				throw;
			}
		}

		public static async Task<T> Post<T>(this HttpClient client, string url, object body)
		{
			try
			{
				var jsonIn = JsonConvert.SerializeObject(body);
				Debug.WriteLine($"URL:\n{url}");
				Debug.WriteLine($"INPUT:\n{jsonIn}");

				var content = new StringContent(jsonIn, Encoding.UTF8, "application/json");
				var resp = await client.PostAsync(url, content);

				var jsonOut = await resp.Content.ReadAsStringAsync();
				Debug.WriteLine($"OUTPUT:\n{jsonOut}");

				var returnValue = JsonConvert.DeserializeObject<T>(jsonOut);
				return returnValue;
			}
			catch (Exception e)
			{
				//TODO: log error
				Debug.WriteLine(e);
				throw;
			}
		}

		public static async Task<T> Get<T>(this HttpClient client, string url)
		{
			try
			{
				var json = await client.GetStringAsync(url);
				var returnValue = JsonConvert.DeserializeObject<T>(json);
				return returnValue;
			}
			catch (Exception e)
			{
				//TODO: log error
				Debug.WriteLine(e);
				throw;
			}
		}

		public static T GetObject<T>(this HttpRequestMessage req)
		{
			try
			{
				//For some reason, the BaseDocument properties are not being deserialized
				var json = req.Content.ReadAsStringAsync().Result;
				var jobject = JsonConvert.DeserializeObject<JObject>(json);
				var toReturn = JsonConvert.DeserializeObject<T>(json);

				if (typeof(T).IsSubclassOf(typeof(BaseDocument)))
				{
					var doc = toReturn as BaseDocument;
					doc.SelfLink = jobject["_self"]?.ToString();
					doc.RID = jobject["_rid"]?.ToString();
					doc.Etag = jobject["_etag"]?.ToString();
					doc.TS = (long)jobject["_ts"];
				}

				return toReturn;
			}
			catch (Exception e)
			{
				//TODO: log here
				Debug.WriteLine(e);
			}

			return default(T);
		}

		public static HttpClient AddJsonHeader(this HttpClient client)
		{
			client.DefaultRequestHeaders
				  .Accept
				  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

			return client;
		}

		public static string Encode(this string s)
		{
			return WebUtility.UrlEncode(s);
		}
	}
}
