using HexaMod.UI.Element.Label;
using UnityEngine;

namespace HexaMod.UI.Element.HexaMod.Loading
{
	public class LoadingText : WLabel
	{
		public static Font loadingFont = null;
		Font currentFont = loadingFont;
		RectTransform parent;

		public bool enableLogging = false;

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			parent = rectTransform.GetParent();
		}

		public override void Update()
		{
			base.Update();

			if (currentFont != loadingFont)
			{
				currentFont = loadingFont;
				SetTextFont(currentFont);
			}

			this.Resize(parent.rect.width, parent.rect.height * 0.15f)
				.SetPivotPosition(0f, parent.rect.height / 2f);

			SetTextFontSize((int)(rectTransform.rect.height * 0.5f));
		}

		public new LoadingText SetText(string text)
		{
			base.SetText(text);

			if (GetText() != text)
			{
				if (enableLogging)
				{
					Mod.Print(text);
				}
			}

			return this;
		}

		public new LoadingText SetTextFont(Font font)
		{
			if (currentFont != font)
			{
				currentFont = font;
			}

			base.SetTextFont(currentFont);

			return this;
		}

		public LoadingText EnableLogging()
		{
			enableLogging = true;
			return this;
		}

		public LoadingText() : base()
		{
			this.SetPivot(0f, 0.5f)
				.SetTextAligment(TextAnchor.MiddleCenter)
				.SetShadowEnabled(false);
		}
	}
}