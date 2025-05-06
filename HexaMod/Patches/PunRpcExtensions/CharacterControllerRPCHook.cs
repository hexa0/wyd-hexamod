using HarmonyLib;
using HexaMod.Scripts;
using HexaMod.Util;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
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
			NetworkedSoundBehavior networkedSound = __instance.gameObject.AddComponent<NetworkedSoundBehavior>();

			Traverse fields = Traverse.Create(__instance);
			networkedSound.AddSound(fields.Field<AudioClip>("m_JumpSound").Value);
			networkedSound.AddSound(fields.Field<AudioClip>("m_LandSound").Value);
			networkedSound.AddSounds(fields.Field<AudioClip[]>("m_FootstepSounds").Value);
		}
    }
}
