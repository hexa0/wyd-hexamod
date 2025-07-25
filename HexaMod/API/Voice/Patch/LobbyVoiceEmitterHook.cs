using HarmonyLib;
using HexaMod.API.Voice.Script;

namespace HexaMod.API.Voice.Patch
{
	[HarmonyPatch]
	internal class LobbyVoiceEmitterHook
	{
		[HarmonyPatch(typeof(PlayerNames), "Start")]
		[HarmonyPostfix]
		static void Start(ref PlayerNames __instance)
		{
			__instance.gameObject.AddComponent<LobbyVoiceEmitterBehavior>();
		}

		[HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
		[HarmonyPostfix]
		static void RefreshNameList(ref PlayerNames __instance)
		{
			LobbyVoiceEmitterBehavior lobbyVoice = __instance.GetComponent<LobbyVoiceEmitterBehavior>();

			if (lobbyVoice != null)
			{
				lobbyVoice.Refresh();
			}
		}
	}
}
