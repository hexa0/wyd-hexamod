using System.Xml.Linq;
using UnityEngine;

namespace HexaMod.UI.Elements
{
	public class WYDUIElementBehaviour : MonoBehaviour
	{
		bool initialized = false;
		bool shown = false;
		WYDUIElement element;

		public void Initialize(WYDUIElement linkedElement)
		{
			element = linkedElement;
			initialized = true;

			if (shown)
			{
				element.Shown();
			}
		}

		void Update()
		{
			if (shown)
			{
				element.Update();
			}
		}

		void OnEnable()
		{
			shown = true;

			if (initialized)
			{
				element.Shown();
			}
		}

		void OnDisable()
		{
			shown = false;

			if (initialized)
			{
				element.Hidden();
			}
		}
	}

	public class WYDUIElement
	{
		public GameObject gameObject;
		public RectTransform rectTransform;

		public WYDUIElement()
		{

		}

		public virtual void Init()
		{
			WYDUIElementBehaviour behavior = gameObject.GetComponent<WYDUIElementBehaviour>();

			if (!behavior)
			{
				gameObject.SetActive(true);
				behavior = gameObject.AddComponent<WYDUIElementBehaviour>();
				behavior.Initialize(this);
			}
		}

		public virtual void Update()
		{

		}

		public virtual void Shown()
		{

		}

		public virtual void Hidden()
		{

		}
	}

	static class WYDUIElementGenericMethodExtensions
	{
		public static Element SetParent<Element>(this Element element, Transform menu) where Element : WYDUIElement
		{
			element.gameObject.transform.SetParent(menu, false);
			element.Init();
			return element;
		}

		public static Element SetParent<Element>(this Element element, Transform menu, bool worldPositionStays) where Element : WYDUIElement
		{
			element.gameObject.transform.SetParent(menu, worldPositionStays);
			return element;
		}

		public static Element SetPosition<Element>(this Element element, Vector2 position) where Element : WYDUIElement
		{
			element.gameObject.transform.localPosition = position;
			return element;
		}

		public static Element SetPosition<Element>(this Element element, float x, float y) where Element : WYDUIElement
		{
			element.SetPosition(new Vector2(x, y));
			return element;
		}

		public static Element SetName<Element>(this Element element, string name) where Element : WYDUIElement
		{
			element.gameObject.name = name;
			return element;
		}
	}
}
