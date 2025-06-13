using HarmonyLib;

namespace HexaMod.Patches.Feature.CustomLevels
{
	[HarmonyPatch(typeof(SpecFXHelper))]
	internal class DistanceCulling
	{
		[HarmonyPatch("RefreshFX")]
		[HarmonyPostfix]
		static void RefreshFX(ref SpecFXHelper __instance)
		{
			float[] cullDistances = __instance.cam.layerCullDistances;

			for (int i = 0; i < cullDistances.Length; i++)
			{
				switch (i)
				{
					case 0: // default
						cullDistances[i] = 0f; // 0f means infinite
						break;
					case 8: // doors
						cullDistances[i] = 75f;
						break;
					default: // other
						cullDistances[i] = 50f;
						break;
				}
			}

			__instance.cam.layerCullDistances = cullDistances;
			__instance.cam.useOcclusionCulling = true;
			// the default near clip plane is pretty acceptable actually so we keep it
			__instance.cam.farClipPlane = 245f;
			__instance.cam.renderingPath = UnityEngine.RenderingPath.DeferredShading;
		}
	}
}
