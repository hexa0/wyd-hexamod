using HexaMod.UI.Interface.Label;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Element.Label
{
	public class WLabel : HexaUIElement, IText<WLabel>, IShadow<WLabel>
	{
		public readonly static Font fallback = Resources.GetBuiltinResource<Font>("Arial.ttf");
		public readonly Text text;
		public readonly Outline outline;

		public WLabel() : base()
		{
			gameObject = new GameObject("wLabel", typeof(RectTransform));

			text = gameObject.AddComponent<Text>();
			text.font = WUIGlobals.instance?.fonts.primary;
			if (text.font == null)
			{
				text.font = fallback;
			}
			text.fontSize = (int)WUIGlobals.Fonts.Sizes.ButtonRegular;
			text.alignment = TextAnchor.MiddleCenter;
			text.raycastTarget = false;

			outline = gameObject.AddComponent<Outline>();
			outline.effectColor = Color.black;
		}

		public WLabel SetText(string text)
		{
			this.text.text = text;
			return this;
		}

		public WLabel SetTextFont(Font font)
		{
			text.font = font;
			return this;
		}

		public WLabel SetTextFontSize(object fontSize)
		{
			text.fontSize = (int)fontSize;
			return this;
		}

		public WLabel SetTextFontResizeMinSize(int minFontSize)
		{
			text.resizeTextMinSize = minFontSize;
			return this;
		}

		public WLabel SetTextFontResizeMaxSize(int maxFontSize)
		{
			text.resizeTextMaxSize = maxFontSize;
			return this;
		}

		public WLabel SetTextAligment(TextAnchor alignment)
		{
			text.alignment = alignment;
			return this;
		}

		public WLabel SetRichTextEnabled(bool richTextEnabled)
		{
			text.supportRichText = richTextEnabled;
			return this;
		}

		public WLabel SetResizeForBestFitEnabled(bool dynamicResizeEnabled)
		{
			text.resizeTextForBestFit = dynamicResizeEnabled;
			return this;
		}

		public WLabel SetShadowEnabled(bool isShadowEnabled)
		{
			outline.enabled = isShadowEnabled;
			return this;
		}

		public WLabel SetShadowColor(Color color)
		{
			outline.effectColor = color;
			return this;
		}

		public WLabel SetShadowDistance(Vector2 distance)
		{
			outline.effectDistance = distance;
			return this;
		}

		public string GetText()
		{
			return text.text;
		}
	}
}