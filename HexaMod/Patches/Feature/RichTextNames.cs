using HarmonyLib;
using UnityEngine.UI;

namespace HexaMod.Voice
{
	[HarmonyPatch]
	internal class RichTextNames
	{
		[HarmonyPatch(typeof(PhotonNetworkManager), "Start")]
		[HarmonyPrefix]
		static void RefreshNameList(ref PhotonNetworkManager __instance)
		{
			__instance.lobbyInputField.characterLimit = 200;
			__instance.lobbyInputField.textComponent.supportRichText = true;
		}

		[HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
		[HarmonyPostfix]
		static void RefreshNameList(ref PlayerNames __instance)
		{
			foreach (Text name in __instance.daddyNames)
			{
				name.supportRichText = true;
			}

			foreach (Text name in __instance.babyNames)
			{
				name.supportRichText = true;
			}
		}
	}
}
