using HarmonyLib;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(RematchHelper))]
    internal class RematchHelperDisable
    {
        [HarmonyPatch("OnLevelWasLoaded")]
        [HarmonyPrefix]
        static bool OnLevelWasLoaded(ref RematchHelper __instance)
        {
            HexaMod.rematchHelper = __instance;

            return false;
        }
    }
}
