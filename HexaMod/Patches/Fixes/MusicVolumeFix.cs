using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class MusicVolumeFix
	{

		static void UpdateVolume()
		{
			HexaMod.tabOutMute.UpdateFocusedState(HexaMod.tabOutMute.IsFocused());
		}

		[HarmonyPatch(typeof(SetOptions), "Start")]
		[HarmonyPrefix]
		static bool Start(ref SetOptions __instance)
		{
			QualitySettings.antiAliasing = PlayerPrefs.GetInt("AntiAliasing", 0);
			QualitySettings.vSyncCount = PlayerPrefs.GetInt("UseVSync", 1);
			HexaMod.networkManager.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("MusicVolume", 1f);

			return false; // MasterVolume is now handled by TabOutMuteBehavior
		}

		[HarmonyPatch(typeof(SetSlider), "Reset")]
		[HarmonyPrefix]
		static bool Reset(ref SetSlider __instance)
		{
			if (__instance.audSlide)
			{
				UpdateVolume();
				__instance.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
			}
			if (__instance.sensSlide)
			{
				__instance.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseSensitivity", (float)1);
			}
			if (__instance.barSlide)
			{
				__instance.GetComponent<Slider>().value = PlayerPrefs.GetFloat("SplitScreenBarSize", 0.75f);
			}
			if (__instance.musicSlide)
			{
				__instance.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
			}

			return false; // MasterVolume is now handled by TabOutMuteBehavior
		}

		[HarmonyPatch(typeof(OptionsController), "ControlVolume")]
		[HarmonyPrefix]
		static bool ControlVolume(ref float val)
		{
			PlayerPrefs.SetFloat("MasterVolume", val);
			UpdateVolume();
			return false;
		}
	}
}
