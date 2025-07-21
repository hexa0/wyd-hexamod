using HarmonyLib;
using HexaMod.Patches.Feature;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	internal class RigidBodyReplication : NetworkMovementRB
	{
		internal Rigidbody rigidBody;
		internal PickUp pickUpScript;
		internal PhotonView netView;

		internal Vector3 position = Vector3.zero;
		internal Quaternion rotation = Quaternion.identity;
		internal Vector3 velocity = Vector3.zero;
		internal Vector3 angularVelocity = Vector3.zero;

		internal float lastSynchronizationTime = Time.fixedTime;

		bool inRange = false;

		public bool IsMine {
			get
			{
				return netView.isMine || !PhotonNetwork.inRoom;
			}
		}

		new void Start() { }

		void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			pickUpScript = GetComponent<PickUp>();

			netView = GetComponent<PhotonView>();

			position = rigidBody.position;
			rotation = rigidBody.rotation;
			velocity = rigidBody.velocity;
			angularVelocity = rigidBody.angularVelocity;

			netView.ObservedComponents.Clear();
			netView.ObservedComponents.Add(this);
			netView.synchronization = ViewSynchronization.Unreliable;
		}

		void Read(PhotonStream stream)
		{
			position = (Vector3)stream.ReceiveNext();
			rotation = (Quaternion)stream.ReceiveNext();
			velocity = (Vector3)stream.ReceiveNext();
			angularVelocity = (Vector3)stream.ReceiveNext();
		}

		void Write(PhotonStream stream)
		{
			if (!inRange || (rigidBody.position.AlmostEquals(position, PhotonNetwork.precisionForVectorSynchronization) && rigidBody.rotation.AlmostEquals(rotation, PhotonNetwork.precisionForVectorSynchronization)))
			{
				return;
			}

			position = rigidBody.position;
			rotation = rigidBody.rotation;

			stream.SendNext(rigidBody.position);
			stream.SendNext(rigidBody.rotation);
			stream.SendNext(rigidBody.velocity);
			stream.SendNext(rigidBody.angularVelocity);
		}

		void ReplicationUpdate(bool hasNewData = true)
		{
			if (hasNewData)
			{
				lastSynchronizationTime = Time.fixedTime;
			}

			float allowedPositionVariance = Mathf.Max(velocity.magnitude * 0.25f, 0.1f);

			// prevents objects from getting stuck under shelfs or other flat surfaces
			// ideally in the future also check if the object has been misplaced by 0.2 units for more then 1 second as well to catch a few edge cases
			if (Time.timeScale <= 0f || rigidBody.isKinematic || rigidBody.IsSleeping() || (rigidBody.position - position).magnitude > allowedPositionVariance || ((Time.fixedTime - lastSynchronizationTime) > 0.5f))
			{
				if (!inRange)
				{
					return;
				}

				rigidBody.position = position;
				rigidBody.rotation = rotation;
				rigidBody.velocity = velocity;
				rigidBody.angularVelocity = angularVelocity;
			}
			else if (hasNewData)
			{
				if (pickUpScript == null || !pickUpScript.held)
				{
					rigidBody.position = Vector3.Lerp(rigidBody.position, position, 0.1f);
				}

				rigidBody.rotation = Quaternion.Lerp(rigidBody.rotation, rotation, 0.1f);
				rigidBody.velocity = velocity;
				rigidBody.angularVelocity = angularVelocity;
			}
		}

		new void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo _)
		{
			if (IsMine && stream.isWriting)
			{
				Write(stream);
			}
			else if (!IsMine && stream.isReading)
			{
				Read(stream);
				ReplicationUpdate();
			}
		}

		new void FixedUpdate()
		{
			if (rigidBody.isKinematic)
			{
				inRange = false;
			}
			else
			{
				inRange = false;

				foreach (Transform player in PlayerControllers.GetPlayerTransforms())
				{
					if ((player.position - transform.position).magnitude < 15f)
					{
						inRange = true;
						break;
					}
				};
			}

			if (!IsMine)
			{
				ReplicationUpdate(false);
			}
		}
	}

	[HarmonyPatch(typeof(NetworkMovementRB))]
	internal class BetterPhysicsReplicationPatch
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
			// nothing too special is required we can just swap it because we extend the base class and nothing stores
			// references to NetworkMovementRB objects that would require being updated
			__instance.gameObject.AddComponent<RigidBodyReplication>();
			Object.DestroyImmediate(__instance);
			return false;
		}

		[HarmonyPatch("OnPhotonSerializeView")]
		[HarmonyPrefix]
		static bool OnPhotonSerializeView()
		{
			return false;
		}
	}
}
