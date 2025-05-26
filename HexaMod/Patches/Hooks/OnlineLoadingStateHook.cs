using System;
using HarmonyLib;

namespace HexaMod.Patches.Hooks
{
	[HarmonyPatch]
	internal class OnlineLoadingStateHook
	{
		[HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby)})]
		[HarmonyPrefix]
		static void CreateRoom()
		{
			HexaMod.mainUI.loadingController.SetTaskState("RoomCreate", true);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string)})]
		[HarmonyPrefix]
		static void JoinRoom()
		{
			HexaMod.mainUI.loadingController.SetTaskState("RoomJoin", true);
			HexaMod.persistentLobby.SetInOtherLobby(true);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "LeaveRoom")]
		[HarmonyPrefix]
		static void LeaveRoom()
		{
			HexaMod.persistentLobby.SetInOtherLobby(false);
			HexaMod.persistentLobby.Reset();
		}

		[HarmonyPatch(typeof(PhotonNetwork), "Disconnect")]
		[HarmonyPrefix]
		static void Disconnect()
		{
			HexaMod.persistentLobby.SetInOtherLobby(false);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinLobby", new Type[] { typeof(TypedLobby) })]
		[HarmonyPrefix]
		static void JoinLobby()
		{
			HexaMod.mainUI.loadingController.SetTaskState("LobbyJoin", true);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "LeaveLobby")]
		[HarmonyPrefix]
		static void LeaveLobby()
		{
			HexaMod.mainUI.loadingController.SetTaskState("LobbyJoin", false);
		}

		[HarmonyPatch(typeof(PhotonNetwork), "ConnectUsingSettings")]
		[HarmonyPrefix]
		static void ConnectUsingSettings()
		{
			HexaMod.mainUI.loadingController.SetTaskState("PhotonConnect", true);
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "JoinWithTag")]
		[HarmonyPrefix]
		static void JoinWithTag()
		{
			HexaMod.mainUI.loadingController.SetTaskState("RoomLookForOrCreateTag", true);
		}
	}
}
