using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Element.HexaMod.Loading
{
	public class CornerLoadingAnimation : HexaUIElement
	{
		public LoadingAnimation loadingAnimation;
		public LoadingText loadingText;
		public RectTransform loadingTextParent;

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

			loadingTextParent = new GameObject("loadingTextParent", typeof(RectTransform)).GetComponent<RectTransform>();
			loadingTextParent.SetParent(rectTransform, false);
			loadingTextParent.offsetMin = Vector2.zero;
			loadingTextParent.offsetMax = Vector2.zero;
			loadingTextParent.anchorMin = new Vector2(0.5f, 0.45f);
			loadingTextParent.anchorMax = new Vector2(1f, 0.55f);
			loadingTextParent.anchoredPosition = Vector2.zero;
			loadingTextParent.sizeDelta = Vector2.zero;
			loadingTextParent.pivot = new Vector2(0.5f, 0.5f);

			loadingText = new LoadingText()
				.SetParent(loadingTextParent, false)
				.SetPosition(0f, 0f)
				.ScaleWithParent()
				.SetPivot(0.5f, 0.5f);
		}
	}
}
