using HexaMod.UI.Element;
using HexaMod.UI.Element.HexaMod.Loading;
using HexaMod.UI.Element.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Menus
{
	public class StartupScreen : HexaUIElement
	{
		public LinearCanvasGroupFader fader;
		public LoadingText loadingText;
		public RectTransform loadingAnimationParent;
		public CornerLoadingAnimation cornerLoadingAnimation;

		RectTransform background;
		Image backgroundImage;

		public StartupScreen() : base()
		{
			gameObject = new GameObject("startupScreen", typeof(RectTransform));

			fader = new LinearCanvasGroupFader()
				.SetParent(rectTransform, false)
				.ScaleWithParent()
				.SetFadeSpeed(4f)
				.SetInitialFadeState(true);

			background = new GameObject("background", typeof(RectTransform)).GetComponent<RectTransform>();
			background.SetParent(fader.rectTransform, false);
			background.ScaleWithParent();

			backgroundImage = background.gameObject.AddComponent<Image>();
			backgroundImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);

			loadingText = new LoadingText()
				.SetParent(fader.rectTransform, false)
				.SetPivot(0f, 0.5f)
				.EnableLogging();

			cornerLoadingAnimation = new CornerLoadingAnimation()
				.SetParent(fader.rectTransform, false);
		}
	}
}
