namespace HexaMod.API.UI
{
	internal class SplitscreenUtil
	{
		public static bool IsInSplitscreen()
		{
			return PhotonNetwork.offlineMode && PhotonNetwork.room != null && PhotonNetwork.room.Name != HexaGlobal.instanceGuid;
		}
	}
}
