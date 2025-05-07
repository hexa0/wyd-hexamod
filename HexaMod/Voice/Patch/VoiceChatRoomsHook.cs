using System;
using HarmonyLib;

namespace HexaMod.Voice
{
	[HarmonyPatch]
	internal class VoiceChatRoomsHook
	{
		[HarmonyPatch(typeof(PhotonNetwork), "LeaveRoom")]
		[HarmonyPrefix]
		static void LeaveRoom()
		{
			Mod.Print($"LeaveRoom");
			wantedRoom = null;
			if (VoiceChat.room != null)
			{
				VoiceChat.LeaveVoiceRoom();
			}
		}

		[HarmonyPatch(typeof(PhotonNetwork), "LeaveLobby")]
		[HarmonyPrefix]
		static void LeaveLobby()
		{
			Mod.Print($"LeaveLobby");
			wantedRoom = null;
			if (VoiceChat.room != null)
			{
				VoiceChat.LeaveVoiceRoom();
			}
		}

		[HarmonyPatch(typeof(PhotonNetwork), "Disconnect")]
		[HarmonyPrefix]
		static void Disconnect()
		{
			Mod.Print($"Disconnect");
			wantedRoom = null;
			if (VoiceChat.room != null)
			{
				VoiceChat.LeaveVoiceRoom();
			}
		}

		public static string wantedRoom = null;

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string) })]
		[HarmonyPrefix]
		static void JoinRoom(string roomName)
		{
			Mod.Print($"JoinRoom {roomName}");
			wantedRoom = roomName;
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string), typeof(string[]) })]
		[HarmonyPrefix]
		static void JoinRoom(string roomName, string[] expectedUsers)
		{
			Mod.Print($"JoinRoom (expectedUsers) {roomName}");
			wantedRoom = roomName;
		}

		[HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby) })]
		[HarmonyPrefix]
		static void CreateRoom(string roomName)
		{
			Mod.Print($"CreateRoom {roomName}");
			wantedRoom = roomName;
		}
	}
}
