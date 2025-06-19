using HexaMod.UI.Util;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using static Mono.Security.X509.X520;

namespace HexaMod.UI.Elements
{
	public class WYDTextButton : WYDUIElement
	{
		public static Vector2 gap = new Vector2(320f, 168f);
		public enum FontSizes : int
		{
			Small = 28,
			Regular = 44,
		}

		public Button button;
		public Text label;
		public Image image
		{
			get => button.image;
			set { button.image = value; }
		}

		public WYDTextButton ClearEvents()
		{
			button.onClick.RemoveAllListeners();
			button.onClick = new Button.ButtonClickedEvent();
			return this;
		}

		public WYDTextButton SetInteractable(bool interactable)
		{
			button.interactable = interactable;
			return this;
		}

		public WYDTextButton SetText(string text)
		{
			label.text = text;
			label.supportRichText = true;
			return this;
		}

		public WYDTextButton SetTextAuto(string text)
		{
			SetText(text)
				.AutoFontSize();
			return this;
		}

		public WYDTextButton SetSprite(Sprite sprite)
		{
			image.sprite = sprite;
			return this;
		}

		public WYDTextButton SetSpriteColor(Color color)
		{
			image.color = color;
			return this;
		}

		public WYDTextButton SetFontSize(int size)
		{
			label.fontSize = size;
			return this;
		}

		public WYDTextButton AutoFontSize()
		{
			SetFontSize(label.text.Length > 7 ? FontSizes.Small : FontSizes.Regular);
			return this;
		}

		public WYDTextButton SetFontSize(FontSizes size)
		{
			label.fontSize = (int)size;
			return this;
		}

		public WYDTextButton SetColors(ColorBlock colors)
		{
			ColorBlock newColors = button.colors;

			newColors.normalColor = colors.normalColor;
			newColors.pressedColor = colors.pressedColor;
			newColors.highlightedColor = colors.highlightedColor;
			newColors.disabledColor = colors.disabledColor;

			button.colors = newColors;
			return this;
		}

		public WYDTextButton AddListener(UnityAction action)
		{
			button.onClick.AddListener(action);

			return this;
		}

		public WYDTextButton AddListeners(UnityAction[] actions)
		{
			foreach (UnityAction action in actions)
			{
				AddListener(action);
			}

			return this;
		}

		public WYDTextButton(GameObject baseButton) : base()
		{
			gameObject = baseButton;
			rectTransform = gameObject.GetComponent<RectTransform>();
			button = gameObject.GetComponent<Button>();
			label = button.transform.GetChild(0).GetComponent<Text>();

			ClearEvents();
		}

		public WYDTextButton() : this(Object.Instantiate(UITemplates.buttonTemplate.gameObject)) { }

		public WYDTextButton(string name, string text, Transform menu, Vector2 position, UnityAction[] actions) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position)
				.SetTextAuto(text)
				.AddListeners(actions);
		}

		public WYDTextButton(string name, string text, Button toReplace, UnityAction[] actions) : this(toReplace.gameObject)
		{
			this.SetName(name)
				.SetParent(toReplace.transform.parent)
				.SetPosition(toReplace.transform.localPosition)
				.SetTextAuto(text)
				.AddListeners(actions);
		}

		public static WYDTextButton MakeBackButton(MenuUtil menu, Transform root, string backMenu = null)
		{
			return new WYDTextButton()
				.SetName("backButton")
				.SetTextAuto("Back")
				.SetParent(root.transform)
				.SetPosition(170f, 90f)
				.AddListener(() =>
				{
					if (backMenu != null)
					{
						menu.menuController.ChangeToMenu(menu.GetMenuId(backMenu));
					}
					else
					{
						menu.GoBack();
					}
				});
		}
	}
}
