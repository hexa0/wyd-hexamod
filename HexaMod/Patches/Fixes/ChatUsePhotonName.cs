using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(RpcChat))]
	internal class ChatUsePhotonName
	{
		[HarmonyPatch("SendRPCChat")]
		[HarmonyPrefix]
		static bool SendRPCChat()
		{
			return false; // to be handled by our own mono behavior
		}

		// this uses PhotonNetwork.playerName instead and uses the global HexaMod.networkManager variable instead-
		// of the private one so we don't have to initialize it to call this

		[HarmonyPatch("CheckWho")]
		[HarmonyPrefix]
		static bool CheckWho(ref RpcChat __instance)
		{
			if (HexaMod.networkManager.curGameMode == 0)
			{
				if (HexaMod.networkManager.isDad)
				{
					__instance.chatName = "Daddy";
				}
				else
				{
					__instance.chatName = "Baby";
				}
			}
			else
			{
				__instance.chatName = PhotonNetwork.playerName;
			}

			return false;
		}
	}

	// this is added elsewhere as the source of the problem is Start() not being called until the chat is opened by the user, which also populates the name field
	// also them just not EVER using info.sender.NickName like they should have from the start lmao
	public class RpcChatExtended : MonoBehaviour
	{
		RpcChat chat;

		public void Init()
		{
			chat = GetComponent<RpcChat>();
			chat.CheckWho();
		}

		[PunRPC]
		public virtual void SendRPCChat(string chatName, string chatMessage, PhotonMessageInfo info)
		{
			// show the chat box
			Color chatMaskColor = chat.theMask.color;
			chatMaskColor.a = 1;
			chat.theMask.color = chatMaskColor;

			if (chatName == "Name")
			{
				// this should never be reached as the variable is initialized as "Baby" but i don't want to mess up the original logic just in case
				// server messages just set the field to "Server" when sending it in the code from what i can see
				Mod.Fatal("the seemingly imposible chatName == \"Name\" condition was reached");
				chatName = "Server";
			}
			else if (chatName != "Server")
			{
				chatName = info.sender.NickName;
			}

			chat.chatBox.text = chat.chatBox.text + chatName + ": " + chatMessage + "\n";
			chat.content.sizeDelta = new Vector2(250f, chat.chatBox.preferredHeight);
			chat.textComp.anchoredPosition = new Vector2(-135f, chat.content.sizeDelta.y / 2f - 25f);
			chat.scrollRect.verticalNormalizedPosition = 0f;
		}
	}
}
