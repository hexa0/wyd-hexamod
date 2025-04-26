using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(SetOptions))]
    internal class MusicVolumeFix
    {
        internal static class VolumeState
        {
            public static bool lastTabbedOut = false;
        }
/*        [HarmonyPatch("Reset")]
        [HarmonyPrefix]
        static void MusicVolumeFixPatch(ref OptionsController __instance)
        {
            HexaModBase.Instance.mls.LogInfo($"MUSIC SOURCE IS NULL?: {__instance.musicSource == null}");
        }*/

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void StartPatch(ref SetOptions __instance)
        {
            GameObject networkManager = GameObject.Find("NetworkManager");

            networkManager.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume", 1f);

            if (VolumeState.lastTabbedOut)
            {
                AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            }
            else
            {
                AudioListener.volume = 0f;
            }
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(ref SetOptions __instance)
        {
            if (Application.isFocused != VolumeState.lastTabbedOut)
            {
                VolumeState.lastTabbedOut = Application.isFocused;

                if (VolumeState.lastTabbedOut)
                {
                    AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                }
                else
                {
                    AudioListener.volume = 0f;
                }
            }
        }
    }
}
