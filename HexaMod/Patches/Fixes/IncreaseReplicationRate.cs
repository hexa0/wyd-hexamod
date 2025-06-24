using HarmonyLib;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(PhotonNetworkManager))]
	internal class IncreaseReplicationRate
	{

		[HarmonyPatch("ConnectToPhoton")]
		[HarmonyPostfix]
		static void IncreaseReplicationRatePatch()
		{
			PhotonNetwork.sendRate = HexaGlobal.sendRate;
			PhotonNetwork.sendRateOnSerialize = HexaGlobal.sendRate;
		}
	}
}
