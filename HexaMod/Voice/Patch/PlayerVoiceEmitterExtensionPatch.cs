using HarmonyLib;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Voice
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class PlayerVoiceEmitterExtensionPatch
	{

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void CharacterControllerExtensionsPatch(ref FirstPersonController __instance)
		{
			__instance.gameObject.AddComponent<PlayerVoiceEmitterRPC>();
		}
	}
}
