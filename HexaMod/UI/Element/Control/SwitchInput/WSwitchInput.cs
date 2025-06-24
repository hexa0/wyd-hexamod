using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using static HexaMod.UI.Util.Menu;
using System.Collections.Generic;

namespace HexaMod.UI.Element.Control.SwitchInput
{
	public class WSwitchOption<Type>
	{
		public string name;
		public Type value;
		public int index;
	}
	public class WSwitchInput<Type> : HexaUIElement
	{
		internal static GameObject boundsTemplate = HexaGlobal.coreBundle.LoadAsset<GameObject>("Assets/ModResources/Core/TemplateBoundingBox/switchElementBoundingBox.prefab");
		public Text label;
		public Text value;
		public Button lowerButton;
		public Button higherButton;

		internal List<UnityAction<WSwitchOption<Type>>> changedActions = new List<UnityAction<WSwitchOption<Type>>>();
		internal List<WSwitchOption<Type>> options = new List<WSwitchOption<Type>>();
		private int currentOption = 0;

		public WSwitchInput<Type> Select(int selection)
		{
			if (selection >= 0 && selection <= options.Count)
			{
				SetOption(selection);

				WSwitchOption<Type> option = options[currentOption];

				foreach (UnityAction<WSwitchOption<Type>> action in changedActions)
				{
					action.Invoke(option);
				}
			}
			else
			{
				throw new System.Exception($"Selection is out of range,\ntried to select {selection} but we only have 0 to {options.Count - 1} to select from");
			}
			
			return this;
		}

		public void SelectNext()
		{
			if (currentOption < options.Count - 1)
			{
				Select(currentOption + 1);
			}
		}

		public void SelectPrevious()
		{
			if (currentOption > 0)
			{
				Select(currentOption - 1);
			}
		}

		public WSwitchInput() : base()
		{
			Transform videoOptionsMenu = WYDMenus.title.FindMenu("VideoOptionsMenu");

			gameObject = Object.Instantiate(boundsTemplate, videoOptionsMenu);

			GameObject higher = Object.Instantiate(videoOptionsMenu.Find("HigherRes").gameObject, gameObject.transform, true);
			GameObject lower = Object.Instantiate(videoOptionsMenu.Find("LowerRes").gameObject, gameObject.transform, true);
			GameObject current = Object.Instantiate(videoOptionsMenu.Find("CurrentResolution").gameObject, gameObject.transform, true);
			GameObject res = Object.Instantiate(videoOptionsMenu.Find("Res").gameObject, gameObject.transform, true);

			label = res.GetComponent<Text>();
			value = current.GetComponent<Text>();
			higherButton = higher.GetComponent<Button>();
			lowerButton = lower.GetComponent<Button>();

			higherButton.onClick = new Button.ButtonClickedEvent();
			lowerButton.onClick = new Button.ButtonClickedEvent();

			higherButton.onClick.AddListener(() =>
			{
				SelectNext();
			});

			lowerButton.onClick.AddListener(() =>
			{
				SelectPrevious();
			});

			value.alignment = TextAnchor.MiddleLeft;
			value.rectTransform.sizeDelta = new Vector2(2000f, 400f);
			value.rectTransform.localPosition = new Vector2(1125f, -4.2999f);
			label.alignment = TextAnchor.MiddleRight;
			label.rectTransform.sizeDelta = new Vector2(2000f, 400f);
			label.rectTransform.localPosition = new Vector2(-1010f, -4.2999f);

			higher.name = "higher";
			lower.name = "lower";
			current.name = "currentValue";
			res.name = "label";
		}

		public WSwitchInput<Type> SetText(string text)
		{
			label.text = text;
			return this;
		}

		public WSwitchInput<Type> SetValueText(string text)
		{
			value.text = text;
			return this;
		}

		public WSwitchInput<Type> SetParent(Transform parent)
		{
			return this.SetParent(parent, true);
		}

		public WSwitchInput<Type> SetOption(int optionId)
		{
			currentOption = optionId;
			SetValueText(options[currentOption].name);
			return this;
		}

		public WSwitchInput<Type> LinkToPreference(Preference<int> preference)
		{
			SetOption(Mathf.Min(preference.Value, options.Count - 1))
				.AddListener((WSwitchOption<Type> option) => preference.Value = option.index);
			return this;
		}

		public WSwitchInput<Type> AddOption(WSwitchOption<Type> option)
		{
			option.index = options.Count;
			options.Add(option);

			SetOption(currentOption);
			return this;
		}

		public WSwitchInput<Type> AddOptions(WSwitchOption<Type>[] options)
		{
			foreach (WSwitchOption<Type> option in options)
			{
				AddOption(option);
			}
			return this;
		}

		public WSwitchInput<Type> AddListener(UnityAction<WSwitchOption<Type>> action)
		{
			changedActions.Add(action);
			return this;
		}

		public WSwitchInput<Type> AddListeners(UnityAction<WSwitchOption<Type>>[] actions)
		{
			foreach (UnityAction<WSwitchOption<Type>> action in actions)
			{
				AddListener(action);
			}

			return this;
		}

		public WSwitchInput(string name, string title, int defaultSelection, WSwitchOption<Type>[] options, Transform menu, Vector2 position, UnityAction<WSwitchOption<Type>>[] changedActions) : this()
		{
			this.SetName(name)
				.SetParent(menu, true)
				.SetPosition(position)
				.SetText(title)
				.AddListeners(changedActions)
				.AddOptions(options)
				.SetOption(defaultSelection);

			rectTransform.localPosition = position;
		}
	}
}
