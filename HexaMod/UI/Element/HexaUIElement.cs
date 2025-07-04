﻿using HexaMod.UI.Element;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexaMod.UI.Element
{
	public class HexaUIElementBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
	{
		bool initialized = false;
		HexaUIElement element;

		bool m_shown = false;
		bool Shown {
			get => element.IsShown;
			set => element.IsShown = value;
		}

		public void Initialize(HexaUIElement linkedElement)
		{
			element = linkedElement;
			initialized = true;

			Shown = m_shown;
		}

		void Update()
		{
			if (m_shown)
			{
				element.Update();
			}
		}

		void FixedUpdate()
		{
			if (m_shown)
			{
				element.FixedUpdate();
			}
		}

		void OnEnable()
		{
			m_shown = true;

			if (initialized)
			{
				Shown = m_shown;
			}
		}

		void OnDisable()
		{
			m_shown = false;

			if (initialized)
			{
				Shown = m_shown;
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			element.MouseDown(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			element.MouseUp(eventData);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			element.MouseEnter(eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			element.MouseLeave(eventData);
		}
	}

	public class HexaUIElement
	{
		public GameObject gameObject;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "why")]
		public RectTransform rectTransform => gameObject.GetComponent<RectTransform>();

		public HexaUIElement()
		{

		}

		public HexaUIElement(GameObject gameObject)
		{
			this.gameObject = gameObject;
		}

		public virtual void Init()
		{
			HexaUIElementBehaviour behavior = gameObject.GetComponent<HexaUIElementBehaviour>();

			if (!behavior)
			{
				gameObject.SetActive(true);
				behavior = gameObject.AddComponent<HexaUIElementBehaviour>();
				behavior.Initialize(this);
			}
		}

		public virtual void Update()
		{

		}

		public virtual void FixedUpdate()
		{

		}

		public virtual void MouseEnter(PointerEventData eventData)
		{

		}

		public virtual void MouseLeave(PointerEventData eventData)
		{

		}

		public virtual void MouseDown(PointerEventData eventData)
		{

		}

		public virtual void MouseUp(PointerEventData eventData)
		{

		}

		bool m_shown = false;
		public bool IsShown
		{
			get => m_shown;
			set {
				m_shown = value;

				if (m_shown)
				{
					Shown();
					FixedUpdate();
					Update();
				}
				else
				{
					Hidden();
				}
			}
		}

		public virtual void Shown()
		{

		}

		public virtual void Hidden()
		{

		}
	}
}

static class HexaUIElementGenericMethodExtensions
{
	public static Element SetParent<Element>(this Element element, Transform menu, bool worldPositionStays = false) where Element : HexaUIElement
	{
		element.gameObject.transform.SetParent(menu, worldPositionStays);
		element.Init();
		return element;
	}

	public static Element SetPosition<Element>(this Element element, Vector2 position, bool localSpace = true) where Element : HexaUIElement
	{
		if (localSpace)
		{
			element.rectTransform.localPosition = position;
		}
		else
		{
			element.rectTransform.position = position;
		}

		return element;
	}

	public static Element SetPosition<Element>(this Element element, float x, float y, bool localSpace = true) where Element : HexaUIElement
	{
		element.SetPosition(new Vector2(x, y), localSpace);
		return element;
	}

	public static Element SetPivotPosition<Element>(this Element element, Vector2 position) where Element : HexaUIElement
	{
		element.rectTransform.SetPivotPosition(position);
		return element;
	}

	public static Element SetPivotPosition<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetPivotPosition(new Vector2(x, y));
		return element;
	}

	public static Element SetAnchorPosition<Element>(this Element element, Vector2 position) where Element : HexaUIElement
	{
		element.rectTransform.anchoredPosition = position;
		return element;
	}

	public static Element SetAnchorPosition<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchorPosition(new Vector2(x, y));
		return element;
	}

	public static Element SetAnchorMin<Element>(this Element element, Vector2 anchor) where Element : HexaUIElement
	{
		element.rectTransform.anchorMin = anchor;
		return element;
	}

	public static Element SetAnchorMin<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchorMin(new Vector2(x, y));
		return element;
	}

	public static Element SetAnchorMax<Element>(this Element element, Vector2 anchor) where Element : HexaUIElement
	{
		element.rectTransform.anchorMax = anchor;
		return element;
	}

	public static Element SetAnchorMax<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchorMax(new Vector2(x, y));
		return element;
	}


	public static Element SetAnchors<Element>(this Element element, Vector2 anchor) where Element : HexaUIElement
	{
		element.rectTransform.anchorMin = anchor;
		element.rectTransform.anchorMax = anchor;
		return element;
	}

	public static Element SetAnchors<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchors(new Vector2(x, y));
		return element;
	}

	public static Element SetPivot<Element>(this Element element, Vector2 position) where Element : HexaUIElement
	{
		element.rectTransform.pivot = position;
		return element;
	}

	public static Element SetPivot<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetPivot(new Vector2(x, y));
		return element;
	}

	public static Element ScaleWithParent<Element>(this Element element) where Element : HexaUIElement
	{
		element.rectTransform.ScaleWithParent();
		return element;
	}

	public static Element Resize<Element>(this Element element, Vector2 size) where Element : HexaUIElement
	{
		element.rectTransform.sizeDelta = size;
		return element;
	}

	public static Element Resize<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.Resize(new Vector2(x, y));
		return element;
	}

	public static Element SetName<Element>(this Element element, string name) where Element : HexaUIElement
	{
		element.gameObject.name = name;
		return element;
	}

	public static string GetName<Element>(this Element element) where Element : HexaUIElement
	{
		return element.gameObject.name;
	}
}
