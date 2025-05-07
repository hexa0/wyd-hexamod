using UnityEngine;

namespace HexaMod.UI.Class
{
	public class WYDUIElementBehaviour : MonoBehaviour
	{
		public bool initialized = false;
		public bool shown = false;
		public WYDUIElement element;

		public void Update()
		{
			if (shown)
			{
				element.Update();
			}
		}

		public void OnEnable()
		{
			shown = true;

			if (initialized)
			{
				element.Shown();
			}
		}

		public void OnDisable()
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

		public virtual void Update()
		{

		}

		public virtual void Shown()
		{

		}

		public virtual void Hidden()
		{

		}

		public void Init()
		{
			WYDUIElementBehaviour behavior = gameObject.GetComponent<WYDUIElementBehaviour>();

			if (behavior == null) {
				behavior = gameObject.AddComponent<WYDUIElementBehaviour>();
			}

			behavior.element = this;
			behavior.initialized = true;
			if (behavior.shown)
			{
				Shown();
			}
		}
	}
}
