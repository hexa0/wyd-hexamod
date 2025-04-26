using HarmonyLib;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(SplitscreenHelper))]
    internal class ReplaceSplitscreen
    {
        [HarmonyPatch("UseSplitScreen")]
        [HarmonyPrefix]
        static bool DisableSplitscreenPatch()
        {
            return true;//false;
        }
    }
}
