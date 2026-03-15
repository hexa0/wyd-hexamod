using HarmonyLib;
using HexaMod.API.Util.Unity.Settings;
using HexaMod.Scripts.Persistent;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(OptionsController))]
	internal class FixGraphicsOptions
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

		public static void RefreshAllFX() {
			Mod.Debug("calling RefreshFX on all SpecFXHelpers");
			foreach (SpecFXHelper specFXHelper in Object.FindObjectsOfType<SpecFXHelper>())
			{
				Mod.Debug($"refresh {specFXHelper.name}");
				specFXHelper.RefreshFX();
			}
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

			QualitySettings.vSyncCount = WYDPreferences.vsync.Value ? 1 : 0;

			RefreshAllFX();

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
	internal class FixBrokenGraphicsToggles
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref SetToggle __instance)
		{
			void Finalize() {
				FixGraphicsOptions.RefreshAllFX();
			};

			if (__instance.showSS)
			{
				__instance.GetComponent<Toggle>().onValueChanged.AddListener(isOn =>
				{
					WYDPreferences.sunShafts.Value = isOn;
					Finalize();
				});
			}
			if (__instance.showFXAA)
			{
				__instance.GetComponent<Toggle>().onValueChanged.AddListener(isOn =>
				{
					WYDPreferences.antiAliasing.Value = isOn;
					// TODO: new options menu will allow this to be changed manually when using Forward rendering
					WYDPreferences.msaaLevel.Value = 4;
					QualitySettings.antiAliasing = WYDPreferences.msaaLevel.Value;
					Finalize();
				});
			}
			if (__instance.showAO)
			{
				__instance.GetComponent<Toggle>().onValueChanged.AddListener(isOn =>
				{
					WYDPreferences.ambientOcclusion.Value = isOn;
					Finalize();
				});
			}
			if (__instance.showDOF)
			{
				__instance.GetComponent<Toggle>().onValueChanged.AddListener(isOn =>
				{
					WYDPreferences.depthOfField.Value = isOn;
					Finalize();
				});
			}
		}
	}

	[HarmonyPatch(typeof(SpecFXHelper))]
	internal class ApplyGraphics
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
			__instance.aoComp.enabled = WYDPreferences.ambientOcclusion.Value && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Vulkan;
			__instance.aoComp.gameObject.GetComponent<ScreenSpaceAmbientObscurance>().enabled = __instance.aoComp.enabled;
			__instance.aoComp.gameObject.GetComponent<VignetteAndChromaticAberration>().enabled = __instance.aoComp.enabled;
			__instance.aoComp.Downsampling = 3;
			__instance.aoComp.Blur = SSAOPro.BlurMode.Gaussian;
			__instance.aoComp.BlurPasses = 3;
			__instance.aoComp.CutoffDistance = 100;
			__instance.aoComp.CutoffFalloff = 25;
			__instance.fxaaComp.mode = AAMode.DLAA; // best quality we can have so far until i can mod in TAA
			__instance.fxaaComp.enabled = WYDPreferences.antiAliasing.Value;
			__instance.ssComp.enabled = WYDPreferences.sunShafts.Value;
		}
	}
}
