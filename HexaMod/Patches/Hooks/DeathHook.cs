using HarmonyLib;
using HexaMapAssemblies;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Hooks
{
	internal class DeathRPC : MonoBehaviour
	{
		bool isDead = false;
		[PunRPC]
		public void DeadRPC()
		{
			if (isDead) return;
			isDead = true;

			gameObject.name = "Dead Baby";
			Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
			rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
			rigidBody.isKinematic = false;
			rigidBody.useGravity = true;
			DestroyImmediate(gameObject.GetComponent<NetworkMovement>());
			gameObject.AddComponent<NetworkMovementRB>();
			BoxCollider collider = gameObject.GetComponent<BoxCollider>();
			collider.isTrigger = false;
			collider.center = new Vector3(0f, -0.13f, -0.3f);
			collider.size = new Vector3(0.5f, 0.2f, 1.2f);
			PickUpFactory pickup = gameObject.AddComponent<PickUpFactory>();
			pickup.babyCannotGrab = true;
			gameObject.tag = "Untagged";
		}
	}

	[HarmonyPatch]
	internal class DeathHook
	{
		[HarmonyPatch(typeof(BabyStats), "Start")]
		[HarmonyPostfix]
		static void Start(ref BabyStats __instance)
		{
			__instance.gameObject.AddComponent<DeathRPC>();
		}

		[HarmonyPatch(typeof(BabyStats), "Dead")]
		[HarmonyPostfix]
		static void Dead(ref BabyStats __instance)
		{
			__instance.GetComponent<PhotonView>().RPC("DeadRPC", PhotonTargets.All);
		}
	}
}