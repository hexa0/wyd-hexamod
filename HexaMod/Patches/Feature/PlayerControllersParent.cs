using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal static class PlayerControllersParent
	{
		public static Transform parent;

		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static void Start(ref FirstPersonController __instance)
		{
			if (__instance.transform.parent == null)
			{
				__instance.transform.SetParent(parent, true);
			}
			else
			{
				// warn that this player isn't in the root of the hierarchy with the full path
				Mod.Warn("huh ", __instance.transform.parent.name, " ", __instance.transform.name);
			}
		}
	}
}
