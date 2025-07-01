using UnityEngine;

namespace HexaMod.UI.Element.Utility
{
	public class LinearCanvasGroupFader : HexaUIElement
	{
		public RectTransform children;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "why")]
		public new RectTransform rectTransform
		{
			get => children;
		}

		readonly CanvasGroup group;

		bool cullChildren = true;
		public bool fadeState = true;
		float alpha = 1f;
		public float fadeSpeed = 1f;

		public override void Shown()
		{
			base.Shown();
		}

		public override void Update()
		{
			base.Update();

			float oldAlpha = alpha;

			if (fadeState)
			{
				alpha += Time.smoothDeltaTime * fadeSpeed;

				if (alpha > 1f)
				{
					alpha = 1f;
				}
			}
			else
			{
				alpha -= Time.smoothDeltaTime * fadeSpeed;

				if (alpha < 0f)
				{
					alpha = 0f;
				}
			}

			if (oldAlpha != alpha)
			{
				group.alpha = alpha;

				if (cullChildren)
				{
					if (alpha == 0f)
					{
						children.gameObject.SetActive(false);
					}
					else if (oldAlpha == 0f)
					{
						children.gameObject.SetActive(true);
					}
				}
			}
		}

		public LinearCanvasGroupFader SetFadeSpeed(float speed)
		{
			fadeSpeed = speed;
			return this;
		}

		public LinearCanvasGroupFader SetInitialFadeState(bool visible)
		{
			fadeState = visible;
			alpha = visible ? 1f : 0f;
			group.alpha = alpha;
			return this;
		}

		public LinearCanvasGroupFader SetChildrenCullingEnabled(bool shouldCullChildren)
		{
			if (shouldCullChildren)
			{
				children.gameObject.SetActive(alpha != 0f);
			}
			else
			{
				children.gameObject.SetActive(true);
			}

			cullChildren = shouldCullChildren;

			return this;
		}

		public LinearCanvasGroupFader() : base()
		{
			gameObject = new GameObject("fader", typeof(RectTransform));

			children = new GameObject("faderChildren", typeof(RectTransform)).GetComponent<RectTransform>();
			children.ScaleWithParent()
					.SetParent(base.rectTransform, false);

			group = gameObject.AddComponent<CanvasGroup>();

			SetInitialFadeState(true);
		}
	}
}
