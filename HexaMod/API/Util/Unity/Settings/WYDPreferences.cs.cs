namespace HexaMod.API.Util.Unity.Settings
{
	public static class WYDPreferences
	{
		public static readonly UnityPreference<bool> dynamicLightingEnabled = new UnityPreference<bool>("UseDL", true);
		public static readonly UnityPreference<float> masterVolume = new UnityPreference<float>("MasterVolume", 0.75f);
		public static readonly UnityPreference<float> musicVolume = new UnityPreference<float>("MusicVolume", 0.7f);
		public static readonly UnityPreference<bool> vsync = new UnityPreference<bool>("UseVSync", true);
		public static readonly UnityPreference<int> msaaLevel = new UnityPreference<int>("AntiAliasing", 4);
		public static readonly UnityPreference<bool> ambientOcclusion = new UnityPreference<bool>("UseAmbientOcclusion", true);
		public static readonly UnityPreference<bool> depthOfField = new UnityPreference<bool>("UseDepthOfField", true);
		public static readonly UnityPreference<bool> antiAliasing = new UnityPreference<bool>("UseFXAA", true);
		public static readonly UnityPreference<bool> sunShafts = new UnityPreference<bool>("UseSunShafts", true);
	}
}
