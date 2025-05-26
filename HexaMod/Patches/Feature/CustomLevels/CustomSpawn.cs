using HarmonyLib;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature.CustomLevels
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class CustomSpawn
	{

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void HandleSpawns(ref FirstPersonController __instance)
		{
			Assets.HandleSpawnTeleport(__instance);
		}
	}
}
