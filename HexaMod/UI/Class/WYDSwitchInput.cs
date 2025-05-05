using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using static HexaMod.UI.Util.Menu;

namespace HexaMod.UI.Class
{
	public class WYDSwitchOption<Type>
	{
		public string name;
		public Type value;
	}
	public class WYDSwitchInput<Type> : WYDUIElement
	{
		internal static GameObject boundsTemplate = HexaMod.coreBundle.LoadAsset<GameObject>("Assets/ModResources/Core/TemplateBoundingBox/switchElementBoundingBox.prefab");
		public Text label;
		public Text value;
		public Button lowerButton;
		public Button higherButton;

		internal UnityAction<WYDSwitchOption<Type>>[] currentChangedActions;
		public WYDSwitchOption<Type>[] currentOptions;
		public int currentOption = 0;

		public void Select(int selection)
		{
			if (selection >= 0 && selection <= currentOptions.Length)
			{
				currentOption = selection;

				WYDSwitchOption<Type> option = currentOptions[currentOption];
				value.text = option.name;

				foreach (var action in currentChangedActions)
				{
					action.Invoke(option);
				}
			}
			else
			{
				throw new System.Exception($"Selection is out of range,\ntried to select {selection} but we only have 0 to {currentOptions.Length - 1} to select from");
			}
		}

		public void SelectNext()
		{
			if (currentOption < currentOptions.Length - 1)
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

		public WYDSwitchInput(string name, string title, int defaultSelection, WYDSwitchOption<Type>[] options, Transform menu, Vector2 position, UnityAction<WYDSwitchOption<Type>>[] changedActions)
		{
			Transform videoOptionsMenu = Menus.title.FindMenu("VideoOptionsMenu");

			gameObject = Object.Instantiate(boundsTemplate, videoOptionsMenu);
			gameObject.name = "switchInputTemp";
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

			currentOptions = options;
			currentChangedActions = changedActions;

			value.text = options[defaultSelection].name;
			currentOption = defaultSelection;

			gameObject.name = name;

			higher.name = "higher";
			lower.name = "lower";
			current.name = "currentValue";
			res.name = "label";

			label.text = title;

			gameObject.transform.SetParent(menu, true);
			rectTransform.localPosition = position;

			gameObject.SetActive(true);
		}
	}
}
