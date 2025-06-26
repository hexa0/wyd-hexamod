using HexaMod.UI.Element;
using HexaMod.UI.Element.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Menu
{
	public class FadeOverlay : HexaUIElement
	{
		public LinearCanvasGroupFader fader;

		RectTransform background;
		Image backgroundImage;

		public FadeOverlay() : base()
		{
			gameObject = new GameObject("fadeOverlay", typeof(RectTransform));

			fader = new LinearCanvasGroupFader()
				.SetParent(rectTransform, false)
				.ScaleWithParent()
				.SetFadeSpeed(2f)
				.SetInitialFadeState(false);

			background = new GameObject("background", typeof(RectTransform)).GetComponent<RectTransform>();
			background.SetParent(fader.rectTransform, false);
			background.ScaleWithParent();

			backgroundImage = background.gameObject.AddComponent<Image>();
			backgroundImage.color = new Color(0f, 0f, 0f, 1f);
		}
	}
}
