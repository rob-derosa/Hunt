using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Hunt.Mobile.Common
{
	public static class Settings
	{
		static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}

		public static string Player
		{
			get { return AppSettings.GetValueOrDefault(nameof(Player), null); }
			set
			{
				if(value == null)
				{
					if(AppSettings.Contains(nameof(Player)))
						AppSettings.Remove(nameof(Player));
				}
				else
				{
					AppSettings.AddOrUpdateValue(nameof(Player), value);
				}
			}
		}

		public static string PlayerAlias
		{
			get { return AppSettings.GetValueOrDefault(nameof(PlayerAlias), null); }
			set { AppSettings.AddOrUpdateValue(nameof(PlayerAlias), value); }
		}

	}
}