using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(PickUp))]
	internal class PickUpScrollWheel
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static void Start(ref PickUp __instance)
		{
			__instance.gameObject.AddComponent<PickUpRpcExtension>();
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static void PickUpScrollWheelPatch(ref PickUp __instance)
		{
			if (HexaPersistentLobby.instance.lobbySettings.modernGrabbing)
			{
				if (__instance.held && __instance.GetComponent<PhotonView>().isMine)
				{
					float grabbedDisCurrent = (float)Traverse.Create(__instance).Field("grabbedDis").GetValue();
					if (grabbedDisCurrent > 1f || Input.mouseScrollDelta.y > 0f)
					{
						float newDistance = grabbedDisCurrent + (Input.mouseScrollDelta.y / 2f);
						if (newDistance != grabbedDisCurrent)
						{
							__instance.GetComponent<PhotonView>().RPC("SetGrabbedDistance", PhotonTargets.Others, new object[] { newDistance });
							Traverse.Create(__instance).Field("grabbedDis").SetValue(newDistance);
						}
					}
				}
			}
		}

		[HarmonyPatch("FixedUpdate")]
		[HarmonyPrefix]
		static bool PickUpVelocityPatch(ref PickUp __instance)
		{
			if (HexaPersistentLobby.instance.lobbySettings.modernGrabbing)
			{
				if (__instance.held && __instance.target)
				{
					var rb = __instance.GetComponent<Rigidbody>();
					float grabbedDis = (float)Traverse.Create(__instance).Field("grabbedDis").GetValue();

					// 15f, 10f
					rb.velocity = Vector3.ClampMagnitude((__instance.target.position + __instance.target.forward * grabbedDis - rb.position) * 15f, 50f);

					if (__instance.GetComponent<PhotonView>().isMine && Vector3.Distance(__instance.transform.position, __instance.target.position) > 5f)
					{
						var holder = GameObject.Find(__instance.holding);

						if (holder)
						{
							holder.SendMessage("DropItem2");
						}
					}

					rb.interpolation = RigidbodyInterpolation.Interpolate;
				}

				return false;
			}

			return true;
		}

		[HarmonyPatch("PickUp")]
		[HarmonyPostfix]
		static void PickUp(ref PickUp __instance)
		{
			PhotonView netView = __instance.GetComponent<PhotonView>();
			netView.RPC("SetGrabbedDistance", PhotonTargets.Others, new object[] { (float)Traverse.Create(__instance).Field("grabbedDis").GetValue() });
		}
	}
}
