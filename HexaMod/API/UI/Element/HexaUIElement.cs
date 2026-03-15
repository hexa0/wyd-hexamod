using HexaMod.API.UI.Element;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexaMod.API.UI.Element
{
	/// <summary> 
	/// base class for all declarative HexaMod UI elements,\
	/// </summary>
	public class HexaUIElement
	{
		/// <summary> 
		/// internal MonoBehaviour class for declarative HexaMod UI elements,\
		/// this tracks pointer events and our active hierarchy state and calls the element update methods
		/// </summary>
		protected class HexaUIElementBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
		{
			bool initialized = false;
			HexaUIElement element;


			/// <summary> 
			/// safe getter/setter for element.IsShown,\
			/// see HexaUIElement.IsShown for more details 
			/// </summary>
			bool Shown {
				get => element != null && element.IsShown;
				set {
					if (element != null) {
						element.IsShown = value;
					}
				}
			}

			/// <summary> 
			/// link ourselves to the correct HexaUIElement object and set our current active hierarchy state
			/// </summary>
			public void Initialize(HexaUIElement linkedElement)
			{
				element = linkedElement;
				initialized = true;

				Shown = gameObject.activeInHierarchy;
			}

			/// <summary> 
			/// if we're shown then update the linked element via Update
			/// </summary>
			void Update()
			{
				if (Shown)
				{
					element.Update();
				}
			}

			/// <summary> 
			/// if we're shown then update the linked element via FixedUpdate
			/// </summary>
			void FixedUpdate()
			{
				if (Shown)
				{
					element.FixedUpdate();
				}
			}

			/// <summary> 
			/// update our active hierarchy state
			/// </summary>
			void OnEnable()
			{
				if (initialized)
				{
					Shown = gameObject.activeInHierarchy;
				}
			}

			/// <summary> 
			/// update our active hierarchy state
			/// </summary>
			void OnDisable()
			{
				if (initialized)
				{
					Shown = gameObject.activeInHierarchy;
				}
			}

			/// <summary> 
			/// forward OnPointerDown event to the linked element
			/// </summary>
			public void OnPointerDown(PointerEventData eventData)
			{
				element.MouseDown(eventData);
			}

			/// <summary> 
			/// forward OnPointerUp event to the linked element
			/// </summary>
			public void OnPointerUp(PointerEventData eventData)
			{
				element.MouseUp(eventData);
			}

			/// <summary> 
			/// forward OnPointerEnter event to the linked element
			/// </summary>
			public void OnPointerEnter(PointerEventData eventData)
			{
				element.MouseEnter(eventData);
			}

			/// <summary> 
			/// forward OnPointerExit event to the linked element
			/// </summary>
			public void OnPointerExit(PointerEventData eventData)
			{
				element.MouseLeave(eventData);
			}
		}

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
		/// <summary> 
		/// Whether we are active in the hierarchy,\
		/// this does not test on-screen visibility
		/// </summary>
		public bool IsShown
		{
			get => m_shown;
			set {
				// prevent any funky double events if that ever comes up
				if (m_shown != value) {
					m_shown = value;

					if (m_shown)
					{
						Shown();
						// instantly update upon showing to prevent seeing unupdated UI for 1 frame
						FixedUpdate();
						Update();
					}
					else
					{
						Hidden();
					}
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

/// <summary> 
/// various generic utils for all declarative HexaMod UI elements
/// </summary>
static class HexaUIElementGenericMethodExtensions
{
	public static Element SetParent<Element>(this Element element, Transform menu, bool worldPositionStays = false) where Element : HexaUIElement
	{
		element.gameObject.transform.SetParent(menu, worldPositionStays);
		element.Init();
		return element;
	}

	/// <summary> 
	/// sets our position using our pivot point but not respecting the parent pivot point\
	/// this can behave weirdly if the parent has a pivot other then the top left corner\
	/// using SetPivotPosition instead is recommended for consistency
	/// </summary>
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

	/// <summary> 
	/// sets our position using our pivot point but not respecting the parent pivot point\
	/// this can behave weirdly if the parent has a pivot other then the top left corner\
	/// using SetPivotPosition instead is recommended for consistency
	/// </summary>
	public static Element SetPosition<Element>(this Element element, float x, float y, bool localSpace = true) where Element : HexaUIElement
	{
		element.SetPosition(new Vector2(x, y), localSpace);
		return element;
	}

	/// <summary> 
	/// sets our position using our pivot point and the parent pivot point\
	/// this is a way more logical behavior and is likely what you want
	/// </summary>
	public static Element SetPivotPosition<Element>(this Element element, Vector2 position) where Element : HexaUIElement
	{
		element.rectTransform.SetPivotPosition(position);
		return element;
	}

	/// <summary> 
	/// sets our position using the pivot point
	/// </summary>
	public static Element SetPivotPosition<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetPivotPosition(new Vector2(x, y));
		return element;
	}

	/// <summary> 
	/// sets our position among the parent element's area
	/// </summary>
	public static Element SetAnchorPosition<Element>(this Element element, Vector2 position) where Element : HexaUIElement
	{
		element.rectTransform.anchoredPosition = position;
		return element;
	}

	/// <summary> 
	/// sets our position among the parent element's area
	/// </summary>
	public static Element SetAnchorPosition<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchorPosition(new Vector2(x, y));
		return element;
	}

	/// <summary> 
	/// scales the element so that the bottom left corner position is the specified value among the parent
	/// </summary>
	public static Element SetAnchorMin<Element>(this Element element, Vector2 anchor) where Element : HexaUIElement
	{
		element.rectTransform.anchorMin = anchor;
		return element;
	}

	/// <summary> 
	/// scales the element so that the bottom left corner position is the specified value among the parent
	/// </summary>
	public static Element SetAnchorMin<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchorMin(new Vector2(x, y));
		return element;
	}

	/// <summary> 
	/// scales the element so that the top right position is the specified value among the parent
	/// </summary>
	public static Element SetAnchorMax<Element>(this Element element, Vector2 anchor) where Element : HexaUIElement
	{
		element.rectTransform.anchorMax = anchor;
		return element;
	}

	/// <summary> 
	/// scales the element so that the top right corner position is the specified value among the parent
	/// </summary>
	public static Element SetAnchorMax<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchorMax(new Vector2(x, y));
		return element;
	}


	/// <summary> 
	/// locks an element to the given position among the parent,\
	/// size is then governed by SizeDelta
	/// </summary>
	public static Element SetAnchors<Element>(this Element element, Vector2 anchor) where Element : HexaUIElement
	{
		element.rectTransform.anchorMin = anchor;
		element.rectTransform.anchorMax = anchor;
		return element;
	}

	/// <summary> 
	/// locks an element to the given position among the parent,\
	/// size is then governed by SizeDelta
	/// </summary>
	public static Element SetAnchors<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetAnchors(new Vector2(x, y));
		return element;
	}

	/// <summary> 
	/// sets the pivot point of the element,\
	/// the element will then pivot around this point
	/// </summary>
	public static Element SetPivot<Element>(this Element element, Vector2 position) where Element : HexaUIElement
	{
		element.rectTransform.pivot = position;
		return element;
	}

	/// <summary> 
	/// sets the pivot point of the element,\
	/// the element will then pivot around this point
	/// </summary>
	public static Element SetPivot<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.SetPivot(new Vector2(x, y));
		return element;
	}

	/// <summary> 
	/// this proxies TransformExtensions.ScaleWithParent\
	/// this simply makes the element as big as the parent element is
	/// </summary>
	public static Element ScaleWithParent<Element>(this Element element) where Element : HexaUIElement
	{
		element.rectTransform.ScaleWithParent();
		return element;
	}

	/// <summary> 
	/// resize the elements SizeDelta
	/// </summary>
	public static Element Resize<Element>(this Element element, Vector2 size) where Element : HexaUIElement
	{
		element.rectTransform.sizeDelta = size;
		return element;
	}


	/// <summary> 
	/// resize the elements SizeDelta,\
	/// this is a alternative method to take in 2 integers instead of a Vector2 if that's preferred
	/// </summary>
	public static Element Resize<Element>(this Element element, float x, float y) where Element : HexaUIElement
	{
		element.Resize(new Vector2(x, y));
		return element;
	}

	/// <summary> 
	/// set our name in the hierachy,\
	/// this is used for debugging with UnityExplorer as there's no runtime reason to do this
	/// </summary>
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
