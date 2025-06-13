using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(Oven))]
	internal class GrillKnobFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref Oven __instance)
		{
			if (__instance.gameObject.name == "Grill Knob")
			{
				__instance.gameObject.transform.localScale = new Vector3(30.19164f, 30.19164f, 30.19164f);
				__instance.gameObject.transform.localPosition = new Vector3(-0.457f, 0.015f, -0.215f);
				BoxCollider collider = __instance.GetComponent<BoxCollider>();
				collider.size = new Vector3(0.005f, 0.005f, 0.002f);
				collider.center = new Vector3(0f, 0f, 0.001f);
				__instance.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
				__instance.transform.parent.Find("Frying Pan").GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
			}
		}
	}
}
