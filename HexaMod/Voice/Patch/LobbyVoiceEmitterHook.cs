using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Voice
{
    [HarmonyPatch]
    internal class LobbyVoiceEmitterHook
    {
        [HarmonyPatch(typeof(PlayerNames), "Start")]
        [HarmonyPostfix]
        static void Start(ref PlayerNames __instance)
        {
            __instance.gameObject.AddComponent<LobbyVoiceEmitterBehavior>();
        }

        [HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
        [HarmonyPostfix]
        static void RefreshNameList(ref PlayerNames __instance)
        {
            __instance.GetComponent<LobbyVoiceEmitterBehavior>().Refresh();
        }
    }
}
