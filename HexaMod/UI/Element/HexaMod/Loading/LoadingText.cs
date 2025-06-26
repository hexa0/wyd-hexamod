using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Element.HexaMod.Loading
{
	public class LoadingText : HexaUIElement
	{
		Text text;
		public static Font loadingFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
		Font currentFont = loadingFont;

		RectTransform textParent;

		public bool enableLogging = false;

		public override void Update()
		{
			base.Update();

			RectTransform parent = rectTransform.GetParent();

			if (currentFont != loadingFont)
			{
				currentFont = loadingFont;
				text.font = currentFont;
			}

			this.Resize(parent.rect.width, parent.rect.height * 0.15f)
				.SetPivotPosition(0f, parent.rect.height / 2f);

			text.fontSize = (int)(rectTransform.rect.height * 0.5f);
		}

		public LoadingText SetText(string text)
		{
			if (this.text.text != text)
			{
				if (enableLogging) {
					Mod.Print(text);
				}
				this.text.text = text;
			}

			return this;
		}

		public LoadingText EnableLogging()
		{
			enableLogging = true;
			return this;
		}

		public LoadingText SetAlignment(TextAnchor alignment)
		{
			text.alignment = alignment;
			return this;
		}

		public LoadingText() : base()
		{
			gameObject = new GameObject("loadingText", typeof(RectTransform));

			textParent = new GameObject("loadingText", typeof(RectTransform)).GetComponent<RectTransform>();
			textParent.SetParent(rectTransform, false);
			textParent.ScaleWithParent();

			text = textParent.gameObject.AddComponent<Text>();
			text.font = currentFont;
			text.supportRichText = true;

			// gameObject.AddComponent<Image>().color = Color.magenta;

			this.SetPivot(0f, 0.5f)
				.SetAlignment(TextAnchor.MiddleCenter);
		}
	}
}