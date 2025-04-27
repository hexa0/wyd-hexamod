using HarmonyLib;
using UnityEngine;

namespace HexaMod
{
    public class TabOutMuteBehavior : MonoBehaviour
    {
        internal static class VolumeState
        {
            public static bool lastTabbedOut = true;
        }

        public void Start()
        {
            if (VolumeState.lastTabbedOut)
            {
                AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            }
            else
            {
                AudioListener.volume = 0f;
            }
        }

        public void Update()
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
