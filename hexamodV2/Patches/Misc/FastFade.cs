using HarmonyLib;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(ImgFade))]
    internal class FastFade
    {
        [HarmonyPatch("Fade")]
        [HarmonyPrefix]
        static void FastFadePatch(ref ImgFade __instance)
        {
            // __instance.fadeDelay /= 4f;
            // __instance.fadeTime /= 4f;
        }
    }
}
