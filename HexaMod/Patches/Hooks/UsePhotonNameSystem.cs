using HarmonyLib;

namespace HexaMod.Patches.Hooks
{
	[HarmonyPatch]
	internal class UsePhotonNameSystem
	{
		[HarmonyPatch(typeof(PhotonNetworkManager), "ChangeLobbyName")]
		[HarmonyPostfix]
		static void ChangeLobbyName(ref PhotonNetworkManager __instance)
		{
			PhotonNetwork.playerName = __instance.lobbyName;
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "Start")]
		[HarmonyPostfix]
		static void Start(ref PhotonNetworkManager __instance)
		{
			PhotonNetwork.playerName = __instance.lobbyName;
		}
	}
}