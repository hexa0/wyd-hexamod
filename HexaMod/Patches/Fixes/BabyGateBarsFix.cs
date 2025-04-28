using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(GateBars))]
    internal class BabyGateBarsFix
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void Update(ref GateBars __instance)
        {
            __instance.transform.localPosition = Vector3.zero;
            __instance.transform.localRotation = Quaternion.identity;
        }
    }
}
