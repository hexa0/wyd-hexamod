using HarmonyLib;
using HexaMod.Scripts.Persistent;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(PetSpawner))]
	internal class NoPets
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool NoPetsPatch()
		{
			return !HexaPersistentLobby.instance.lobbySettings.disablePets;
		}
	}
}
