using HarmonyLib;
using HexaMod.API.Util.Patching;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature
{
	[ModdedPatch]
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class CharacterCulling
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref FirstPersonController __instance)
		{
			__instance.myCam.cullingMask ^= 1 << 12;
		}
	}
}
