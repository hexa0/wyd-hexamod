using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(PhotonNetworkManager))]
	internal class ConnectSpamFix
	{
		[HarmonyPatch("ConnectToPhoton")]
		[HarmonyPrefix]
		static bool ConnectToPhoton(ref PhotonNetworkManager __instance)
		{
			if (PhotonNetwork.connectionState != ConnectionState.Disconnected)
			{
				// fixes console spam
				return false;
			}

			return true;
		}
	}
}
