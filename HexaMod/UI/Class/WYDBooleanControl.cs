using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using HexaMod.UI.Util;

namespace HexaMod.UI.Class
{
	public class WYDBooleanControl : WYDUIElement
	{
		public Toggle control;
		public Text label;
		public WYDBooleanControl(string name, string text, bool active, Transform menu, Vector2 position, UnityAction<bool>[] actions)
		{
			gameObject = Object.Instantiate(UITemplates.hostControlToggleTemplate.gameObject, menu);

			control = gameObject.GetComponent<Toggle>();
			label = gameObject.GetComponentInChildren<Text>(true);
			rectTransform = gameObject.GetComponent<RectTransform>();
			gameObject.transform.Find("Background").localPosition = new Vector2(-130f, 0f);

			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - 10f);

			label.text = text;
			label.transform.localPosition = new Vector2(570f, -7f);
			label.raycastTarget = false;
			label.GetComponent<RectTransform>().sizeDelta = new Vector3(1000f, 1f);

			control.name = name;
			rectTransform.localPosition = position;
			control.onValueChanged = new Toggle.ToggleEvent();
			control.isOn = active;

			foreach (var action in actions)
			{
				control.onValueChanged.AddListener(action);
			}

			control.gameObject.SetActive(true);

			Init();
		}
	}
}
