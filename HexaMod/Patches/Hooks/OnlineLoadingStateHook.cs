using System;
using HarmonyLib;
using HexaMod.UI;

namespace HexaMod.Patches.Hooks
{
	[HarmonyPatch]
	internal class OnlineLoadingStateHook
	{
		[HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby)})]
		[HarmonyPrefix]
		static void CreateRoom()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("RoomCreate", true);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string)})]
		[HarmonyPrefix]
		static void JoinRoom()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("RoomJoin", true);
			HexaPersistentLobby.instance.SetInOtherLobby(true);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "LeaveRoom")]
		[HarmonyPrefix]
		static void LeaveRoom()
		{
			HexaPersistentLobby.instance.SetInOtherLobby(false);
			HexaPersistentLobby.instance.Reset();
		}

		[HarmonyPatch(typeof(PhotonNetwork), "Disconnect")]
		[HarmonyPrefix]
		static void Disconnect()
		{
			HexaPersistentLobby.instance.SetInOtherLobby(false);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinLobby", new Type[] { typeof(TypedLobby) })]
		[HarmonyPrefix]
		static void JoinLobby()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("LobbyJoin", true);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "LeaveLobby")]
		[HarmonyPrefix]
		static void LeaveLobby()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("LobbyJoin", false);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "ConnectUsingSettings")]
		[HarmonyPrefix]
		static void ConnectUsingSettings()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("PhotonConnect", true);
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "JoinWithTag")]
		[HarmonyPrefix]
		static void JoinWithTag()
		{
			HexaMenus.loadingOverlay.controller.SetTaskState("RoomLookForOrCreateTag", true);
		}
	}
}
