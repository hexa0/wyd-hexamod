using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(MicrowaveButton))]
	internal class StartClickFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref MicrowaveButton __instance)
		{
			AudioSource clickSound = __instance.GetComponent<AudioSource>();
			clickSound.playOnAwake = false;
			clickSound.Stop();
		}
	}
}
