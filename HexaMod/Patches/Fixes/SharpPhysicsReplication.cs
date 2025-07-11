using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	internal static class SharpPhysicsReplicationHandler
	{
		internal static void Read(PhotonStream stream, Rigidbody rigidBody, PickUp pickUpScript)
		{
			Vector3 position = (Vector3)stream.ReceiveNext();
			Quaternion rotation = (Quaternion)stream.ReceiveNext();
			Vector3 velocity = (Vector3)stream.ReceiveNext();
			Vector3 angularVelocity = (Vector3)stream.ReceiveNext();

			// prevents objects from getting stuck under shelfs or other flat surfaces
			// ideally in the future also check if the object has been misplaced by 0.2 units for more then 1 second as well to catch a few edge cases
			if (rigidBody.IsSleeping() || (rigidBody.position - position).magnitude > 3f)
			{
				rigidBody.position = position;
				rigidBody.rotation = rotation;
				rigidBody.velocity = velocity;
				rigidBody.angularVelocity = angularVelocity;
			}
			else
			{
				if (!pickUpScript)
				{
					rigidBody.position = Vector3.Lerp(rigidBody.position, position, 0.1f);
				}
				rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, rotation, 0.1f);
				rigidBody.velocity = velocity;
				rigidBody.angularVelocity = angularVelocity;
			}

		}

		internal static void Write(PhotonStream stream, Rigidbody rigidBody)
		{
			stream.SendNext(rigidBody.position);
			stream.SendNext(rigidBody.rotation);
			stream.SendNext(rigidBody.velocity);
			stream.SendNext(rigidBody.angularVelocity);
		}
	}
	[HarmonyPatch(typeof(NetworkMovementRB))]
	internal class SharpPhysicsReplication
	{
		[HarmonyPatch("SyncedMovement")]
		[HarmonyPrefix]
		static bool SyncedMovement()
		{
			return false;
		}

		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool Start(ref NetworkMovementRB __instance)
		{
			Traverse fields = Traverse.Create(__instance);
			PhotonView netView = __instance.GetComponent<PhotonView>();
			fields.Field<PhotonView>("netView").Value = netView;
			fields.Field<Rigidbody>("rb").Value = __instance.GetComponent<Rigidbody>();
			netView.ObservedComponents.Add(__instance);
			netView.synchronization = ViewSynchronization.UnreliableOnChange;
			fields.Field<PickUp>("pickUpScript").Value = __instance.GetComponent<PickUp>();

			return false;
		}

		[HarmonyPatch("OnPhotonSerializeView")]
		[HarmonyPrefix]
		static bool OnPhotonSerializeView(ref NetworkMovementRB __instance, PhotonStream stream, PhotonMessageInfo info)
		{
			if (info.sender == PhotonNetwork.player && stream.isWriting)
			{
				SharpPhysicsReplicationHandler.Write(stream, __instance.GetComponent<Rigidbody>());
			}
			else if (info.sender != PhotonNetwork.player && stream.isReading)
			{
				Traverse fields = Traverse.Create(__instance);
				SharpPhysicsReplicationHandler.Read(stream, __instance.GetComponent<Rigidbody>(), fields.Field<PickUp>("pickUpScript").Value);
			}

			return false;
		}
	}
}
