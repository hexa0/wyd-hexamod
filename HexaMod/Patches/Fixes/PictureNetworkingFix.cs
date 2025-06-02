using System.Collections;
using HarmonyLib;
using HexaMapAssemblies;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	internal class PictureNetworking : MonoBehaviour
	{
		public bool[] brokenJoints;
		public Joint[] joints;

		[PunRPC]
		void OnJointIdBroken(byte id)
		{
			Destroy(joints[id]);
		}

		void OnJointBreak()
		{
			SendMessage("OnJointBrokenHost");
		}

		IEnumerator OnJointBrokenHost()
		{
			yield return 0;

			for (int i = 0; i < joints.Length; i++)
			{
				var joint = joints[i];
				if (joint == null)
				{
					if (!brokenJoints[i])
					{
						brokenJoints[i] = true;
						GetComponent<PhotonView>().RPC("OnJointIdBroken", PhotonTargets.Others, (byte)i);
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(PictureJointBreak))]
	internal class PictureNetworkingFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref PictureJointBreak __instance)
		{
			if (__instance.GetComponent<PhotonView>() == null)
			{
				GlobalPhotonFactory.Register(__instance.gameObject, true);
			}

			if (__instance.GetComponent<NetworkMovementRB>() == null)
			{
				__instance.gameObject.AddComponent<NetworkMovementRB>();
			}

			PictureNetworking picture = __instance.gameObject.AddComponent<PictureNetworking>();
			picture.joints = __instance.GetComponents<Joint>();
			picture.brokenJoints = new bool[picture.joints.Length];

			if (!PhotonNetwork.isMasterClient)
			{
				foreach (var joint in picture.joints)
				{
					joint.breakForce = float.PositiveInfinity;
					joint.breakTorque = float.PositiveInfinity;
				}
			}
		}
	}
}
