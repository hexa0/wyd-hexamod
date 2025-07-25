using HarmonyLib;
using HexaMod.API.UI.Util;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class FixStartingLocalGames
	{
		[HarmonyPatch(typeof(ChallengeStarter), "StartChallenge")]
		[HarmonyPrefix]
		static void StartChallenge()
		{
			Menu.menuCanvas.Find("InGameElements").gameObject.SetActive(true);
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "CreateOfflineGame")]
		[HarmonyPrefix]
		static void CreateOfflineGame()
		{
			Menu.menuCanvas.Find("InGameElements").gameObject.SetActive(true);
		}
	}
}