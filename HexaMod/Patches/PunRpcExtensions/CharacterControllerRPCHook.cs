using HarmonyLib;
using HexaMod.Scripts;
using HexaMod.Util;
using HexaMod.Voice;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.PunRpcExtensions
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class CharacterControllerRPCHook
	{

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void CharacterControllerExtensionsPatch(ref FirstPersonController __instance)
		{
			__instance.gameObject.AddComponent<CharacterExtendedRPCBehavior>();
			__instance.gameObject.AddComponent<CharacterModelSwapper>();
			__instance.gameObject.AddComponent<PlayerVoiceEmitterRPC>();

			NetworkedSoundBehavior networkedSound = __instance.gameObject.AddComponent<NetworkedSoundBehavior>();

			Traverse fields = Traverse.Create(__instance);
			networkedSound.RegisterSound(fields.Field<AudioClip>("m_JumpSound").Value);
			networkedSound.RegisterSound(fields.Field<AudioClip>("m_LandSound").Value);
			networkedSound.RegisterSounds(fields.Field<AudioClip[]>("m_FootstepSounds").Value);
		}
	}
}
