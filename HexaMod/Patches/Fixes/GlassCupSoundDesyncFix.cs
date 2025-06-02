using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(VelocityBreak))]
	internal class GlassCupSoundDesyncFix
	{
		[HarmonyPatch("OnCollisionEnter")]
		[HarmonyPrefix]
		static bool OnCollisionEnter(ref VelocityBreak __instance)
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
