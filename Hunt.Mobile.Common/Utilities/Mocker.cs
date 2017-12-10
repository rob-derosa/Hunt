using System;
using System.Collections.Generic;
using Hunt.Common;
using Attribute = Hunt.Common.Attribute;

namespace Hunt.Mobile.Common
{
	public static class Mocker
	{
		public static Game GetGame(int teamCount, int playerCount, bool mockCoordinator, bool mockPlayers, bool mockTreasure, bool mockAcquiredTreasure, bool mockSettings = false)
		{
			var ned = new Player
			{
				Alias = "Ned",
				Email = "ned@hbo.com",
				Avatar = $"{ConfigManager.Instance.StorageAssetsBaseUrl}/avatars/ned.jpg",
			};

			var teams = new List<string[]>
			{
				new[] { "Lannister", "Cersei", "Jamie", "Joffrey", "Tyrion" },
				new[] { "Stark", "Jon", "Sansa" },
				new[] { "Targaryen", "Dany", "Viserys" },
				new[] { "Greyjoy", "Yara", "Theon", "Euron" },
				new[] { "Bolton", "Ramsay", "Arya" },
			};

			var game = new Game();

			if(mockSettings)
			{
				game.StartDate = DateTime.UtcNow;
				game.DurationInMinutes = 60;
				game.EntryCode = "000000";
				game.PlayerCountPerTeam = 4;
				game.TeamCount = teams.Count;
			}

			if(mockCoordinator)
				game.Coordinator = ned;

			foreach(var t in teams)
			{
				var team = new Team();
				for(int i = 0; i < t.Length; i++)
				{
					var p = t[i];
					if(i == 0)
					{
						team.Name = $"House {p}";
						continue;
					}

					if(mockPlayers)
					{
						var player = new Player
						{
							Alias = p,
							Email = $"{p.ToLower()}@hbo.com",
							Avatar = $"{ConfigManager.Instance.StorageAssetsBaseUrl}/avatars/{p.ToLower()}.jpg",
						};
						team.Players.Add(player);
					}
				}

				game.Teams.Add(team);
			}

			if(mockTreasure)
			{
				var dog = new Treasure
				{
					Hint = "Not a cat",
					ImageSource = $"{ConfigManager.Instance.StorageAssetsBaseUrl}/treasures/dog.jpg",
					Points = 100,
				};

				dog.Attributes.Add(new Attribute("dog"));
				//dog.Attributes.Add(new Attribute("laying"));
				//dog.Attributes.Add(new Attribute("floor"));

				var shoes = new Treasure
				{
					Hint = "What has a soul, a tongue and eyes but isn't alive?",
					ImageSource = $"{ConfigManager.Instance.StorageAssetsBaseUrl}/treasures/shoes.jpg",
					Points = 100,
				};

				shoes.Attributes.Add(new Attribute("shoes"));
				//shoes.Attributes.Add(new Attribute("footwear"));
				//shoes.Attributes.Add(new Attribute("leather"));

				var bottle = new Treasure
				{
					Hint = "What has a neck, no head, yet still wears a cap?",
					ImageSource = $"{ConfigManager.Instance.StorageAssetsBaseUrl}/treasures/wine_bottle.jpg",
					Points = 50,
				};

				bottle.Attributes.Add(new Attribute("bottle"));
				bottle.Attributes.Add(new Attribute("beverage"));
				//bottle.Attributes.Add(new Attribute("table"));

				game.Treasures.Add(bottle);
				game.Treasures.Add(shoes);
				game.Treasures.Add(dog);

				if(mockAcquiredTreasure && mockPlayers)
				{
					var bottleAt = new AcquiredTreasure
					{
						TreasureId = bottle.Id,
						ImageSource = bottle.ImageSource,
						ClaimedTimeStamp = DateTime.Now.AddMinutes(20),
						ClaimedPoints = bottle.Points,
						PlayerId = game.Teams[1].Players[1].Id,
					};

					var dogAt = new AcquiredTreasure
					{
						TreasureId = dog.Id,
						ImageSource = dog.ImageSource,
						ClaimedTimeStamp = DateTime.Now.AddMinutes(20),
						ClaimedPoints = dog.Points,
						PlayerId = game.Teams[1].Players[0].Id,
					};

					//game.Teams[1].AcquiredTreasure.Add(bottleAt);
					game.Teams[1].AcquiredTreasure.Add(dogAt);
				}
			}

			return game;
		}
	}
}
