using HarmonyLib;
using HexaMod.API.Util.Patching;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Hooks
{
	public class WinManager
	{
		public static PhotonPlayer lastPlayerWon;
	}

	[ModdedPatch]
	[HarmonyPatch(typeof(GameStateController))]
	internal class DadWinHook
	{
		[HarmonyPatch("DadWins")]
		[HarmonyPostfix]
		static void DadWins()
		{
			foreach (FirstPersonController firstPersonController in Object.FindObjectsOfType<FirstPersonController>())
			{
				if (firstPersonController.name.ToLower().StartsWith("dad"))
				{
					WinManager.lastPlayerWon = firstPersonController.GetComponent<PhotonView>().owner;
					break;
				}
			}
		}
	}
}
