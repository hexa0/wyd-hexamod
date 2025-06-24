using HexaMod.UI.Element;
using HexaMod.UI.Element.Utility;
using HexaMod.UI.Element.VoiceChatUI.Debug;
using HexaMod.Voice;
using UnityEngine;

namespace HexaMod.UI.Menus
{
	public class VoiceChatDebugOverlay : HexaUIElement
	{
		public UIElementStack elementStack;
		bool overlayEnabled = true;

		public override void Update()
		{
			base.Update();

			if (VoiceChat.debugOverlayEnabled.Value != overlayEnabled)
			{
				overlayEnabled = VoiceChat.debugOverlayEnabled.Value;
				elementStack.gameObject.SetActive(overlayEnabled);
			}

			if (overlayEnabled)
			{
				elementStack.SetPosition(0f, Screen.currentResolution.height, false);
			}
		}

		public VoiceChatDebugOverlay() : base()
		{
			gameObject = new GameObject("voiceChatDebugOverlay", typeof(RectTransform));

			elementStack = new UIElementStack(5f)
				.SetParent(rectTransform)
				.SetAlignment(UIElementStack.StackAlignment.TopToBottom)
				.SetPivot(0f, 1f)
				.SetPosition(0f, 0f, false);

			elementStack.AddChild(new VoiceChatStatusDebug());
		}
	}
}
