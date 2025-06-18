using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using HexaMod.UI.Util;
using static UnityEngine.UI.InputField;

namespace HexaMod.UI.Elements
{
	public class WYDTextInputField : WYDUIElement
	{
		public InputField field;
		public Text label;

		public WYDTextInputField SetText(string text)
		{
			label.text = text;
			return this;
		}

		public WYDTextInputField SetFieldText(string text)
		{
			field.text = text;
			return this;
		}

		public WYDTextInputField SetCharacterLimit(int characterLimit)
		{
			field.characterLimit = characterLimit;
			return this;
		}

		public WYDTextInputField SetContentType(ContentType type)
		{
			field.contentType = type;
			field.ForceLabelUpdate();
			return this;
		}

		public WYDTextInputField SetFieldTextColor(Color color)
		{
			field.textComponent.color = color;
			return this;
		}

		public WYDTextInputField() : base()
		{
			gameObject = Object.Instantiate(UITemplates.textInputFieldTemplate);
			rectTransform = gameObject.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y * 1.25f);

			field = gameObject.transform.GetChild(0).GetComponent<InputField>();
			field.transform.localPosition = new Vector2(field.transform.localPosition.x, 5f);
			label = gameObject.transform.GetChild(1).GetComponent<Text>();

			label.transform.localPosition = new Vector2(label.transform.localPosition.x, 70f);

			ClearEvents();
			gameObject.SetActive(true);

			Init();
		}

		public WYDTextInputField ClearEvents()
		{
			field.onValueChanged = new InputField.OnChangeEvent();
			field.onEndEdit = new InputField.SubmitEvent();
			return this;
		}

		public WYDTextInputField AddChangedListener(UnityAction<string> action)
		{
			field.onValueChanged.AddListener(action);

			return this;
		}

		public WYDTextInputField AddChangedListeners(UnityAction<string>[] actions)
		{
			foreach (UnityAction<string> action in actions)
			{
				AddChangedListener(action);
			}

			return this;
		}

		public WYDTextInputField AddSubmitListener(UnityAction<string> action)
		{
			field.onEndEdit.AddListener(action);

			return this;
		}

		public WYDTextInputField AddSubmitListeners(UnityAction<string>[] actions)
		{
			foreach (UnityAction<string> action in actions)
			{
				AddSubmitListener(action);
			}

			return this;
		}

		public WYDTextInputField(string name, string title, string defaultText, Transform menu, Vector2 position, UnityAction<string>[] changedActions, UnityAction<string>[] submitActions) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position)
				.SetFieldText(defaultText)
				.SetText(title)
				.AddChangedListeners(changedActions)
				.AddSubmitListeners(submitActions);
		}
	}
}
