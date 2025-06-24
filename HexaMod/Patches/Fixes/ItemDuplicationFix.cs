using HarmonyLib;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class ItemDuplicationFix
	{
		[HarmonyPatch(typeof(ItemSpawner), "NetworkSpawnObjects")]
		[HarmonyPrefix]
		static bool NetworkSpawnObjectsPatch()
		{
			return PhotonNetwork.isMasterClient;
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "PlayerSpawnBaby")]
		[HarmonyPrefix]
		static void PlayerSpawnBaby()
		{
			if (PhotonNetwork.isMasterClient)
			{
				ItemSpawner itemSpawner = HexaGlobal.networkManager.itemSpawner.GetComponent<ItemSpawner>();
				itemSpawner.NetworkSpawnObjects();
			}
		}
	}
}
