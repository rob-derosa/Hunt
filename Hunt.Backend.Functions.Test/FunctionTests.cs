using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

using Newtonsoft.Json;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Hunt.Common;
using System;
using Newtonsoft.Json.Linq;

namespace Hunt.Backend.Functions.Test
{
	[TestClass]
	public class FunctionTests
	{
		private string _gameId;
        private string _email;
        private string _entryCode;

        private string _functionBaseUrl = "https://huntapp.azurewebsites.net/api/{0}";

        [TestMethod]
        public async Task CreateAndSaveGame()
        {
            var bronn = new Player()
            {
                Email = "bronn@gameofthrones.com",
                Alias = "Bronn",
                Avatar = "http://huntappstorage.blob.core.windows.net/images/avatars/bronn.jpg"
            };

            var game = new Game
            {
                TeamCount = 4,
                CreateDate = DateTime.UtcNow,
                PlayerCountPerTeam = 4,
                Coordinator = bronn,
                Name = string.Format($"New Game by {0}", bronn.Alias),
                DurationInMinutes = 60,
            };

            dynamic payload = new JObject();
            payload.action = GameUpdateAction.Create;
            payload.arguments = null;
            payload.game = JObject.FromObject(game);

            for (int i = 0; i < game.TeamCount; i++)
            {
                var team = new Team
                {
                    Name = $"Team {i + 1}",
                };

                game.Teams.Add(team);
            }

            var result = await SendPostRequest("SaveGame", payload) as HttpResponseMessage;
            var json = await result.Content.ReadAsStringAsync();
            var savedGame = JsonConvert.DeserializeObject<Game>(json);

            // store game details locally for other tests
            _gameId = savedGame.Id;
            _email = savedGame.Coordinator.Email;
            _entryCode = savedGame.EntryCode;

            Debug.Assert(result.StatusCode == HttpStatusCode.OK);
        }

        [TestMethod]
		public async Task AnalyseImage()
		{
			var result = await SendPostRequest("AnalyseImage", new string[]
            {
                "https://huntappstorage.blob.core.windows.net/images/avatars/arya.jpg",
                "https://huntappstorage.blob.core.windows.net/images/avatars/arya.jpg",
                "https://huntappstorage.blob.core.windows.net/images/avatars/arya.jpg",
            });

			// temp, for now just testing if we receive 200 A'OKAY
			Debug.Assert(result.StatusCode == HttpStatusCode.OK);
		}

        [TestMethod]
        public async Task AnalyseText()
        {
            var result = await SendPostRequest("AnalyseText?text=This test is absolute crap.", string.Empty);

            Debug.Assert(result.StatusCode == HttpStatusCode.OK);
        }

        [TestMethod]
		public async Task GeStorageToken()
		{
			var result = await SendGetRequest("GetStorageToken?blobName=avatars/jon.jpg");

            Debug.Assert(result == HttpStatusCode.OK);
        }

        [TestMethod]
		public async Task PostMessageToQueue()
		{
            var result = await SendPostRequest("PostMessageToQueue", string.Empty);

            Debug.Assert(result.StatusCode == HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetGame()
        {
            var result = await SendGetRequest(string.Format("GetGame?id={0}", _gameId));

            Debug.Assert(result == HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetGameByEntryCode()
        {
            var result = await SendGetRequest(string.Format("GetGameByEntryCode?email={0}&entryCode", _email, _entryCode));

            Debug.Assert(result == HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task GetOngoingGame()
        {
            var result = await SendGetRequest("GetOngoingGame?email=test");

            Debug.Assert(result == HttpStatusCode.OK);
        }


        [TestMethod]
        public async Task SetEndGame()
        {
            var result = await SendGetRequest(string.Format("SetEndGame?minutes=1&gameId={0}", _gameId));

            Debug.Assert(result == HttpStatusCode.OK);
        }

        [TestMethod]
        public async Task EndGame()
        {
            /*var result = Backend.Triggers.EndGame.Run();

            Debug.Assert(result == HttpStatusCode.OK);*/
        }

        async Task<HttpStatusCode> SendGetRequest(string apiCall, IDictionary<string, string> headers = null)
		{
            var url = string.Format(_functionBaseUrl, apiCall);

            using (var client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var key in headers.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key, headers[key]);
                    }
                }

                HttpResponseMessage response;

                response = await client.GetAsync(url);

                return response.StatusCode;
            }
        }

		async Task<HttpResponseMessage> SendPostRequest(string apiCall, object body, IDictionary<string, string> headers = null)
		{
            var url = string.Format(_functionBaseUrl, apiCall);

            using (var client = new HttpClient())
            {
                if (headers != null)
                {
                    foreach (var key in headers.Keys)
                    {
                        client.DefaultRequestHeaders.Add(key, headers[key]);
                    }
                }                

                HttpResponseMessage response;

                var jsonIn = JsonConvert.SerializeObject(body);

                using (var content = new StringContent(jsonIn))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(url, content);
                    return response;
                }
            }
		}
	}
}
