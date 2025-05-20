using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch]
	internal class ItemTargetingNameSizeFix
	{
		[HarmonyPatch(typeof(ItemTargeting), "Update")]
		[HarmonyPrefix]
		static void Update(ref ItemTargeting __instance)
		{
			// this is dumb but doing an IL patch for this would be a pain in the ass so how about no
			if (__instance.camComp.enabled)
			{
				RaycastHit hit;
				Physics.Raycast(__instance.cam.position, __instance.cam.TransformDirection(Vector3.forward), out hit, __instance.rayDis, 68160769);
				if (hit.collider != null)
				{
					GameObject gameObject = hit.transform.gameObject;
					if (gameObject.name.Length <= 3)
					{
						gameObject.name = gameObject.name.PadRight(4, ' ');
					}
				}
			}
		}
	}
}
