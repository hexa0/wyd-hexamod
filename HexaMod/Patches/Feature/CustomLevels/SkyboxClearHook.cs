using HarmonyLib;
using HexaMapAssemblies;
using UnityEngine;

namespace HexaMod.Patches
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

	[HarmonyPatch(typeof(BabyStats))]
	internal class BabyDeadCameraFix
	{
		[HarmonyPatch("Dead")]
		[HarmonyPostfix]
		static void Dead(ref BabyStats __instance)
		{
			__instance.mainCam.GetComponent<Camera>().enabled = false;
		}
	}
}
	