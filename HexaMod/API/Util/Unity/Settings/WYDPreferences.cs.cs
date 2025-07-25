namespace HexaMod.API.Util.Unity.Settings
{
	public static class WYDPreferences
	{
		public static readonly UnityPreference<bool> dynamicLightingEnabled = new UnityPreference<bool>("UseDL", true);
		public static readonly UnityPreference<float> masterVolume = new UnityPreference<float>("MasterVolume", 0.75f);
		public static readonly UnityPreference<float> musicVolume = new UnityPreference<float>("MusicVolume", 0.7f);
	}
}
