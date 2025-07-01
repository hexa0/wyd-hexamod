using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using HexaMod.UI.Util;
using static UnityEngine.UI.InputField;

namespace HexaMod.UI.Element.Control.TextInputField
{
	public class WTextInputField : HexaUIElement
	{
		public InputField field;
		public Text label;

		public WTextInputField SetText(string text)
		{
			label.text = text;
			return this;
		}

		public WTextInputField SetFieldText(string text)
		{
			field.text = text;
			return this;
		}

		public WTextInputField SetCharacterLimit(int characterLimit)
		{
			field.characterLimit = characterLimit;
			return this;
		}

		public WTextInputField SetContentType(ContentType type)
		{
			field.contentType = type;
			field.ForceLabelUpdate();
			return this;
		}

		public WTextInputField SetFieldTextColor(Color color)
		{
			field.textComponent.color = color;
			return this;
		}

		public WTextInputField() : base()
		{
			gameObject = Object.Instantiate(UITemplates.textInputFieldTemplate);
			rectTransform.sizeDelta = new Vector2(320f, 100f);

			field = gameObject.transform.GetChild(0).GetComponent<InputField>();
			RectTransform fieldTransform = field.GetComponent<RectTransform>();
			fieldTransform.pivot = Vector2.zero;
			fieldTransform.SetPivotPosition(0f, 0f);

			label = gameObject.transform.GetChild(1).GetComponent<Text>();
			RectTransform labelTransform = label.GetComponent<RectTransform>();
			labelTransform.pivot = Vector2.zero;
			labelTransform.sizeDelta = new Vector2(800f, 30f);
			labelTransform.SetPivotPosition(0f, 60f);

			ClearEvents();
		}

		public WTextInputField ClearEvents()
		{
			field.onValueChanged = new InputField.OnChangeEvent();
			field.onEndEdit = new InputField.SubmitEvent();
			return this;
		}

		public WTextInputField AddChangedListener(UnityAction<string> action)
		{
			field.onValueChanged.AddListener(action);

			return this;
		}

		public WTextInputField AddChangedListeners(UnityAction<string>[] actions)
		{
			foreach (UnityAction<string> action in actions)
			{
				AddChangedListener(action);
			}

			return this;
		}

		public WTextInputField AddSubmitListener(UnityAction<string> action)
		{
			field.onEndEdit.AddListener(action);

			return this;
		}

		public WTextInputField AddSubmitListeners(UnityAction<string>[] actions)
		{
			foreach (UnityAction<string> action in actions)
			{
				AddSubmitListener(action);
			}

			return this;
		}

		public WTextInputField(string name, string title, string defaultText, Transform menu, Vector2 position, UnityAction<string>[] changedActions, UnityAction<string>[] submitActions) : this()
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
