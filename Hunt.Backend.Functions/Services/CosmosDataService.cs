using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Hunt.Common;
using System.Linq;

namespace Hunt.Backend.Functions
{
	public partial class CosmosDataService
	{
		const string _databaseId = @"Games";
		const string _collectionId = @"Items";
		DocumentClient _client;

		Uri _collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);

		public CosmosDataService()
		{
			_client = new DocumentClient(new Uri(Keys.Cosmos.Url), Keys.Cosmos.Key, ConnectionPolicy.Default);
		}

		static CosmosDataService _instance;
		public static CosmosDataService Instance
		{
			get { return _instance ?? (_instance = new CosmosDataService()); }
		}

		Uri GetCollectionUri()
		{
			return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
		}

		Uri GetDocumentUri(string id)
		{
			return UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
		}

		/// <summary>
		/// Ensures the database and collection are created
		/// </summary>
		async Task EnsureDatabaseConfigured()
		{
			var db = new Database { Id = _databaseId };
			var collection = new DocumentCollection { Id = _collectionId };

			var result = await _client.CreateDatabaseIfNotExistsAsync(db);

			if(result.StatusCode == HttpStatusCode.Created || result.StatusCode == HttpStatusCode.OK)
			{
				var dbLink = UriFactory.CreateDatabaseUri(_databaseId);
				var colResult = await _client.CreateDocumentCollectionIfNotExistsAsync(dbLink, collection);
			}
		}

		/// <summary>
		/// Fetches an item based on it's Id
		/// </summary>
		/// <returns>The serialized item object</returns>
		/// <param name="id">The Id of the item to retrieve</param>
		public async Task<T> GetItemAsync<T>(string id) where T : BaseModel, new()
		{
			var docUri = GetDocumentUri(id);
			var result = await _client.ReadDocumentAsync<T>(docUri);

			if(result.StatusCode == HttpStatusCode.OK)
			{
				return result.Document;
			}

			return null;
		}

		/// <summary>
		/// Inserts the document into the collection and creates the database and collection if it has not yet been created
		/// </summary>
		/// <param name="item">The item to add</param>
		public async Task InsertItemAsync<T>(T item) where T : BaseModel
		{
			try
			{
				var result = await _client.CreateDocumentAsync(_collectionLink, item);
				item.Id = result.Resource.Id;
			}
			catch(DocumentClientException dce)
			{
				if(dce.StatusCode == HttpStatusCode.NotFound)
				{
					await EnsureDatabaseConfigured();
					await InsertItemAsync(item);
				}
			}
		}

		/// <summary>
		/// Updates the document
		/// </summary>
		/// <param name="item">The item to update</param>
		public async Task UpdateItemAsync<T>(T item) where T : BaseModel
		{
			await _client.ReplaceDocumentAsync(GetDocumentUri(item.Id), item);
		}

		public Game GetGameByEntryCode(string entryCode)
		{
			var query = _client.CreateDocumentQuery<Game>(GetCollectionUri()).Where(g => g.EntryCode == entryCode);

			foreach (var game in query)
				return game;

			return null;
		}

		/// <summary>
		/// Returns the first open/existing game that is either coordinated or joined by the email provided
		/// </summary>
		/// <param name="email">The players's email address</param>
		/// <returns>A dynamic Game, should one exist</returns>
		public dynamic GetOngoingGame(string email)
		{
			{
				//First lets check to see if the user has a game they're coordinating
				var sql = $"SELECT * FROM game WHERE game.endDate = null AND game.coordinator.email = \"{email}\"";
				var query = _client.CreateDocumentQuery(GetCollectionUri(), sql);

				foreach (var game in query)
				{
					return game;
				}
			}

			{
				//Now check to see if the the user is part of an onging game as a player
				var sql = "SELECT game FROM game " +
					"JOIN teams IN game.teams " +
					"JOIN player IN teams.players " +
					$"WHERE game.endDate = null AND player.email = \"{email}\"";
				var query = _client.CreateDocumentQuery(GetCollectionUri(), sql);

				foreach (var game in query)
				{
					return game.game;
				}
			}

			return null;
		}
	}
}
