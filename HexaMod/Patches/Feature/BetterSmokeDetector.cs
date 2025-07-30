using HarmonyLib;
using HexaMod.API.Util.Patching;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[OptionalPatch("useHexasSmokeDetectorSound", "Smoke Detector Uses Actual Smoke Detector Sound", "Tweaks", true)]
	[HarmonyPatch(typeof(SmokeDectector))]
	internal class BetterSmokeDetector
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref SmokeDectector __instance)
		{
			if (Assets.loadedLevel != null && Assets.loadedLevel == Assets.defaultLevel)
			{
				__instance.beepSound.GetComponent<AudioSource>().clip = HexaGlobal.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/SmokeDetector.wav");
			}
		}
	}
}
