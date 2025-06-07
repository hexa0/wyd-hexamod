using HarmonyLib;
using HexaMod.UI.Util;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class FixStartingLocalGames
	{
		[HarmonyPatch(typeof(ChallengeStarter), "StartChallenge")]
		[HarmonyPrefix]
		static void StartChallenge()
		{
			Menu.menuCanvas.transform.Find("InGameElements").gameObject.SetActive(true);
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "CreateOfflineGame")]
		[HarmonyPrefix]
		static void CreateOfflineGame()
		{
			Menu.menuCanvas.transform.Find("InGameElements").gameObject.SetActive(true);
		}
	}
}