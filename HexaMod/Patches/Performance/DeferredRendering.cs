using HarmonyLib;
using HexaMod.API.Util.Patching;

namespace HexaMod.Patches.Performance
{
	[OptionalPatch("useDeferredRendering", "Use Deferred Rendering", "Rendering")]
	[HarmonyPatch(typeof(SpecFXHelper))]
	internal class DeferredRendering
	{
		[HarmonyPatch("RefreshFX")]
		[HarmonyPostfix]
		static void RefreshFX(ref SpecFXHelper __instance)
		{
			__instance.cam.renderingPath = UnityEngine.RenderingPath.DeferredShading;
		}
	}
}
