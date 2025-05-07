using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(OptionsController))]
	internal class ForceNoSteamName
	{
		[HarmonyPatch("Reset")]
		[HarmonyPostfix]
		static void DefaultToggle(ref OptionsController __instance)
		{
			__instance.ToggleSteamName(false);
		}

		[HarmonyPatch("ToggleSteamName")]
		[HarmonyPrefix]
		static void DisableToggle(ref bool isOn)
		{
			isOn = false;
		}
	}

	[HarmonyPatch(typeof(SetToggle))]
	internal class ForceNoSteamNameHideOption
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static void DefaultToggle(ref SetToggle __instance)
		{
			__instance.showUseSteamName = false;

			var useSteamNameUIElement = GameObject.Find("UseSteamName");
			if (useSteamNameUIElement)
			{
				useSteamNameUIElement.SetActive(false);
				PlayerPrefs.SetInt("UseSteamName", 0);
			}
		}
	}
}
