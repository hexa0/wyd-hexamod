using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using HexaMod.UI.Util;

namespace HexaMod.UI.Class
{
	public class WYDTextInputField : WYDUIElement
	{
		public InputField field;
		public Text label;
		public WYDTextInputField(string name, string title, string defaultText, Transform menu, Vector2 position, UnityAction<string>[] changedActions, UnityAction<string>[] submitActions)
		{
			gameObject = Object.Instantiate(UITemplates.textInputFieldTemplate, menu);
			rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * 1.25f);
			gameObject.name = name;
			gameObject.transform.localPosition = position;

			field = gameObject.transform.GetChild(0).GetComponent<InputField>();
			field.transform.localPosition = new Vector2(field.transform.localPosition.x, 5f);
			label = gameObject.transform.GetChild(1).GetComponent<Text>();

			field.text = defaultText;

			label.text = title;
			label.transform.localPosition = new Vector2(label.transform.localPosition.x, 70f);

			field.onValueChanged = new InputField.OnChangeEvent();
			foreach (var action in changedActions)
			{
				field.onValueChanged.AddListener(action);
			}

			field.onEndEdit = new InputField.SubmitEvent();
			foreach (var action in submitActions)
			{
				field.onEndEdit.AddListener(action);
			}

			gameObject.SetActive(true);
		}
	}
}
