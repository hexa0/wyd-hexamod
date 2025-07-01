using HexaMod.UI.Util;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Element.Control.TextButton
{
	public class WTextButton : HexaUIElement
	{
		public UISound ButtonUpSound {
			set => buttonSoundBehavior.ButtonUpSound = value;
		}

		public UISound ButtonDownSound
		{
			set => buttonSoundBehavior.ButtonDownSound = value;
		}

		public UISound HoverSound
		{
			set => buttonSoundBehavior.HoverSound = value;
		}

		private readonly ButtonSoundBehavior buttonSoundBehavior;

		public static Vector2 gap = new Vector2(320f, 168f);
		public static Vector2 padding = new Vector2(11f, 11f);
		public static Vector2 defaultSize = new Vector2(300f, 150f);

		public enum FontSizes : int
		{
			Small = 28,
			Regular = 44,
		}

		public Button button;
		public Text label;
		public Image Image
		{
			get => button.image;
			set { button.image = value; }
		}

		public WTextButton ClearEvents()
		{
			button.onClick.RemoveAllListeners();
			button.onClick = new Button.ButtonClickedEvent();
			return this;
		}

		public WTextButton SetInteractable(bool interactable)
		{
			button.interactable = interactable;
			return this;
		}

		public WTextButton SetText(string text)
		{
			label.text = text;
			label.supportRichText = true;
			return this;
		}

		public WTextButton SetTextAuto(string text)
		{
			SetText(text)
				.AutoFontSize();
			return this;
		}

		public WTextButton SetSprite(Sprite sprite)
		{
			Image.sprite = sprite;
			return this;
		}

		public WTextButton SetSpriteColor(Color color)
		{
			Image.color = color;
			return this;
		}

		public WTextButton SetFontSize(int size)
		{
			label.fontSize = size;
			return this;
		}

		public WTextButton AutoFontSize()
		{
			SetFontSize(label.text.Length > 7 ? FontSizes.Small : FontSizes.Regular);
			return this;
		}

		public WTextButton SetFontSize(FontSizes size)
		{
			label.fontSize = (int)size;
			return this;
		}

		public WTextButton SetButtonSound(UISound sound)
		{
			return SetButtonDownSound(sound).SetButtonUpSound(sound);
		}

		public WTextButton SetButtonUpSound(UISound sound)
		{
			ButtonUpSound = sound;
			return this;
		}

		public WTextButton SetButtonDownSound(UISound sound)
		{
			ButtonDownSound = sound;
			return this;
		}

		public WTextButton SetButtonHoverSound(UISound sound)
		{
			HoverSound = sound;
			return this;
		}

		public WTextButton SetColors(ColorBlock colors)
		{
			ColorBlock newColors = button.colors;

			newColors.normalColor = colors.normalColor;
			newColors.pressedColor = colors.pressedColor;
			newColors.highlightedColor = colors.highlightedColor;
			newColors.disabledColor = colors.disabledColor;

			button.colors = newColors;
			return this;
		}

		public WTextButton AddListener(UnityAction action)
		{
			button.onClick.AddListener(action);

			return this;
		}

		public WTextButton AddListeners(UnityAction[] actions)
		{
			foreach (UnityAction action in actions)
			{
				AddListener(action);
			}

			return this;
		}

		public WTextButton(GameObject baseButton) : base()
		{
			gameObject = baseButton;
			button = gameObject.GetComponent<Button>();
			label = button.transform.GetChild(0).GetComponent<Text>();

			buttonSoundBehavior = button.gameObject.GetComponent<ButtonSoundBehavior>();
			if (!buttonSoundBehavior)
			{
				buttonSoundBehavior = button.gameObject.AddComponent<ButtonSoundBehavior>();
			}

			ClearEvents();
		}

		public WTextButton() : this(Object.Instantiate(UITemplates.buttonTemplate.gameObject)) { }

		public WTextButton(string name, string text, Transform menu, Vector2 position, UnityAction[] actions) : this()
		{
			this.SetName(name)
				.SetParent(menu)
				.SetPosition(position)
				.SetTextAuto(text)
				.AddListeners(actions);
		}

		public WTextButton(string name, string text, Button toReplace, UnityAction[] actions) : this(toReplace.gameObject)
		{
			this.SetName(name)
				.SetParent(toReplace.transform.parent)
				.SetPosition(toReplace.transform.localPosition)
				.SetTextAuto(text)
				.AddListeners(actions);
		}

		public static WTextButton MakeBackButton(MenuUtil menu, Transform root, string backMenu = null)
		{
			return new WTextButton()
				.SetName("backButton")
				.SetTextAuto("Back")
				.SetButtonSound(UISound.Back)
				.SetParent(root.transform)
				.SetPosition(-960f + padding.y, -540f + padding.x)
				.SetPivot(0f, 0f)
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
