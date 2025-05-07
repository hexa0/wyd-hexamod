using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class CharacterAudioPanningFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void FixBabyStereoAudioIssue(ref FirstPersonController __instance)
		{
			PhotonNetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<PhotonNetworkManager>();

			AudioSource audioEmitter = __instance.GetComponent<AudioSource>();

			if (__instance.gameObject.name == networkManager.playerObj.name && __instance.GetComponent<PhotonView>().isMine)
			{
				audioEmitter.bypassEffects = true;
				// audioEmitter.panStereo = -0.4f;
				audioEmitter.spatialBlend = 0;
				audioEmitter.volume = 0.35f;
			}
			else
			{
				audioEmitter.spatialBlend = 1f;
				audioEmitter.spatialize = true;
			}
		}
	}
}
