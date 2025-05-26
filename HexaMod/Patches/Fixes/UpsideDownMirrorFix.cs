using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(MirrorCam))]
	internal class UpsideDownMirrorFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		public static void Start(ref MirrorCam __instance)
		{
			__instance.transform.rotation *= Quaternion.Euler(0, 0, 180);
		}
	}
}
