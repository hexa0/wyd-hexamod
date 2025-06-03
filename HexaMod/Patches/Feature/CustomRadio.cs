using HarmonyLib;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(Radio))]
	internal class CustomRadio
	{
		static AudioClip[] clips;
		static int index = 0;

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref Radio __instance)
		{
			clips = new AudioClip[Assets.radioTracks.Count + 1];
			int i = 0;
			foreach (var item in Assets.radioTracks)
			{
				clips[i] = Assets.radioTracks[i].radioTrack;
				i++;
			}

			clips[i] = __instance.aud.clip;

			index = 0;
			new System.Random(HexaMod.persistentLobby.lobbySettings.roundNumber).Shuffle(clips);
		}

		[HarmonyPatch("RPCInteract")]
		[HarmonyPrefix]
		static void RPCInteract(ref Radio __instance)
		{
			if (__instance.state)
			{
				Mod.Print($"Play Radio Track {index} with name {clips[index].name}");
				__instance.aud.clip = clips[index];
				__instance.aud.Play();

				index++;
				index %= clips.Length;
			}
		}
	}
}
