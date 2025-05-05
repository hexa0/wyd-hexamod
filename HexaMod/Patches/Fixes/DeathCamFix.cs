using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(DeathCam))]
	internal class DeathCamFix
	{
		private static bool lastSpectateMode = false;
		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void Update(ref DeathCam __instance)
		{
			if (!__instance.spectateMode)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				if (lastSpectateMode != __instance.spectateMode)
				{
					Vector3 oldWorldPosition = __instance.transform.position;
					__instance.transform.SetParent(null, true);
					__instance.transform.rotation = Quaternion.identity;
					__instance.transform.position = oldWorldPosition;
				}
			}

			lastSpectateMode = __instance.spectateMode;
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
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
}
