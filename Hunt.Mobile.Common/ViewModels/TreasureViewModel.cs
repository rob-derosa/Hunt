using System;
using Hunt.Common;

namespace Hunt.Mobile.Common
{
	public class TreasureViewModel
	{
		public Game Game { get; set; }
		
		public TreasureViewModel(Game game, Treasure treasure)
		{
			Game = game;
			Treasure = treasure;
			AcquiredTreasure = Game.GetAcquiredTreasure(treasure);
		}

		public string Hint
		{
			get
			{
				return Game.HasStarted || Game.IsCoordinator() ? Treasure.Hint : "Hints are hidden until game starts";
			}
		}

		public Treasure Treasure { get; set; }

		public AcquiredTreasure AcquiredTreasure { get;set; }

		public bool IsAcquired
		{
			get { return AcquiredTreasure != null; }
		}

		public string AcquiredIconPath
		{
			get { return IsAcquired ? "complete_item.svg" : "incomplete_item.svg"; }
		}
	}
}
