using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Hunt.Common;
using System.Collections.Generic;
using System.Linq;

namespace Hunt.Backend.Functions
{
	public partial class CosmosDataService : IDisposable
	{
		static CosmosDataService defaultInstance = new CosmosDataService();

		const string _databaseId = @"Games";
		const string _collectionId = @"Items";

		Uri _collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
		DocumentClient _client;

		public CosmosDataService()
		{
			_client = new DocumentClient(new Uri(Keys.Cosmos.Url), Keys.Cosmos.Key, ConnectionPolicy.Default);
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
			try
			{
				var docUri = GetDocumentUri(id);
				var result = await _client.ReadDocumentAsync<T>(docUri);

				if(result.StatusCode == HttpStatusCode.OK)
				{
					return result.Document;
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(@"ERROR {0}", e.Message);
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
			catch(Exception e)
			{
				Console.Error.WriteLine(@"ERROR {0}", e.Message);
			}
		}

		/// <summary>
		/// Updates the document
		/// </summary>
		/// <param name="item">The item to update</param>
		public async Task UpdateItemAsync<T>(T item) where T : BaseModel
		{
			try
			{
				await _client.ReplaceDocumentAsync(GetDocumentUri(item.Id), item);
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(@"ERROR {0}", e.Message);
			}
		}

		public Game GetGameByEntryCode(string entryCode)
		{
			try
			{
				var query = _client.CreateDocumentQuery<Game>(GetCollectionUri()).Where(g => g.EntryCode == entryCode);

				foreach (var game in query)
					return game;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(@"ERROR {0}", e);
			}

			return null;
		}

		/// <summary>
		/// Returns the first open/existing game that is either coordinated or joined by the email provided
		/// </summary>
		/// <param name="email">The players's email address</param>
		/// <returns>A dynamic Game, should one exist</returns>
		public dynamic GetOngoingGame(string email)
		{
			try
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
			}
			catch(Exception e)
			{
				Console.Error.WriteLine(@"ERROR {0}", e);
			}

			return null;
		}

		public void Dispose()
		{
			_client?.Dispose();
			_client = null;
		}
	}
}
