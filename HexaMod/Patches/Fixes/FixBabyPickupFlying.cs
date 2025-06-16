using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class FixBabyPickupFlying
	{
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static void Update(ref FirstPersonController __instance)
		{
			if (!__instance.GetComponent<PhotonView>().isMine)
			{
				__instance.GetComponent<CharacterController>().enabled = !(__instance.restrained || __instance.restrainedHeld);
			}
		}
	}
}
