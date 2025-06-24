using UnityEngine;

namespace HexaMod.UI.Element.HexaMod.Loading
{
	public class CornerLoadingAnimation : HexaUIElement
	{
		public LoadingAnimation loadingAnimation;

		public override void Update()
		{
			base.Update();

			float padding = Screen.height * 0.05f;
			float loadingAnimationSize = Screen.height * 0.2f;
			rectTransform.SetPivotPosition(Screen.width - (loadingAnimationSize / 2f) - padding, padding + loadingAnimationSize / 2f);
			rectTransform.sizeDelta = Vector2.one * loadingAnimationSize;
		}

		public CornerLoadingAnimation() : base()
		{
			gameObject = new GameObject("cornerLoadingAnimation", typeof(RectTransform));

			loadingAnimation = new LoadingAnimation()
				.SetParent(rectTransform, false)
				.ScaleWithParent();
		}
	}
}
