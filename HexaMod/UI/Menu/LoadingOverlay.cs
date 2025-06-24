using HexaMod.UI.Element;
using HexaMod.UI.Element.HexaMod.Loading;
using HexaMod.UI.Element.Utility;
using UnityEngine;

namespace HexaMod.UI.Menus
{
	public class LoadingOverlay : HexaUIElement
	{
		public LinearCanvasGroupFader fader;
		public CornerLoadingAnimation cornerLoadingAnimation;
		public LoadingController controller;

		public LoadingOverlay() : base()
		{
			gameObject = new GameObject("loadingOverlay", typeof(RectTransform));

			fader = new LinearCanvasGroupFader()
				.SetParent(rectTransform, false)
				.ScaleWithParent()
				.SetFadeSpeed(8f)
				.SetInitialFadeState(false);

			cornerLoadingAnimation = new CornerLoadingAnimation()
				.SetParent(fader.rectTransform, false);

			controller = gameObject.AddComponent<LoadingController>();
		}
	}
}
