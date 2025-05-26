using HarmonyLib;

namespace HexaMod.Patches.Feature.CustomLevels
{
	[HarmonyPatch(typeof(PhotonNetworkManager))]
	internal class TransmitStateOnJoin
	{

		[HarmonyPatch("OnPhotonPlayerConnected")]
		[HarmonyPostfix]
		static void OnPhotonPlayerConnected()
		{
			HexaMod.hexaLobby.TryNetworkLobbySettings(HexaMod.persistentLobby.lobbySettings);
		}
	}
}
