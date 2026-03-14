using HarmonyLib;
using HexaMod.SDK.Levels.Scripts.Lighting;
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
				#if NOT_LINUX_NATIVE
					GameObject.Find("BackgroundCamera").GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
					__instance.cam.clearFlags = CurrentLevelSkybox.current.clearFlags;
				#endif

				#if LINUX_NATIVE
					GameObject.Find("BackgroundCamera").GetComponent<Camera>().clearFlags = CameraClearFlags.Depth;

					if (CurrentLevelSkybox.current.clearFlags == CameraClearFlags.Nothing) {
						__instance.cam.clearFlags = CameraClearFlags.Depth;
					}
					else
					{
						__instance.cam.clearFlags = CurrentLevelSkybox.current.clearFlags;
					}
				#endif
			}
		}
	}
}
	