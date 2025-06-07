using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(VelocityBreak))]
	internal class GlassCupSoundDesyncFix
	{
		[HarmonyPatch("Main")]
		[HarmonyPostfix]
		static void Main(ref VelocityBreak __instance)
		{
			__instance.GetComponent<Collider>().contactOffset = 0.001f; // this removes most of the bullshit moments where it doesn't touch anything and breaks
		}

		[HarmonyPatch("OnCollisionEnter")]
		[HarmonyPrefix]
		static bool OnCollisionEnter(Collision col, ref VelocityBreak __instance)
		{
			if (__instance.enabled && __instance.GetComponent<Rigidbody>().velocity.magnitude > __instance.breakVelocity)
			{
				if ((bool)GameObject.Find("GlassChallenge"))
				{
					GameObject.Find("GlassChallenge").SendMessage("RemoveObj", __instance.gameObject);
				}

				PhotonView netView = __instance.GetComponent<PhotonView>();

				if (netView.isMine)
				{
					netView.RPC("RPCDestroy", PhotonTargets.All);
					PhotonNetwork.Instantiate(__instance.breakObj.name, __instance.transform.position, __instance.transform.rotation, 0);
					PhotonNetwork.Destroy(__instance.gameObject);
					__instance.enabled = false;
				}
			}

			return false;
		}

		[HarmonyPatch("RPCDestroy")]
		[HarmonyPrefix]
		static bool RPCDestroy(ref VelocityBreak __instance)
		{
			Object.Instantiate(__instance.breakSound, __instance.transform.position, __instance.transform.rotation);
			return false;
		}
	}
}
