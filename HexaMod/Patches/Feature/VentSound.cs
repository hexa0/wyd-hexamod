using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(AirVent))]
	internal class VentSound
	{
		static AudioClip ventOrignAudioClip = HexaMod.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/VentUse01.wav");
		static AudioClip ventGoalAudioClip = HexaMod.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/VentUse02.wav");

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref AirVent __instance)
		{
			AudioSource ventOriginSoundSource = __instance.gameObject.AddComponent<AudioSource>();
			ventOriginSoundSource.spatialBlend = 1f;
			ventOriginSoundSource.spatialize = true;
			ventOriginSoundSource.dopplerLevel = 0f;
			ventOriginSoundSource.volume = 1f;
			ventOriginSoundSource.maxDistance *= 2f;
		}

		[HarmonyPatch("RPCInteract")]
		[HarmonyPostfix]
		static void RPCInteract(ref AirVent __instance, string inputName)
		{
			if (HexaMod.persistentLobby.lobbySettings.ventSounds && inputName.Substring(0, 3) == "Bab" && !__instance.broken && __instance.exitPos != null && __instance.correspondingVent != null)
			{
				__instance.gameObject.GetComponent<AudioSource>().PlayOneShot(ventOrignAudioClip);
				__instance.correspondingVent.gameObject.GetComponent<AudioSource>().PlayOneShot(ventGoalAudioClip);
			}
		}
	}
}
