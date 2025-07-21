using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch]
	static class ItemSpawnerParent
	{
		public static Transform parent;

		static void ParentCheck(Transform transform)
		{
			if (transform.parent == null)
			{
				transform.parent = parent;
			}
		}

		[HarmonyPatch(typeof(Fork), "Start")]
		[HarmonyPostfix]
		static void Start(ref Fork __instance) => ParentCheck(__instance.transform);

		[HarmonyPatch(typeof(Fork), "Update")]
		[HarmonyPostfix]
		static void Update(ref Fork __instance) => ParentCheck(__instance.transform);

		[HarmonyPatch(typeof(LeftHand), "Start")]
		[HarmonyPostfix]
		static void Start(ref LeftHand __instance) => ParentCheck(__instance.transform);

		[HarmonyPatch(typeof(LeftHand), "LateUpdate")]
		[HarmonyPostfix]
		static void LateUpdate(ref LeftHand __instance) => ParentCheck(__instance.transform);
	}
}
