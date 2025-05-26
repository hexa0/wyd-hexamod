using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(Fork))]
	internal class ForkStuck
	{

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void ForkStuckPatch(ref Fork __instance)
		{
			if (__instance.held)
			{
				PhotonNetworkManager networkManager = GameObject.Find("NetworkManager").GetComponent<PhotonNetworkManager>();
				if (__instance.transform.IsChildOf(networkManager.playerObj.transform))
				{
					var dadItemTargetting = networkManager.playerObj.GetComponent<DadItemTargeting>();
					var itemTargetting = networkManager.playerObj.GetComponent<ItemTargeting>();
					if (dadItemTargetting)
					{
						if (dadItemTargetting.heldItem == null && dadItemTargetting.heldItem2 == null)
						{
							// desynced!
							__instance.held = false;
							__instance.Drop(Vector3.zero);
						}
					}
					else if (itemTargetting)
					{
						if (itemTargetting.heldItem == null && itemTargetting.heldItem2 == null)
						{
							// desynced!
							__instance.held = false;
							__instance.Drop(Vector3.zero);
						}
					}
				}
			}
		}
	}
}
