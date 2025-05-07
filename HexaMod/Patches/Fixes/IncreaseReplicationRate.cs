using HarmonyLib;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(PhotonNetworkManager))]
	internal class IncreaseReplicationRate
	{

		[HarmonyPatch("ConnectToPhoton")]
		[HarmonyPostfix]
		static void IncreaseReplicationRatePatch()
		{
			PhotonNetwork.sendRate = HexaMod.sendRate;
			PhotonNetwork.sendRateOnSerialize = HexaMod.sendRate;
		}
	}
}
