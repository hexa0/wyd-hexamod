using UnityEngine;

namespace HexaMod.UI.Element.HexaMod.Loading
{
	public class LoadingAnimation : HexaUIElement
	{
		public static GameObject loadingAnimation;
		bool waitingForPrefab = false;
		RectTransform animation;

		void InstantiatePrefab()
		{
			waitingForPrefab = false;

			animation = Object.Instantiate(loadingAnimation).GetComponent<RectTransform>();
			animation.SetParent(rectTransform, false);
			animation.pivot = new Vector2(0f, 0f);
			animation.localPosition = Vector3.zero;
			animation.ScaleWithParent();
		}

		public override void Update()
		{
			base.Update();

			if (waitingForPrefab && loadingAnimation != null)
			{
				InstantiatePrefab();
			}
		}

		public LoadingAnimation() : base()
		{
			gameObject = new GameObject("loadingAnimation", typeof(RectTransform));
			waitingForPrefab = loadingAnimation == null;

			if (!waitingForPrefab)
			{
				InstantiatePrefab();
			}

			this.SetPivot(0f, 0f)
				.SetPosition(0f, 0f);
		}
	}
}
