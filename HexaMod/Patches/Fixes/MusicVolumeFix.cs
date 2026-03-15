using HarmonyLib;
using HexaMod.API.Util.Unity.Settings;
using HexaMod.Scripts.Persistent;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class MusicVolumeFix
	{

		static void UpdateVolume()
		{
			TabOutMute.instance.UpdateFocusedState(TabOutMute.instance.IsFocused());
		}

		[HarmonyPatch(typeof(SetOptions), "Start")]
		[HarmonyPrefix]
		static bool Start()
	{
			QualitySettings.antiAliasing = WYDPreferences.msaaLevel.Value;
			QualitySettings.vSyncCount = WYDPreferences.vsync.Value ? 1 : 0;
			return false; // MasterVolume is now handled by TabOutMuteBehavior
		}

		[HarmonyPatch(typeof(SetSlider), "Reset")]
		[HarmonyPrefix]
		static bool Reset(ref SetSlider __instance)
		{
			if (__instance.audSlide)
			{
				UpdateVolume();
				__instance.GetComponent<Slider>().value = WYDPreferences.masterVolume.Value;
			}
			if (__instance.musicSlide)
			{
				UpdateVolume();
				__instance.GetComponent<Slider>().value = WYDPreferences.musicVolume.Value;
			}
			if (__instance.sensSlide)
			{
				__instance.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
			}
			if (__instance.barSlide)
			{
				__instance.GetComponent<Slider>().value = PlayerPrefs.GetFloat("SplitScreenBarSize", 0.75f);
			}

			return false; // MasterVolume is now handled by TabOutMuteBehavior
		}

		[HarmonyPatch(typeof(OptionsController), "ControlVolume")]
		[HarmonyPrefix]
		static bool ControlVolume(ref float val)
		{
			WYDPreferences.masterVolume.Value = val;
			UpdateVolume();
			return false;
		}

		[HarmonyPatch(typeof(OptionsController), "ControlMusicVolume")]
		[HarmonyPrefix]
		static bool ControlMusicVolume(ref float val)
		{
			WYDPreferences.musicVolume.Value = val;
			UpdateVolume();
			return false;
		}
	}
}
