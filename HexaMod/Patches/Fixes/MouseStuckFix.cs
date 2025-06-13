using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class MouseStuckFix
	{
		[HarmonyPatch(typeof(InGameMenuHelper), "Update")]
		[HarmonyPostfix]
		static void Update(ref InGameMenuHelper __instance)
		{
			if (__instance.menuOn && !__instance.deathCam.GetComponent<DeathCam>().spectateMode)
			{
				if (__instance.allowMenuControl)
				{
					Cursor.visible = true;
					Cursor.lockState = CursorLockMode.None;
				}
			}
		}
	}
}
