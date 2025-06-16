using System;
using HarmonyLib;

namespace HexaMod.Voice
{
	[HarmonyPatch]
	internal class VoiceChatRoomsHook
	{
		public static bool inRoom = false;
		[HarmonyPatch(typeof(PhotonNetwork), "LeaveRoom")]
		[HarmonyPrefix]
		static void LeaveRoom()
		{
			Mod.Print($"LeaveRoom");
			inRoom = false;

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
			inRoom = false;

			if (VoiceChat.room != null)
			{
				VoiceChat.LeaveVoiceRoom();
			}
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string) })]
		[HarmonyPrefix]
		static void JoinRoom(string roomName)
		{
			Mod.Print($"JoinRoom {roomName}");
			inRoom = true;
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string), typeof(string[]) })]
		[HarmonyPrefix]
		static void JoinRoom(string roomName, string[] expectedUsers)
		{
			Mod.Print($"JoinRoom (expectedUsers) {roomName}");
			inRoom = true;
		}

		[HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby) })]
		[HarmonyPrefix]
		static void CreateRoom(string roomName)
		{
			Mod.Print($"CreateRoom {roomName}");
			inRoom = false;
		}
	}
}
