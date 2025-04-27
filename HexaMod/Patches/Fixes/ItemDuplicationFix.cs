using HarmonyLib;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(ItemSpawner))]
    internal class ItemDuplicationFix
    {
        [HarmonyPatch("NetworkSpawnObjects")]
        [HarmonyPrefix]
        static bool NetworkSpawnObjectsPatch()
        {
            return PhotonNetwork.isMasterClient;
        }
    }
}
