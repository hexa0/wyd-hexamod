using Boo.Lang;
using HarmonyLib;
using HexaMod.UI;
using HexaMod.Voice;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches.Hooks
{
	[HarmonyPatch]
	internal class UsePhotonNameSystem
	{
		[HarmonyPatch(typeof(PhotonNetworkManager), "ChangeLobbyName")]
		[HarmonyPostfix]
		static void ChangeLobbyName(ref PhotonNetworkManager __instance)
		{
			PhotonNetwork.playerName = __instance.lobbyName;
		}

		[HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
		[HarmonyPrefix]
		static void RefreshNameList(ref PlayerNames __instance)
		{
			for (int i = 0; i < __instance.daddyPlayerNames.Count; i++)
			{
				__instance.daddyPlayerNames[i] = HexaLobby.GetPlayerName(__instance.daddyPlayerIds[i]);
			}

			for (int i = 0; i < __instance.babyPlayerNames.Count; i++)
			{
				__instance.babyPlayerNames[i] = HexaLobby.GetPlayerName(__instance.babyPlayerIds[i]);
			}
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "Start")]
		[HarmonyPostfix]
		static void Start(ref PhotonNetworkManager __instance)
		{
			PhotonNetwork.playerName = __instance.lobbyName;
		}

		[HarmonyPatch(typeof(PhotonNetworkManager), "OnPhotonPlayerDisconnected")]
		[HarmonyPrefix]
		static bool OnPhotonPlayerDisconnected()
		{
			return false; // to be handled by hexalobby
		}

		[HarmonyPatch(typeof(RpcChat), "SendRPCChat")]
		[HarmonyPrefix]
		static bool SendRPCChat()
		{
			return false; // to be handled by our own mono behavior
		}

		// this uses PhotonNetwork.playerName instead and uses the global HexaMod.networkManager variable instead-
		// of the private one so we don't have to initialize it to call this

		[HarmonyPatch(typeof(RpcChat), "CheckWho")]
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
	public class RpcChatExtended : Photon.MonoBehaviour
	{
		public RpcChat chat;
		public List<string> messages = new List<string>();

		public void Init()
		{
			chat = GetComponent<RpcChat>();
			chat.CheckWho();

			transform.localPosition = new Vector3(0f, -920f, 0f);

			InputField inputField = GetComponent<InputField>();

			GameObject chatMask = transform.parent.parent.gameObject;
			chatMask.GetComponent<Mask>().enabled = false;
			chatMask.GetComponent<Image>().enabled = false;

			GameObject chatBox = transform.parent.gameObject;
			Destroy(chatBox.GetComponent<Image>());

			chatBox.GetComponent<RectTransform>().sizeDelta = new Vector2(
				600f,
				200f
			);

			GameObject scrollBar = transform.parent.Find("Scrollbar Vertical").gameObject;
			Destroy(scrollBar.GetComponent<Image>());

			Text chatTextContent = transform.parent.Find("Viewport").GetChild(0).GetChild(0).GetComponent<Text>();
			chatTextContent.fontSize = 15;
			chatTextContent.supportRichText = true;

			chatTextContent.gameObject.AddComponent<Outline>().effectDistance = new Vector2(-1.5f, -1.5f);

			RectTransform chatTextTransform = transform.parent.Find("Viewport").GetChild(0).GetChild(0).GetComponent<RectTransform>();

			chatTextTransform.pivot = new Vector2(
				0.3f,
				0.5f
			);

			chatTextTransform.sizeDelta = new Vector2(
				400f,
				25f
			);

			ColorBlock colors = inputField.colors;
			colors.normalColor = new Color(colors.normalColor.r, colors.normalColor.g, colors.normalColor.b, 0f);
			colors.disabledColor = new Color(colors.disabledColor.r, colors.disabledColor.g, colors.disabledColor.b, 0f);

			chat.scrollRect.verticalScrollbarSpacing = -20f;

			inputField.colors = colors;
			inputField.textComponent.supportRichText = true;
		}

		public void FixedUpdate()
		{
			chat.maskGroup.blocksRaycasts = HexaMod.networkManager.gameStarted;
		}

		public void SendUnformattedChatMessage(string message)
		{
			if (PhotonNetwork.inRoom)
			{
				photonView.RPC("OnChatMessage", PhotonTargets.All, message);
			}
		}

		public void SendFormattedChatMessage(string author, string message)
		{
			string chatColor = MainUI.GetCurrentShirtColorHex();

			if (!chatColor.StartsWith("#"))
			{
				chatColor = $"#{chatColor}";
			}

			SendUnformattedChatMessage($"<b><color=\"{chatColor}\">{author}</color></b>: {message}");
		}



		public void SendServerMessage(string message)
		{
			SendUnformattedChatMessage($"[Server] {message}");
		}

		public void SendLocalMessage(string message)
		{
			SendFormattedChatMessage(PhotonNetwork.playerName, message);
		}

		public void ShowChatBox(bool shown)
		{
			Color chatMaskColor = chat.theMask.color;
			chatMaskColor.a = shown ? 1 : 0;
			chat.theMask.color = chatMaskColor;
		}

		public void RenderMessages()
		{
			chat.chatBox.text = string.Join("\n", messages.ToArray()) + "\n";
			chat.content.sizeDelta = new Vector2(250f, chat.chatBox.preferredHeight);
			chat.textComp.anchoredPosition = new Vector2(-135f, chat.content.sizeDelta.y / 2f - 25f);
			chat.scrollRect.verticalNormalizedPosition = 0f;
		}

		public void ClearChatBox()
		{
			messages.Clear();
			RenderMessages();
		}

		[PunRPC]
		public void OnChatMessage(string chatMessage)
		{
			ShowChatBox(true);
			messages.Add(chatMessage);

			if (messages.Count > 15) // prevent VertexHelper from complaining about text being > 65000 vertices
			{
				messages.RemoveAt(0);
			}

			RenderMessages();
		}


		// we don't handle messages sent in the original format anymore, possibly re-enable this if i ever add support for vanilla clients to play although that sounds like a pipe dream because vanilla is borderline unplayably broken

		//[PunRPC]
		//public void SendRPCChat(string chatName, string chatMessage, PhotonMessageInfo info)
		//{
		//	if (chatName == "Name")
		//	{
		//		// this should never be reached as the variable is initialized as "Baby" but i don't want to mess up the original logic just in case
		//		// server messages just set the field to "Server" when sending it in the code from what i can see
		//		Mod.Fatal("the seemingly imposible chatName == \"Name\" condition was reached");
		//		chatName = "Server";
		//	}
		//	else if (chatName != "Server")
		//	{
		//		chatName = HexaLobby.GetPlayerName(info.sender, chatName);
		//	}

		//	OnChatMessage($"{chatName}: {chatMessage}");
		//}
	}
}