using System;
using System.Collections;
using HarmonyLib;
using HexaMod.UI.Util;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.Patches.Hooks
{
	[HarmonyPatch]
	internal class RobustLobbyHook
	{
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
			if (PhotonNetwork.isMasterClient && PhotonNetwork.room != null && !PhotonNetwork.room.IsOpen)
			{
				HexaMod.persistentLobby.lobbySettings.roundNumber++;
				HexaMod.persistentLobby.CommitChanges();
			}
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "SpawnPlayerInLobby")]
		[HarmonyPrefix]
		static bool SpawnPlayerInLobby()
		{
			return false;
		}

		[HarmonyPatch(typeof(PlayerNames), "SendPlayerLists")]
		[HarmonyPrefix]
		static bool SendPlayerLists()
		{
			return !PhotonNetwork.isMasterClient;
		}

		[HarmonyPatch(typeof(PlayerNames), "MoveDaddy")]
		[HarmonyPrefix]
		static void MoveDaddy(int oldSpot, ref PlayerNames __instance)
		{
			PhotonPlayer player = __instance.daddyPlayerIds[oldSpot];
			HexaMod.persistentLobby.dads[player.ID] = false;
		}

		[HarmonyPatch(typeof(PlayerNames), "MoveBaby")]
		[HarmonyPrefix]
		static void MoveBaby(int oldSpot, ref PlayerNames __instance)
		{
			PhotonPlayer player = __instance.babyPlayerIds[oldSpot];
			HexaMod.persistentLobby.dads[player.ID] = true;
		}

		[HarmonyPatch(typeof(PlayerNames), "AddDaddy")]
		[HarmonyPrefix]
		static bool AddDaddy(string input, PhotonPlayer input2, ref PlayerNames __instance)
		{
			if (PhotonNetwork.isMasterClient)
			{
				__instance.daddyPlayerNames.Add(input);
				__instance.daddyPlayerIds.Add(input2);
				HexaMod.persistentLobby.dads[input2.ID] = true;
				HexaMod.networkManager.GetComponent<PhotonView>().RPC("SetIsDad", input2, true);
				__instance.GetComponent<PhotonView>().RPC("SendPlayerLists", PhotonTargets.Others, __instance.daddyPlayerNames.ToArray(), __instance.daddyPlayerIds.ToArray(), __instance.babyPlayerNames.ToArray(), __instance.babyPlayerIds.ToArray());
				__instance.RefreshNameList();
			}

			return false;
		}

		[HarmonyPatch(typeof(PlayerNames), "AddBaby")]
		[HarmonyPrefix]
		static bool AddBaby(string input, PhotonPlayer input2, ref PlayerNames __instance)
		{
			if (PhotonNetwork.isMasterClient)
			{
				__instance.babyPlayerNames.Add(input);
				__instance.babyPlayerIds.Add(input2);
				HexaMod.persistentLobby.dads[input2.ID] = false;
				HexaMod.networkManager.GetComponent<PhotonView>().RPC("SetIsDad", input2, false);
				__instance.GetComponent<PhotonView>().RPC("SendPlayerLists", PhotonTargets.Others, __instance.daddyPlayerNames.ToArray(), __instance.daddyPlayerIds.ToArray(), __instance.babyPlayerNames.ToArray(), __instance.babyPlayerIds.ToArray());
				__instance.RefreshNameList();
			}

			return false;
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "OnJoinedRoom")]
		[HarmonyPrefix]
		static bool OnJoinedRoom()
		{
			if (PhotonNetwork.room.Name.Contains(HexaMod.testGameGuid))
			{
				Mod.Warn("OnJoinedRoom Cancelled");
				return false;
			}

			if (PhotonNetwork.isMasterClient)
			{
				var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];
				Transform hostMenu = Menu.Menus.title.FindMenu(mode.hostMenuName);
				PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

				if (mode.hostDefaultTeamIsDad)
				{
					playerList.AddDaddy(HexaMod.networkManager.lobbyName, PhotonNetwork.player);
				}
				else
				{
					playerList.AddBaby(HexaMod.networkManager.lobbyName, PhotonNetwork.player);
				}
			}

			return false;
		}
	}
}