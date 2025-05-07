using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches
{
	[HarmonyPatch]
	internal class RobustLobbyStartup
	{
		[HarmonyPatch(typeof(MenuController), "ChangeToMenu")]
		[HarmonyPrefix]
		static void ChangeToMenu(ref ImgFade __instance, ref int val)
		{
			// we don't use this menu so treat it showing up as an error
			if (val == 3)
			{
				// this triggers when you are abandoned so this cannot be done actually
				// throw new Exception("bad menu");
			}
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "RematchReady")]
		[HarmonyPostfix]
		static IEnumerator RematchReady(IEnumerator result)
		{
			yield return 0;

			Mod.Error("RematchReadyPatch was somehow called, trace:\n", Environment.StackTrace);
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "Rematch")]
		[HarmonyPrefix]
		static void Rematch()
		{
			HexaMod.persistentLobby.lobbySettings.roundNumber++;
		}
	}
}