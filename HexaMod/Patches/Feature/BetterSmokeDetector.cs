using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(SmokeDectector))]
	internal class BetterSmokeDetector
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref SmokeDectector __instance)
		{
			if (Assets.loadedLevel != null && Assets.loadedLevel == Assets.titleLevel)
			{
				__instance.beepSound.GetComponent<AudioSource>().clip = HexaMod.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/SmokeDetector.wav");
			}
		}
	}
}
