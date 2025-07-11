using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class InstantRandomCam
	{
		// fix for the split second of a black screen when the game starts
		[HarmonyPatch(typeof(RandomCam), "Start")]
		[HarmonyPrefix]
		static bool Start(ref RandomCam __instance)
		{
			__instance.GetComponent<Camera>().enabled = true;
			__instance.NewPos();
			return false;
		}
	}
}
