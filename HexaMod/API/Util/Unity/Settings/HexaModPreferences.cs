using HexaMod.Scripts.Persistent;

namespace HexaMod.API.Util.Unity.Settings
{
	public static class HexaModPreferences
	{
		public static readonly ModPreference<bool> tabOutMute = new ModPreference<bool>("TabOutMute", true).LinkTo(TabOutMute.SetEnabled);
		public static readonly ModPreference<bool> doUItheme = new ModPreference<bool>("DoUITheme", true);
		public static readonly ModPreference<bool> smoothCrouching = new ModPreference<bool>("SmoothCrouching", true);
		public static readonly ModPreference<bool> viewBobbing = new ModPreference<bool>("ViewBobbing", true);
	}
}
