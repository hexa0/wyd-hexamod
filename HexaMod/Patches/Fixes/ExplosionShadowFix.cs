using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch]
	internal class ExplosionShadowFix
	{
		// FYI: DO NOT EVER reference any of the explosion mono behaviors, that will crash the unity editor
		[HarmonyPatch(typeof(TriggerExpand), "Start")]
		[HarmonyPrefix]
		static void TriggerExpandStart(ref TriggerExpand __instance)
		{
			if (__instance.explosion)
			{
				foreach (MeshRenderer meshRenderer in __instance.explosion.GetComponentsInChildren<MeshRenderer>())
				{
					meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				}
			}
		}

		[HarmonyPatch(typeof(Ball), "Start")]
		[HarmonyPrefix]
		static void BallStart(ref Ball __instance)
		{
			foreach (MeshRenderer meshRenderer in __instance.explosion.GetComponentsInChildren<MeshRenderer>())
			{
				meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			}
		}
	}
}
