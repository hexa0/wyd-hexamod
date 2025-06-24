using UnityEngine;
using UnityEngine.UI;
using HexaMod.UI.Element.Control.TextButton;
using HexaMod.Voice;

namespace HexaMod.UI.Element.VoiceChatUI
{
	public class MicrophoneIndicator : WTextButton
	{
		private readonly static Sprite microphoneIcon = HexaGlobal.coreBundle.LoadAsset<Sprite>("Assets/ModResources/Core/Sprite/Microphone512.png");

		public MicrophoneIndicator() : base()
		{
			rectTransform.sizeDelta = new Vector2(50f, 50f);
			rectTransform.pivot = new Vector2(0f, 0.5f);
			this.SetName("microphoneVolume")
				.SetInteractable(false)
				.SetSprite(microphoneIcon)
				.SetText("")
				.SetFontSize(15);
		}

		public MicrophoneIndicator(string name, Transform menu, Vector2 position) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position);
		}

		public override void Shown()
		{
			if (VoiceChat.room == null && !VoiceChat.listening)
			{
				VoiceChat.StartListening();
			}
		}

		public override void Hidden()
		{
			if (VoiceChat.room == null && VoiceChat.listening)
			{
				VoiceChat.StopListening();
			}
		}

		public static AnimationCurve SetCurveLinear(AnimationCurve curve)
		{
			for (int i = 0; i < curve.keys.Length; ++i)
			{
				float intangent = 0;
				float outtangent = 0;
				bool intangent_set = false;
				bool outtangent_set = false;
				Vector2 point1;
				Vector2 point2;
				Vector2 deltapoint;
				Keyframe key = curve[i];

				if (i == 0)
				{
					intangent = 0; intangent_set = true;
				}

				if (i == curve.keys.Length - 1)
				{
					outtangent = 0; outtangent_set = true;
				}

				if (!intangent_set)
				{
					point1.x = curve.keys[i - 1].time;
					point1.y = curve.keys[i - 1].value;
					point2.x = curve.keys[i].time;
					point2.y = curve.keys[i].value;

					deltapoint = point2 - point1;

					intangent = deltapoint.y / deltapoint.x;
				}
				if (!outtangent_set)
				{
					point1.x = curve.keys[i].time;
					point1.y = curve.keys[i].value;
					point2.x = curve.keys[i + 1].time;
					point2.y = curve.keys[i + 1].value;

					deltapoint = point2 - point1;

					outtangent = deltapoint.y / deltapoint.x;
				}

				key.inTangent = intangent;
				key.outTangent = outtangent;
				curve.MoveKey(i, key);
			}

			return curve;
		}

		public Color speakingColor = new Color(255f / 255f, 173f / 255f, 168f / 255f);
		public Color normalColor = new Color(15f / 255f, 13f / 255f, 13f / 255f);
		public AnimationCurve speakingCurve = SetCurveLinear(new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(0.1f, 0.25f),
			new Keyframe(0.25f, 0.5f),
			new Keyframe(0.75f, 0.85f),
			new Keyframe(1f, 1f),
		}));

		public override void Update()
		{
			ColorBlock colors = button.colors;
			colors.fadeDuration = 0;
			colors.disabledColor = Color.Lerp(normalColor, speakingColor, speakingCurve.Evaluate(VoiceChat.currentPeak));
			// label.text = Mathf.Round(VoiceChat.currentPeak * 100f).ToString();
			button.colors = colors;
		}
	}
}
