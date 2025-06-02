using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class ForceRigidInterp
	{
		[HarmonyPatch(typeof(VelocityBreak), "Main")]
		[HarmonyPostfix]
		static void Main(ref VelocityBreak __instance)
		{
			foreach (var rigidbody in __instance.breakObj.GetComponentsInChildren<Rigidbody>())
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

				if (rigidbody.GetComponent<NetworkMovementRB>() == null)
				{
					rigidbody.gameObject.AddComponent<NetworkMovementRB>();
				}
			}
		}

		[HarmonyPatch(typeof(GlassTable), "Start")]
		[HarmonyPostfix]
		static void Start(ref GlassTable __instance)
		{
			foreach (var rigidbody in __instance.brokenTable.GetComponentsInChildren<Rigidbody>())
			{
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

				if (rigidbody.GetComponent<NetworkMovementRB>() == null)
				{
					rigidbody.gameObject.AddComponent<NetworkMovementRB>();
				}
			}
		}

		[HarmonyPatch(typeof(PianoKey), "Start")]
		[HarmonyPostfix]
		static void Start(ref PianoKey __instance)
		{
			__instance.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		}

		[HarmonyPatch(typeof(Blaster), "Start")]
		[HarmonyPostfix]
		static void Start(ref Blaster __instance)
		{
			__instance.bullet.gameObject.GetComponentInChildren<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		}

		[HarmonyPatch(typeof(ToiletLid), "Start")]
		[HarmonyPostfix]
		static void Start(ref ToiletLid __instance)
		{
			if (__instance.name == "Grill Lid")
			{
				// grills accidentally have both
				// this causes them to stutter and also (even worse) go backwards with interpolation on
				Object.Destroy(__instance);
			}
		}

		[HarmonyPatch(typeof(Radio), "Start")]
		[HarmonyPostfix]
		static void Start(ref Radio __instance)
		{
			__instance.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		}

		[HarmonyPatch(typeof(Ball), "Start")]
		[HarmonyPostfix]
		static void Start(ref Ball __instance)
		{
			__instance.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
		}
	}
}
