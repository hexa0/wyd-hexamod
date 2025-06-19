using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using static HexaMod.UI.Util.Menu;
using System.Collections.Generic;

namespace HexaMod.UI.Elements
{
	public class WYDSwitchOption<Type>
	{
		public string name;
		public Type value;
		public int index;
	}
	public class WYDSwitchInput<Type> : WYDUIElement
	{
		internal static GameObject boundsTemplate = HexaMod.coreBundle.LoadAsset<GameObject>("Assets/ModResources/Core/TemplateBoundingBox/switchElementBoundingBox.prefab");
		public Text label;
		public Text value;
		public Button lowerButton;
		public Button higherButton;

		internal List<UnityAction<WYDSwitchOption<Type>>> changedActions = new List<UnityAction<WYDSwitchOption<Type>>>();
		internal List<WYDSwitchOption<Type>> options = new List<WYDSwitchOption<Type>>();
		private int currentOption = 0;

		public WYDSwitchInput<Type> Select(int selection)
		{
			if (selection >= 0 && selection <= options.Count)
			{
				SetOption(selection);

				WYDSwitchOption<Type> option = options[currentOption];

				foreach (UnityAction<WYDSwitchOption<Type>> action in changedActions)
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

		public WYDSwitchInput() : base()
		{
			Transform videoOptionsMenu = Menus.title.FindMenu("VideoOptionsMenu");

			gameObject = Object.Instantiate(boundsTemplate, videoOptionsMenu);
			rectTransform = gameObject.GetComponent<RectTransform>();

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

		public WYDSwitchInput<Type> SetText(string text)
		{
			label.text = text;
			return this;
		}

		public WYDSwitchInput<Type> SetValueText(string text)
		{
			value.text = text;
			return this;
		}

		public WYDSwitchInput<Type> SetParent(Transform parent)
		{
			return this.SetParent(parent, true);
		}

		public WYDSwitchInput<Type> SetOption(int optionId)
		{
			currentOption = optionId;
			SetValueText(options[currentOption].name);
			return this;
		}

		public WYDSwitchInput<Type> LinkToPreference(Preference<int> preference)
		{
			SetOption(Mathf.Min(preference.Value, options.Count - 1))
				.AddListener((WYDSwitchOption<Type> option) => preference.Value = option.index);
			return this;
		}

		public WYDSwitchInput<Type> AddOption(WYDSwitchOption<Type> option)
		{
			option.index = options.Count;
			options.Add(option);

			SetOption(currentOption);
			return this;
		}

		public WYDSwitchInput<Type> AddOptions(WYDSwitchOption<Type>[] options)
		{
			foreach (WYDSwitchOption<Type> option in options)
			{
				AddOption(option);
			}
			return this;
		}

		public WYDSwitchInput<Type> AddListener(UnityAction<WYDSwitchOption<Type>> action)
		{
			changedActions.Add(action);
			return this;
		}

		public WYDSwitchInput<Type> AddListeners(UnityAction<WYDSwitchOption<Type>>[] actions)
		{
			foreach (UnityAction<WYDSwitchOption<Type>> action in actions)
			{
				AddListener(action);
			}

			return this;
		}

		public WYDSwitchInput(string name, string title, int defaultSelection, WYDSwitchOption<Type>[] options, Transform menu, Vector2 position, UnityAction<WYDSwitchOption<Type>>[] changedActions) : this()
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
