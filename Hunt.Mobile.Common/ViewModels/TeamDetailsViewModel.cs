using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using Hunt.Common;
using System.Linq;

namespace Hunt.Mobile.Common
{
	public class TeamDetailsViewModel : BaseViewModel
	{
		Team _team;
		public Team Team
		{
			get { return _team; }
			set { SetPropertyChanged(ref _team, value); }
		}

		public string PlayerCount
		{
			get
			{
				var s = Team.Players.Count == 1 ? string.Empty : "s";
				return string.Format($"{Team.Players.Count} player{s}");
			}
		}

		public string TeamName
		{
			get
			{
				if(Team == null)
					return string.Empty;
				
				if(Team.Name.Length >= 20)
					return Team.Name.Substring(0, 17) + "...";

				return Team.Name;
			}
		}
	}
}