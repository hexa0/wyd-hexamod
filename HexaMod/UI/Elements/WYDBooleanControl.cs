using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using HexaMod.UI.Util;
namespace HexaMod.UI.Elements
{
	public class WYDBooleanControl : WYDUIElement
	{
		public Toggle control;
		public Text label;

		public WYDBooleanControl SetState(bool state)
		{
			control.isOn = state;
			return this;
		}

		public WYDBooleanControl LinkToPreference(string preference)
		{
			SetState(PlayerPrefs.GetInt(preference, 1) == 1)
				.AddListener((bool value) =>
				{
					PlayerPrefs.SetInt(preference, value ? 1 : 0);
				});
			return this;
		}

		public WYDBooleanControl SetText(string text)
		{
			label.text = text;
			return this;
		}

		public WYDBooleanControl AddListener(UnityAction<bool> action)
		{
			control.onValueChanged.AddListener(action);
			return this;
		}

		public WYDBooleanControl AddListeners(UnityAction<bool>[] actions)
		{
			foreach (UnityAction<bool> action in actions)
			{
				AddListener(action);
			}
			return this;
		}

		public WYDBooleanControl ClearEvents()
		{
			control.onValueChanged = new Toggle.ToggleEvent();
			return this;
		}

		public WYDBooleanControl() : base()
		{
			gameObject = Object.Instantiate(UITemplates.hostControlToggleTemplate.gameObject);

			control = gameObject.GetComponent<Toggle>();
			label = gameObject.GetComponentInChildren<Text>(true);
			rectTransform = gameObject.GetComponent<RectTransform>();
			gameObject.Find("Background").transform.localPosition = new Vector2(-130f, 0f);

			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - 10f);

			label.transform.localPosition = new Vector2(570f, -7f);
			label.raycastTarget = false;
			label.GetComponent<RectTransform>().sizeDelta = new Vector3(1000f, 1f);

			ClearEvents();

			control.gameObject.SetActive(true);

			Init();
		}

		public WYDBooleanControl(string name, string text, bool active, Transform menu, Vector2 position, UnityAction<bool>[] actions) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position)
				.SetText(text)
				.SetState(active)
				.AddListeners(actions);
		}
	}
}
