using System;
using Hunt.Common;

namespace Hunt.Mobile.Common
{
	public class BaseAddTreasureViewModel : BaseGameViewModel
	{
		public Action<Game> OnTreasureAdded { get; set; }

		string _hint;
		public string Hint
		{
			get { return _hint; }
			set { SetPropertyChanged(ref _hint, value); }
		}
	}
}