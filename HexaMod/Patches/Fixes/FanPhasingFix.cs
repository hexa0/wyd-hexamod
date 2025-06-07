using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(Fan))]
	internal class FanPhasingFix
	{
		static System.Random random = new System.Random();
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref Fan __instance)
		{
			AudioSource fanSound = __instance.transform.parent.GetComponent<AudioSource>();
			float randomFloat = (float)random.NextDouble();
			fanSound.time = randomFloat * fanSound.clip.length;
		}
	}
}
