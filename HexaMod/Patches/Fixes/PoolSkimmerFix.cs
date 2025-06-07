using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(LaserLight))]
	internal class PoolSkimmerFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Update(ref LaserLight __instance)
		{
			if (__instance.isSkimmer)
			{
				Object.Destroy(__instance.transform.Find("PoolSkimmerHead").GetComponent<Rigidbody>());
			}
		}
	}
}
