using HarmonyLib;
using HexaMod.API.Util.Unity.Settings;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(LightHolder))]
	internal static class LightHolderNoSingleLightFix
	{
		[HarmonyPatch("ToggleLights")]
		[HarmonyPrefix]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "because it doesn't work dumbass, it throws errors")]
		static bool ToggleLights(ref LightHolder __instance, bool input)
		{
			for (int i = 0; i < __instance.lights.Length; i++)
			{
				if (__instance.lights[i] != null)
				{
					__instance.lights[i].SetActive(input);
				}
			}

			WYDPreferences.dynamicLightingEnabled.Set(input);

			return false;
		}
	}
}
