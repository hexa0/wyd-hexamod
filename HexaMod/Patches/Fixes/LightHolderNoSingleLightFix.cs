using HarmonyLib;
using HexaMod.Settings;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(LightHolder))]
	internal static class LightHolderNoSingleLightFix
	{
		[HarmonyPatch("ToggleLights")]
		[HarmonyPrefix]
		static bool ToggleLights(ref LightHolder __instance, bool input)
		{
			for (int i = 0; i < __instance.lights.Length; i++)
			{
				__instance.lights[i]?.SetActive(input);
			}

			WYDPreferences.dynamicLightingEnabled.Set(input);

			return false;
		}
	}
}
