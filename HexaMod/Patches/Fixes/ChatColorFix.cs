using HarmonyLib;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(RpcChat))]
	internal class ChatColorFix
	{
		[HarmonyPatch("SendChat")]
		[HarmonyPrefix]
		static bool SendChat(ref RpcChat __instance)
		{
			if (__instance.curChatMessage != string.Empty)
			{
				HexaMod.textChat.SendLocalMessage(__instance.curChatMessage);
			}

			__instance.CloseChat();

			return false;
		}

		[HarmonyPatch("CloseChat")]
		[HarmonyPrefix]
		static bool CloseChat(ref RpcChat __instance)
		{
			Traverse fields = Traverse.Create(__instance);
			var timer = fields.Field<float>("timer");

			__instance.CheckWho();
			__instance.openChat = false;
			__instance.hasFocus = false;
			__instance.curChatMessage = string.Empty;
			timer.Value = 0f;

			if (HexaMod.networkManager.playerObj)
			{
				HexaMod.networkManager.playerObj.GetComponent<FirstPersonController>().haltInput = false;
			}

			__instance.maskGroup.interactable = false;
			__instance.chatInput.DeactivateInputField();
			__instance.chatInput.text = string.Empty;
			EventSystem.current.SetSelectedGameObject(null);

			return false;
		}
	}
}
