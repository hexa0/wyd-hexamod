using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(VelocityBreak))]
	internal class ForceRigidInterpGlassCups
	{
		[HarmonyPatch("RPCDestroy")]
		[HarmonyPostfix]
		static void MakeRigidbodyInterpolated()
		{
			HexaMod.FixRigidBodies();
		}
	}

	[HarmonyPatch(typeof(GlassTable))]
	internal class ForceRigidInterpGlassTable
	{
		[HarmonyPatch("RPCUseInteract")]
		[HarmonyPostfix]
		static void MakeRigidbodyInterpolated()
		{
			HexaMod.FixRigidBodies();
		}
	}

	[HarmonyPatch(typeof(PianoKey))]
	internal class PianoKeyFix
	{
		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void Update(ref PianoKey __instance)
		{
			__instance.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
		}
	}

	[HarmonyPatch(typeof(BlasterFix))]
	internal class BlasterFix
	{
		[HarmonyPatch("ShootRPC")]
		[HarmonyPostfix]
		static void ShootRPC()
		{
			HexaMod.FixRigidBodies();
		}

		[HarmonyPatch("Shoot")]
		[HarmonyPostfix]
		static void Shoot()
		{
			HexaMod.FixRigidBodies();
		}

		[HarmonyPatch("Fire")]
		[HarmonyPostfix]
		static void Fire()
		{
			HexaMod.FixRigidBodies();
		}
	}

	[HarmonyPatch(typeof(ToiletLid))]
	internal class GrillFix
	{
		[HarmonyPatch("Start")]
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
	}

	[HarmonyPatch(typeof(Ball))]
	internal class BallFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref ToiletLid __instance)
		{
			HexaMod.FixRigidBodies();
		}
	}
}
