using HarmonyLib;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
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
