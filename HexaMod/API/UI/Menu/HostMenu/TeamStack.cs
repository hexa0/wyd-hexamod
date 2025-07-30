using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Label;
using HexaMod.API.UI.Element.Utility;
using HexaMod.API.Util.WhosYourDaddy;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Menu.HostMenu
{
	public class TeamStack : HexaUIElement
	{
		public UIElementStack playerStack;
		public ScrollRect scrollRect;
		public RectTransform viewport;
		public Image scrollBackground;
		public Mask scrollMask;
		public WLabel teamLabel;

		public TeamStack(Team team) : base()
		{
			gameObject = new GameObject("teamStack", typeof(RectTransform));
			scrollRect = gameObject.AddComponent<ScrollRect>();

			playerStack = new UIElementStack(5f)
				.SetParent(rectTransform)
				.SetName("playerStack")
				.SetAnchors(0.5f, 0.5f)
				.SetPivot(0.5f, 0.5f)
				.SetAlignment(UIElementStack.StackAlignment.TopToBottom);

			scrollRect.content = playerStack.rectTransform;
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.scrollSensitivity = 10f;
			scrollRect.inertia = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.verticalScrollbar = playerStack.gameObject.AddComponent<Scrollbar>();
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.verticalScrollbarSpacing = 0f;
			scrollRect.viewport = viewport = new GameObject("viewport", typeof(RectTransform)).SetParent(gameObject).transform as RectTransform;
			viewport.ScaleWithParent();
			scrollBackground = viewport.gameObject.AddComponent<Image>();
			scrollBackground.sprite = WUIGlobals.instance.resources.spriteInputField128;
			scrollBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
			scrollBackground.type = Image.Type.Sliced;
			scrollBackground.preserveAspect = true;
			scrollBackground.raycastTarget = false;
			scrollMask = viewport.gameObject.AddComponent<Mask>();
			scrollMask.showMaskGraphic = false;
			viewport.gameObject.AddComponent<CanvasRenderer>();
			viewport.gameObject.AddComponent<RectMask2D>();
			rectTransform.ScaleWithParent();

			teamLabel = new WLabel()
				.SetText(team.displayName)
				.SetTextFont(WUIGlobals.instance.resources.fontPrimary)
				.SetTextFontSize(WUIGlobals.Resources.FontSizes.Title)
				.SetParent(rectTransform)
				.SetAnchors(0.5f, 1f)
				.SetPivot(0.5f, 1f)
				.SetPosition(0f, 0f);
		}
	}
}
