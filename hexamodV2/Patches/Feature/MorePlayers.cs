using HarmonyLib;

namespace HexaMod.Patches
{
    [HarmonyPatch()]
    internal class MorePlayers
    {
        [HarmonyPatch(typeof(PhotonNetworkManager), "Start")]
        [HarmonyPostfix]
        static void MorePlayersByDefaultPatch(ref PhotonNetworkManager __instance)
        {
            __instance.maxPlayer = HexaMod.defaultMaxPlayers;
            for (int i = 0; i < __instance.maxPlayerText.Length; i++)
            {
                __instance.maxPlayerText[i].text = HexaMod.defaultMaxPlayers.ToString();
            }
        }

        [HarmonyPatch(typeof(PhotonNetworkManager), "IncreasePlayerCount")]
        [HarmonyPrefix]
        static bool MorePlayersPatch(ref PhotonNetworkManager __instance)
        {
            __instance.maxPlayer++;

            for (int i = 0; i < __instance.maxPlayerText.Length; i++)
            {
                __instance.maxPlayerText[i].text = __instance.maxPlayer.ToString();
            }

            return false;
        }

        [HarmonyPatch(typeof(GameModeInfo), "SetText")]
        [HarmonyPrefix]
        static void SetText(ref string input)
        {
            input = input.Replace("8", "∞").Replace("7", "∞");
        }
    }
}
