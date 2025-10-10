using System.Collections.Generic;
using HexaMod.API.UI.Menu.PlayMenu;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Element.Control.ScrollList
{
	public class WScrollStack : HexaUIElement
	{
		readonly List<HexaUIElement> children = new List<HexaUIElement>();
		public float gap = 5f;

		readonly RectTransform contentBackground;
		readonly RectTransform contentMaskArea;
		readonly RectTransform contentArea;
		readonly VerticalLayoutGroup verticalLayoutGroup;
		readonly ContentSizeFitter contentSizeFilter;
		readonly RectTransform scrollBarArea;
		readonly RectTransform scrollBarHandle;
		readonly ScrollRect scrollRect;
		readonly Scrollbar scrollBar;

		public WScrollStack AddChild(HexaUIElement child)
		{
			children.Add(child
				.SetParent(contentArea)
				.SetPivot(0.5f, 0.5f)
				.SetPosition(0f, 0f)
			);

			return this;
		}

		public WScrollStack RemoveChild(HexaUIElement child)
		{
			children.Remove(child);
			child.SetParent(null);

			return this;
		}

		public WScrollStack Clear()
		{
			// temporary array so we don't itterate while modifying
			HexaUIElement[] allChildren = children.ToArray();

			foreach (HexaUIElement child in allChildren)
			{
				RemoveChild(child);
			}

			return this;
		}

		public WScrollStack SetScrollbarSize(float scrollbarSize)
		{
			contentMaskArea.anchorMax = new Vector2(1f - scrollbarSize, 1f);
			scrollBarArea.anchorMin = new Vector2(1f - scrollbarSize, 0f);

			return this;
		}

		public WScrollStack() : base()
		{
			gameObject = new GameObject("scrollStack", typeof(RectTransform));

			contentMaskArea = new GameObject("contentMaskArea", typeof(RectTransform)).transform as RectTransform;
			contentMaskArea.sizeDelta = Vector2.zero;
			contentMaskArea.pivot = new Vector2(0.5f, 0.5f);
			contentMaskArea.anchorMin = new Vector2(0f, 0f);
			contentMaskArea.SetParent(rectTransform, false);
			contentMaskArea.gameObject.AddComponent<Image>();
			contentMaskArea.gameObject.AddComponent<Mask>().showMaskGraphic = false;
			
			contentBackground = new GameObject("contentBackground", typeof(RectTransform)).transform as RectTransform;
			contentBackground.ScaleWithParent();
			contentBackground.SetParent(contentMaskArea, false);
			contentBackground.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.5f);
			
			contentArea = new GameObject("contentArea", typeof(RectTransform)).transform as RectTransform;
			contentArea.SetParent(contentBackground, false);
			contentArea.pivot = new Vector2(0.5f, 1f);
			contentArea.anchorMin = new Vector2(0f, 0f);
			contentArea.anchorMax = new Vector2(1f, 0f);
			contentArea.sizeDelta = Vector2.zero;
			
			verticalLayoutGroup = contentArea.gameObject.AddComponent<VerticalLayoutGroup>();
			verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
			verticalLayoutGroup.childForceExpandHeight = false;
			verticalLayoutGroup.childControlHeight = false;
			verticalLayoutGroup.spacing = gap;
			verticalLayoutGroup.padding = new RectOffset((int)(gap * 2), (int)(gap * 2), (int)(gap * 2), (int)(gap * 2));

			contentSizeFilter = contentArea.gameObject.AddComponent<ContentSizeFitter>();
			contentSizeFilter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			scrollBarArea = new GameObject("scrollArea", typeof(RectTransform)).transform as RectTransform;
			scrollBarArea.sizeDelta = Vector2.zero;
			scrollBarArea.pivot = new Vector2(0.5f, 0.5f);
			scrollBarArea.anchorMax = new Vector2(1f, 1f);
			scrollBarArea.SetParent(rectTransform, false);
			scrollBarArea.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
			
			scrollBarHandle = new GameObject("scrollHandle", typeof(RectTransform)).transform as RectTransform;
			scrollBarHandle.SetParent(scrollBarArea, false);
			scrollBarHandle.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
			scrollBarHandle.anchorMin = new Vector2(0f, 0.4f);
			scrollBarHandle.anchorMax = new Vector2(1f, 0.6f);
			scrollBarHandle.offsetMin = Vector2.zero;
			scrollBarHandle.offsetMax = Vector2.zero;
			scrollBarHandle.anchoredPosition = Vector2.zero;

			scrollBar = scrollBarArea.gameObject.AddComponent<Scrollbar>();
			scrollBar.direction = Scrollbar.Direction.BottomToTop;
			scrollBar.handleRect = scrollBarHandle;

			// Default scrollbar size to 5% of the width
			SetScrollbarSize(5f / 100f);

			scrollRect = gameObject.AddComponent<ScrollRect>();
			scrollRect.content = contentArea;
			scrollRect.viewport = contentMaskArea;
			scrollRect.verticalScrollbar = scrollBar;
			scrollRect.scrollSensitivity = 100f + gap;
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
			scrollRect.verticalScrollbarSpacing = 0;
		}
	}
}
