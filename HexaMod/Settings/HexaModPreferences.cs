namespace HexaMod.Settings
{
	public static class HexaModPreferences
	{
		public static readonly ModPreference<bool> tabOutMute = new ModPreference<bool>("TabOutMute", true).LinkTo(TabOutMute.SetEnabled);
		public static readonly ModPreference<bool> doUItheme = new ModPreference<bool>("DoUITheme", true);
	}
}
