using HarmonyLib;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature
{
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
