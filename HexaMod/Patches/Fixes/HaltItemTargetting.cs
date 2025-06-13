using HarmonyLib;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class HaltItemTargetting
	{
		[HarmonyPatch(typeof(DadItemTargeting), "Update")]
		[HarmonyPrefix]
		static bool Update(ref DadItemTargeting __instance)
		{
			return !__instance.fpsController.haltInput;
		}

		[HarmonyPatch(typeof(ItemTargeting), "Update")]
		[HarmonyPrefix]
		static bool Update(ref ItemTargeting __instance)
		{
			return !__instance.fpsController.haltInput;
		}
	}
}
