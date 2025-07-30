using HarmonyLib;
using HexaMod.API.UI.Util;
using HexaMod.API.Util.Patching;

namespace HexaMod.Patches.Fixes
{
	[ModdedPatch]
	[HarmonyPatch]
	internal class FixStartingLocalGames
	{
		[HarmonyPatch(typeof(ChallengeStarter), "StartChallenge")]
		[HarmonyPrefix][ModdedPatch]
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