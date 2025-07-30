using HarmonyLib;
using HexaMod.API.Util.Patching;
using HexaMod.Scripts.Persistent;

namespace HexaMod.Patches.Feature
{
	[ModdedPatch]
	[HarmonyPatch(typeof(PetSpawner))]
	internal class PetsToggle
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool NoPetsPatch()
		{
			return !HexaPersistentLobby.instance.lobbySettings.disablePets;
		}
	}
}
