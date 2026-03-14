using HarmonyLib;
using HexaMod.API.Util.Unity.Settings;
using HexaMod.Scripts.Persistent;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(OptionsController))]
	internal class FixOptions
	{
		internal static OptionsController controller;

		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool Start(ref OptionsController __instance)
		{
			controller = __instance;
			controller.Reset();

			return false;
		}

		[HarmonyPatch("Reset")]
		[HarmonyPrefix]
		static bool Reset()
		{
			if (controller.resText != null)
			{
				controller.resText.text = "Native (Forced)";
			}

			if (controller.musicSource)
			{
				controller.musicSource.volume = WYDPreferences.musicVolume.Value;
			}

			QualitySettings.vSyncCount = WYDPreferences.uesVsync.Value ? 1 : 0;

			Mod.Debug("calling RefreshFX on all SpecFXHelpers");
			foreach (SpecFXHelper specFXHelper in Object.FindObjectsOfType<SpecFXHelper>())
			{
				Mod.Debug($"refresh {specFXHelper.name}");
				specFXHelper.RefreshFX();
			}

			return false;
		}

		[HarmonyPatch("SetRes")]
		[HarmonyPrefix]
		static bool SetRes()
		{
			Mod.Debug("Blocked SetRes");
			return false; 
		}

		[HarmonyPatch("LowerResText")]
		[HarmonyPrefix]
		static bool LowerResText()
		{
			Mod.Debug("Blocked LowerResText");
			return false; 
		}

		[HarmonyPatch("RaiseResText")]
		[HarmonyPrefix]
		static bool RaiseResText()
		{
			Mod.Debug("Blocked LowerResText");
			return false; 
		}
	}

	[HarmonyPatch(typeof(SetToggle))]
	internal class FixBrokenToggles
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref SetToggle __instance)
		{
			if (__instance.showSS)
			{
				__instance.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
				{
					FixOptions.controller.ToggleSunShafts(isOn);
				});
			}
			if (__instance.showFXAA)
			{
				__instance.GetComponent<Toggle>().onValueChanged.AddListener((bool isOn) =>
				{
					QualitySettings.antiAliasing = isOn ? 4 : 0;
				});
			}
		}
	}

	[HarmonyPatch(typeof(SpecFXHelper))]
	internal class EnableMSAA
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref SpecFXHelper __instance)
		{
			__instance.cam.allowMSAA = true;
			__instance.cam.allowHDR = true;
		}

		[HarmonyPatch("RefreshFX")]
		[HarmonyPostfix]
		static void RefreshFX(ref SpecFXHelper __instance)
		{
			__instance.aoComp.enabled = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan;
			__instance.aoComp.gameObject.GetComponent<ScreenSpaceAmbientObscurance>().enabled = __instance.aoComp.enabled;
			__instance.aoComp.Downsampling = 3;
			__instance.aoComp.Blur = SSAOPro.BlurMode.Gaussian;
			__instance.aoComp.BlurPasses = 3;
			__instance.aoComp.CutoffDistance = 100;
			__instance.aoComp.CutoffFalloff = 25;
		}
	}
}
