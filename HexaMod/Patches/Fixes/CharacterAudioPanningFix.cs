using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class CharacterAudioPanningFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void FixCharacterAudioPanning(ref FirstPersonController __instance)
		{
			AudioSource audioEmitter = __instance.GetComponent<AudioSource>();

			if (__instance.gameObject.name == HexaMod.networkManager.playerObj.name && __instance.GetComponent<PhotonView>().isMine)
			{
				audioEmitter.bypassEffects = true;
				// audioEmitter.panStereo = -0.4f;
				audioEmitter.spatialBlend = 0f;
				audioEmitter.volume = 0.35f;
			}
			else
			{
				audioEmitter.panStereo = 0f;
				audioEmitter.bypassEffects = true;
				audioEmitter.spatialBlend = 1f;
				audioEmitter.spatialize = true;
			}
		}
	}
}
