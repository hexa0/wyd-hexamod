using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(FirstPersonController))]
    internal class SmoothCharacterControllerPatch
    {
        public static class SmoothCharacterControllerPatchGlobal
        {
            public static bool isRunningFromPatch = false;
        }

        [HarmonyPatch("FixedUpdate")]
        [HarmonyPrefix]
        static bool CancelFixedUpdate()
        {
            if (!SmoothCharacterControllerPatchGlobal.isRunningFromPatch)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void RunFixedUpdateOnUpdate(ref FirstPersonController __instance)
        {
            PhotonView netView = __instance.GetComponent<PhotonView>();
            if (netView & netView.isMine)
            {
                var oldDelta = Time.fixedDeltaTime;
                SmoothCharacterControllerPatchGlobal.isRunningFromPatch = true;
                Time.fixedDeltaTime = Time.deltaTime;
                MethodInfo fixedUpdateMethod = AccessTools.Method(__instance.GetType(), "FixedUpdate");
                fixedUpdateMethod?.Invoke(__instance, null);
                SmoothCharacterControllerPatchGlobal.isRunningFromPatch = false;
                Time.fixedDeltaTime = oldDelta;
            }
        }
    }
}
