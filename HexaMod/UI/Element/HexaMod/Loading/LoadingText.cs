using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Element.HexaMod.Loading
{
	public class LoadingText : HexaUIElement
	{
		Text text;
		public static Font loadingFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
		Font currentFont = loadingFont;

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

		public void SetText(string text)
		{
			this.text.text = text;
		}

		public LoadingText() : base()
		{
			gameObject = new GameObject("loadingText", typeof(RectTransform));
			text = gameObject.AddComponent<Text>();
			text.font = currentFont;
			text.supportRichText = true;
			text.alignment = TextAnchor.MiddleCenter;

			this.SetPivot(0f, 0.5f);
		}
	}
}
