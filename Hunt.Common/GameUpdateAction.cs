namespace Hunt.Common
{
	public class GameUpdateAction
	{
		public const string Create = nameof(Create);
		public const string JoinTeam = nameof(JoinTeam);
		public const string LeaveTeam = nameof(LeaveTeam);
		public const string StartGame = nameof(StartGame);
		public const string EndGame = nameof(EndGame);
		public const string AcquireTreasure = nameof(AcquireTreasure);
		public const string AddTreasure = nameof(AddTreasure);
		public const string RemoveTreasure = nameof(RemoveTreasure);
		public const string UpdatePlayer = nameof(UpdatePlayer);
	}
}
