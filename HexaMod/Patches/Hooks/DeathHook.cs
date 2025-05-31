using System.Collections;
using HarmonyLib;
using HexaMapAssemblies;
using UnityEngine;
using UnityStandardAssets.Effects;

namespace HexaMod.Patches.Hooks
{
	internal class DeathRPC : MonoBehaviour
	{
		bool isDead = false;
		[PunRPC]
		public IEnumerator DeadRPC()
		{
			if (!isDead)
			{
				isDead = true;

				gameObject.name = "Dead Baby";
				Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
				rigidBody.interpolation = RigidbodyInterpolation.Interpolate;
				rigidBody.isKinematic = false;
				rigidBody.useGravity = true;
				DestroyImmediate(gameObject.GetComponent<NetworkMovement>());
				NetworkMovementRB networkMovement = gameObject.AddComponent<NetworkMovementRB>();
				networkMovement.timer = 5f;
				BoxCollider collider = gameObject.GetComponent<BoxCollider>();
				collider.isTrigger = false;
				collider.center = new Vector3(0f, -0.13f, -0.3f);
				collider.size = new Vector3(0.5f, 0.2f, 1.2f);
				PickUpFactory pickup = gameObject.AddComponent<PickUpFactory>();
				pickup.babyCannotGrab = true;
				gameObject.tag = "Untagged";

				yield return new WaitForEndOfFrame();

				if ((Time.time - DeathHook.lastExplosionTime) < 1f)
				{
					float multiplier = DeathHook.lastExplosion.GetComponent<ParticleSystemMultiplier>().multiplier;
					float radius = 20f * multiplier;
					Vector3 underBabyExplosionPosition = DeathHook.lastExplosion.transform.position;
					underBabyExplosionPosition.y = rigidBody.position.y - 5f;
					rigidBody.AddExplosionForce((DeathHook.lastExplosion.explosionForce * 15f) * multiplier, underBabyExplosionPosition, radius, 1f * multiplier, ForceMode.Impulse);
				}
			}
		}
	}

	[HarmonyPatch]
	internal class DeathHook
	{
		internal static ExplosionPhysicsForce lastExplosion;
		internal static float lastExplosionTime = 0f;

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

		[HarmonyPatch(typeof(BabyStats), "ShockedRPC")]
		[HarmonyPrefix]
		static bool ShockedRPC(ref BabyStats __instance)
		{
			__instance.heat = 200f;
			__instance.cooking = true;
			__instance.Dead();

			return false;
		}

		[HarmonyPatch(typeof(BabyStats), "Shocked")]
		[HarmonyPrefix]
		static bool Shocked(ref BabyStats __instance)
		{
			__instance.GetComponent<PhotonView>().RPC("ShockedRPC", PhotonTargets.All);
			return false;
		}

		[HarmonyPatch(typeof(ExplosionPhysicsForce), "Start")]
		[HarmonyPrefix]
		static void ExplosionPhysicsForceStart(ref ExplosionPhysicsForce __instance)
		{
			lastExplosion = __instance;
			lastExplosionTime = Time.time;
		}
	}
}