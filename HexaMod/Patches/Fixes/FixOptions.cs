using HarmonyLib;
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
		[HarmonyPostfix]
		static void Start(ref OptionsController __instance)
		{
			controller = __instance;
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
			__instance.aoComp.gameObject.GetComponent<ScreenSpaceAmbientObscurance>().enabled = __instance.aoComp.enabled;
			__instance.aoComp.Downsampling = 3;
			__instance.aoComp.Blur = SSAOPro.BlurMode.Gaussian;
			__instance.aoComp.BlurPasses = 3;
			__instance.aoComp.CutoffDistance = 100;
			__instance.aoComp.CutoffFalloff = 25;
		}
	}
}
