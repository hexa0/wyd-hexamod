using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(NetworkMovementRB))]
	internal class SharpPhysicsReplication
	{
		[HarmonyPatch("SyncedMovement")]
		[HarmonyPrefix]
		static bool SyncedMovement(ref NetworkMovementRB __instance)
		{
			return false;
		}

		[HarmonyPatch("OnPhotonSerializeView")]
		[HarmonyPostfix]
		static void OnPhotonSerializeView(ref NetworkMovementRB __instance)
		{
			var privateFields = Traverse.Create(__instance);

			var netView = privateFields.Field<PhotonView>("netView");
			var rigidBody = __instance.gameObject.GetComponent<Rigidbody>();
			var syncPosition = privateFields.Field<Vector3>("syncPosition");
			var syncRotation = privateFields.Field<Quaternion>("syncRotation");
			var syncVel = privateFields.Field<Vector3>("syncVel");
			var syncAngVel = privateFields.Field<Vector3>("syncAngVel");

			if (!netView.Value.isMine && __instance.updated)
			{
				rigidBody.position = Vector3.Lerp(rigidBody.position, syncPosition.Value, 0.1f);
				rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, syncRotation.Value, 0.1f);
				rigidBody.velocity = syncVel.Value;
				rigidBody.angularVelocity = syncAngVel.Value;
			}
			if (syncPosition.Value != null && syncPosition.Value != Vector3.zero && !netView.Value.isMine && (rigidBody.IsSleeping() || (rigidBody.position - syncPosition.Value).magnitude > 3f))
			{
				rigidBody.position = syncPosition.Value;
				rigidBody.rotation = syncRotation.Value;
				rigidBody.velocity = syncVel.Value;
				rigidBody.angularVelocity = syncAngVel.Value;
			}
		}

		[HarmonyPatch("FixedUpdate")]
		[HarmonyPostfix]
		static void FixedUpdate(ref NetworkMovementRB __instance)
		{
			var privateFields = Traverse.Create(__instance);

			var netView = privateFields.Field<PhotonView>("netView");
			var rigidBody = __instance.gameObject.GetComponent<Rigidbody>();
			var syncPosition = privateFields.Field<Vector3>("syncPosition");
			var syncRotation = privateFields.Field<Quaternion>("syncRotation");
			var syncVel = privateFields.Field<Vector3>("syncVel");
			var syncAngVel = privateFields.Field<Vector3>("syncAngVel");

			if (syncPosition.Value != null && syncPosition.Value != Vector3.zero && !netView.Value.isMine && (rigidBody.IsSleeping() || (rigidBody.position - syncPosition.Value).magnitude > 3f))
			{
				rigidBody.position = syncPosition.Value;
				rigidBody.rotation = syncRotation.Value;
				rigidBody.velocity = syncVel.Value;
				rigidBody.angularVelocity = syncAngVel.Value;
			}
		}
	}
}
