using HarmonyLib;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(OptionsController))]
	internal class DisplayCountFix
	{
		// originally the game would incorrectly display this zero indexed
		// this confused the hell outta me so this fixes that, also user facing settings should be zero indexed????

		[HarmonyPatch("ChangeDisplayUp")]
		[HarmonyPostfix]
		static void ChangeDisplayUp(ref OptionsController __instance)
		{
			__instance.dispText.text = (__instance.curDisplay + 1).ToString();
		}

		[HarmonyPatch("ChangeDisplayDown")]
		[HarmonyPostfix]
		static void ChangeDisplayDown(ref OptionsController __instance)
		{
			__instance.dispText.text = (__instance.curDisplay + 1).ToString();
		}

		[HarmonyPatch("Reset")]
		[HarmonyPostfix]
		static void Reset(ref OptionsController __instance)
		{
			__instance.dispText.text = (__instance.curDisplay + 1).ToString();
		}
	}
}
