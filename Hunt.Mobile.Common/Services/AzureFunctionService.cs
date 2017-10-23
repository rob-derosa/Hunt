using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Hunt.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hunt.Mobile.Common
{
	#region FunctionNames

	public struct FunctionNames
	{
		public static string WakeUp = nameof(WakeUp);
		public static string SaveImage = nameof(SaveImage);
		public static string GetStorageToken = nameof(GetStorageToken);
		public static string AnalyseText = nameof(AnalyseText);
		public static string AnalyseImage = nameof(AnalyseImage);
		public static string SaveGame = nameof(SaveGame);
		public static string GetGame = nameof(GetGame);
		public static string DeleteGame = nameof(DeleteGame);
		public static string GetOngoingGame = nameof(GetOngoingGame);
		public static string JoinGame = nameof(JoinGame);
		public static string RegisterDevice = nameof(RegisterDevice);
		public static string GetGameByEntryCode = nameof(GetGameByEntryCode);
	}

	#endregion

	public class AzureFunctionService
	{
		#region Properties

		HttpClient _client;

		HttpClient Client
		{
			get
			{
				return _client ?? (_client = GetHttpClient());
			}
		}

		HttpClient GetHttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders
				  .Accept
				  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

			return client;
		}

		#endregion

		#region Low Level Requests

		async Task<T> SendGetRequest<T>(string functionName, IDictionary<string, object> parameters = null)
		{
			try
			{
				var url = functionName.ToUrl(parameters);
				Debug.WriteLine($"GET URL: {url}");

				var json = await Client.GetStringAsync(url);
				var returnValue = JsonConvert.DeserializeObject<T>(json);
				Debug.WriteLine($"GET OUTPUT:\n{json}");

#if DEBUG
				await Task.Delay(200);
#endif

				return returnValue;
			}
			catch(Exception e)
			{
				Logger.Instance.WriteLine(e);
				throw;
			}
		}

		async Task<T> SendPostRequest<T>(string url, object body)
		{
			try
			{
				Debug.WriteLine($"POST URL: {url}");

#if DEBUG
				await Task.Delay(200).ConfigureAwait(false);
#endif
				var jsonIn = JsonConvert.SerializeObject(body);
				Debug.WriteLine($"POST INPUT:\n{jsonIn}");
				var content = new StringContent(jsonIn);
				var resp = await Client.PostAsync(url, content).ConfigureAwait(false);

				var jsonOut = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
				Debug.WriteLine($"POST OUTPUT:\n{jsonOut}");
				var errorMessage = resp.ReasonPhrase;

				if(resp.StatusCode == HttpStatusCode.Conflict)
				{
					var jobject = JObject.Parse(jsonOut);
					var msg = jobject["Message"].ToString();
					throw new DocumentVersionConclictException(msg, body as BaseDocument);
				}

				if(resp.StatusCode == HttpStatusCode.BadRequest)
				{
					var jobject = JObject.Parse(jsonOut);
					errorMessage = jobject["Message"].ToString();
				}

				if(resp.StatusCode == HttpStatusCode.OK)
				{
					var returnValue = JsonConvert.DeserializeObject<T>(jsonOut);
					return returnValue;
				}

				throw new HttpRequestException(errorMessage);
			}
			catch(Exception e)
			{
				Logger.Instance.WriteLine(e);
				throw;
			}
		}

#endregion

		async public Task<Game> GetGame(string gameId)
		{
			if(string.IsNullOrEmpty(gameId))
				return null;

			var result = await SendGetRequest<Game>(FunctionNames.GetGame, new Dictionary<string, object> { { "id", gameId } });
			return result;
		}

		async public Task<Game> GetOngoingGame(string email)
		{
			var games = await SendGetRequest<Game>(nameof(FunctionNames.GetOngoingGame),
				new Dictionary<string, object> { { "email", email } } );

			return games;
		}

		async public Task<Game> SaveGame(Game game, string action, IDictionary<string, string> args = null)
		{
			if(args == null)
				args = new KVP();

			if(!args.ContainsKey("playerId"))
				args.Add("playerId", App.Instance.Player.Id);
	
			dynamic payload = new JObject();
			payload.action = action;
			payload.arguments = JObject.FromObject(args);
			payload.game = JObject.FromObject(game);

			var url = nameof(FunctionNames.SaveGame).ToUrl();
			var result = await SendPostRequest<Game>(url, payload);
			return result;
		}

		async public Task<string> RegisterDevice(string deviceToken, string playerId)
		{
			var registration = new DeviceRegistration
			{
				Handle = deviceToken,
				Platform = Xamarin.Forms.Device.RuntimePlatform,
				Tags = new[] { playerId }
			};

			var url = nameof(FunctionNames.RegisterDevice).ToUrl();
			var result = await SendPostRequest<string>(url, registration);
			return result;
		}

		async public Task<Game> GetGameByEntryCode(string entryCode)
		{
			var result = await SendGetRequest<Game>(FunctionNames.GetGameByEntryCode,
				new Dictionary<string, object> { { "email", App.Instance.Player.Email },
				{ "entryCode", entryCode } }).ConfigureAwait(false);

			return result;
		}

		async public Task<string[]> AnalyseImage(string[] imageIds)
		{
			try
			{
				var url = nameof(FunctionNames.AnalyseImage).ToUrl();
				var result = await SendPostRequest<string[]>(url, imageIds);
				return result;
			}
			catch(Exception e)
			{
				Logger.Instance.WriteLine(e);
			}

			return null;
		}

		async public Task<string> GetStorageToken(string blobId)
		{
			var result = await SendGetRequest<JObject>(nameof(FunctionNames.GetStorageToken), new Dictionary<string, object> { { "blobName", blobId } });
			if(result.TryGetValue("SasUri", out JToken jtoken))
			{
				return jtoken.Value<string>();
			}

			throw new Exception("Unable to generate SAS token");
		}

		async public Task<bool> IsGravatarValid(string email)
		{
			var task = new Task<bool>(() =>
			{
				var resp = Client.GetAsync(email.ToGravatarUrl(20)).Result;
				var contentLength = resp.Content.Headers.ContentLength;
				return contentLength != 1040;
			});

			await task.RunProtected();

			if(task.WasSuccessful())
				return task.Result;

			return false;
		}

		async public Task WakeUpServer()
		{
			var result = await SendGetRequest<string>(nameof(FunctionNames.WakeUp), null);
		}

		async public Task<bool> IsTextValidAppropriate(string text)
		{
			var result = await SendGetRequest<bool>(nameof(FunctionNames.AnalyseText), new Dictionary<string, object> { { "text", text } });
			return result;
		}
	}

	#region Utilities

	static class FunctionExtensions
	{
		public static string ToUrl(this string s, IDictionary<string, object> p = null)
		{
			var qs = p == null ? string.Empty : "?" + string.Join("&", p.Select((x) => x.Key + "=" + x.Value));
			return string.Format($"{Keys.Azure.FunctionsUrl}/{s}{qs}");
		}
	}

	public class DocumentVersionConclictException : Exception
	{
		public DocumentVersionConclictException(string message, BaseDocument doc) : base(message)
		{
			Document = doc;
		}

		public BaseDocument Document { get; set; }
	}

	#endregion
}