using HarmonyLib;
using HexaMod.API.Util.Patching;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[ModdedPatch]
	[HarmonyPatch(typeof(RematchHelper))]
	internal class RematchHelperDisable
	{
		 static GameObject last;

		[HarmonyPatch("OnLevelWasLoaded")]
		[HarmonyPrefix]
		static bool OnLevelWasLoaded(ref RematchHelper __instance)
		{
			HexaGlobal.rematchHelper = __instance;

			return false;
		}

		[HarmonyPatch("Awake")]
		[HarmonyPrefix]
		static void Awake(ref RematchHelper __instance)
		{
			if (last != null)
			{
				Object.DestroyImmediate(last);
				last = null;
			}

			HexaGlobal.rematchHelper = __instance;
			last = __instance.gameObject;
		}
	}
}
