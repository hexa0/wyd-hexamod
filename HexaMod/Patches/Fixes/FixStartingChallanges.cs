using HarmonyLib;
using HexaMod.UI.Util;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(ChallengeStarter))]
	internal class FixStartingChallanges
	{
		[HarmonyPatch("StartChallenge")]
		[HarmonyPrefix]
		static void StartChallenge()
		{
			Menu.menuCanvas.transform.Find("InGameElements").gameObject.SetActive(true);
		}
	}
}