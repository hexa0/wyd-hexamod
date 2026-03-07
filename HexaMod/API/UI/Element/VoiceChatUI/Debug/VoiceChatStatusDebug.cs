using HexaMod.API.Voice;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Element.VoiceChatUI.Debug
{
	public class VoiceChatStatusDebug : HexaUIElement
	{
		readonly Text text;

		public override void Update()
		{
			base.Update();

			text.text =
				$"room: {(VoiceChat.room ?? "null")}\n"+
				$"relay: {(VoiceChat.relayIp != null ? (PhotonNetwork.isMasterClient ? VoiceChat.relayIp : "[Hidden]") : "null")}\n" +
				$"listening: {VoiceChat.listening}\n" +
				$"microphoneDeviceId: {VoiceChat.microphoneDeviceId.Value}\n" +
				$"microphoneBitrate: {VoiceChat.microphoneBitrate.Value}\n" +
				$"microphoneBufferMillis: {VoiceChat.microphoneBufferMillis.Value}\n" +
				$"denoisingEnabled: {VoiceChat.denoisingEnabled.Value}\n" +
				$"transcodeServerReady: {VoiceChat.transcodeReady}\n";
		}

		public VoiceChatStatusDebug() : base()
		{
			gameObject = new GameObject("voiceChatStatusDebug", typeof(RectTransform));
			rectTransform.sizeDelta = new Vector2(500f, 100f);
			rectTransform.pivot = new Vector2(0f, 0f);

			text = gameObject.AddComponent<Text>();
			text.resizeTextForBestFit = true;
			text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		}
	}
}
