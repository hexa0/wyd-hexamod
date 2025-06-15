using HarmonyLib;
using HexaMapAssemblies;
using UnityEngine;

namespace HexaMod.Patches.Feature.CustomLevels
{
	[HarmonyPatch(typeof(SpecFXHelper))]
	internal class SkyboxClearHook
	{
		[HarmonyPatch("RefreshFX")]
		[HarmonyPostfix]
		static void RefreshFX(ref SpecFXHelper __instance)
		{
			if (CurrentLevelSkybox.current != null)
			{
				GameObject.Find("BackgroundCamera").GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
				__instance.cam.clearFlags = CurrentLevelSkybox.current.clearFlags;
			}
		}
	}
}
	