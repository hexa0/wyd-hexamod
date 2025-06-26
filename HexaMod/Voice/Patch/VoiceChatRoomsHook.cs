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
			inRoom = true;
		}

		[HarmonyPatch(typeof(PhotonNetwork), "JoinRoom", new Type[] { typeof(string), typeof(string[]) })]
		[HarmonyPrefix]
		static void JoinRoom(string roomName, string[] expectedUsers)
		{
			inRoom = true;
		}

		[HarmonyPatch(typeof(PhotonNetwork), "CreateRoom", new Type[] { typeof(string), typeof(RoomOptions), typeof(TypedLobby) })]
		[HarmonyPrefix]
		static void CreateRoom(string roomName)
		{
			inRoom = false;
		}
	}
}
