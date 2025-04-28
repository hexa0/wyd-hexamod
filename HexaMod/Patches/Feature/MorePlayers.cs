using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches
{
    [HarmonyPatch]
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

        [HarmonyPatch(typeof(PlayerNames), "Start")]
        [HarmonyPostfix]
        static void Start(ref PlayerNames __instance)
        {
            if (__instance.daddyNames.Length > 0)
            {
                float yLast =
                    __instance.daddyNames[__instance.daddyNames.Length - 1].transform.localPosition.y;
                float yBeforeLast =
                    __instance.daddyNames[__instance.daddyNames.Length - 2].transform.localPosition.y;

                float yDifference = yLast - yBeforeLast;

                Text[] newNames = new Text[__instance.daddyNames.Length * 50];

                for (int i = 0; i < newNames.Length; i++)
                {
                    if (i < __instance.daddyNames.Length)
                    {
                        newNames[i] = __instance.daddyNames[i];
                    }
                    else
                    {
                        newNames[i] = Object.Instantiate(__instance.daddyNames[__instance.daddyNames.Length - 1], __instance.daddyNames[0].transform.parent);
                        newNames[i].transform.localPosition = new Vector2(newNames[i].transform.localPosition.x, yLast + yDifference);
                        yLast = newNames[i].transform.localPosition.y;
                    }
                }

                __instance.daddyNames = newNames;
            }

            if (__instance.babyNames.Length > 0)
            {
                float yLast =
                    __instance.babyNames[__instance.babyNames.Length - 1].transform.localPosition.y;
                float yBeforeLast =
                    __instance.babyNames[__instance.babyNames.Length - 2].transform.localPosition.y;

                float yDifference = yLast - yBeforeLast;

                Text[] newNames = new Text[__instance.babyNames.Length * 50];

                for (int i = 0; i < newNames.Length; i++)
                {
                    if (i < __instance.babyNames.Length)
                    {
                        newNames[i] = __instance.babyNames[i];
                    }
                    else
                    {
                        newNames[i] = Object.Instantiate(__instance.babyNames[__instance.babyNames.Length - 1], __instance.babyNames[0].transform.parent);
                        newNames[i].transform.localPosition = new Vector2(newNames[i].transform.localPosition.x, yLast + yDifference);
                        yLast = newNames[i].transform.localPosition.y;
                    }
                }

                __instance.babyNames = newNames;
            }
        }
    }
}
