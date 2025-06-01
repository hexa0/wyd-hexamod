using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	internal class PlayerRemover : MonoBehaviour // jane remover refernce 😱
	{
		void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
		{
			PhotonView netView = GetComponent<PhotonView>();

			if (oldPlayer.ID == netView.ownerId)
			{
				FirstPersonController controller = GetComponent<FirstPersonController>();
				controller.restrained = false;
				controller.transform.parent = null;
				controller.gameObject.SetActive(false);

				Destroy(controller.gameObject);
			}

		}
	}

	[HarmonyPatch(typeof(FirstPersonController))]
	internal class GhostFix
	{

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start()
		{
			
		}

		[HarmonyPatch("OnPhotonPlayerDisconnected")]
		[HarmonyPrefix]
		static bool OnPhotonPlayerDisconnected()
		{
			return false;
		}
	}
}
