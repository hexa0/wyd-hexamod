using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature.CustomLevels
{
	[HarmonyPatch(typeof(LightHolder))]
	internal class SetCustomSunLightHook
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static void Start(ref LightHolder __instance)
		{
			__instance.sunLight = RenderSettings.sun;
		}
	}
}