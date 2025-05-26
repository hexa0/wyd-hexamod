using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(RandomCam))]
	internal class RandomCamRigidBodyFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static void RemoveAccidentalRigidBody(ref RandomCam __instance)
		{
			Object.Destroy(__instance.GetComponent<Rigidbody>());
		}
	}
}
