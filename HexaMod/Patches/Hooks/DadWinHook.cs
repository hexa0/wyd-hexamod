using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Hooks
{
	public class WinManager
	{
		public static PhotonPlayer lastPlayerWon;
	}

	[HarmonyPatch(typeof(GameStateController))]
	internal class DadWinHook
	{
		[HarmonyPatch("DadWins")]
		[HarmonyPostfix]
		static void DadWins()
		{
			foreach (FirstPersonController firstPersonController in Object.FindObjectsOfType<FirstPersonController>())
			{
				if (firstPersonController.name.Substring(0, 3) == "Dad")
				{
					WinManager.lastPlayerWon = firstPersonController.GetComponent<PhotonView>().owner;
					break;
				}
			}
		}
	}
}
