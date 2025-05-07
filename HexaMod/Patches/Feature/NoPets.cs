using HarmonyLib;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(PetSpawner))]
	internal class NoPets
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool NoPetsPatch()
		{
			return !HexaMod.persistentLobby.lobbySettings.disablePets;
		}
	}
}
