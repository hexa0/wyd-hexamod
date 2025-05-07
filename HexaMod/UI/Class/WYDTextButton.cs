using HexaMod.UI.Util;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.UI.Class
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
		public Image image;

		public WYDTextButton(string name, string text, Transform menu, Vector2 position, UnityAction[] actions)
		{
			gameObject = Object.Instantiate(UITemplates.buttonTemplate.gameObject, menu);
			rectTransform = gameObject.GetComponent<RectTransform>();
			button = gameObject.GetComponent<Button>();
			image = gameObject.GetComponent<Image>();
			label = button.transform.GetChild(0).GetComponent<Text>();
			label.fontSize = text.Length > 7 ? (int)FontSizes.Small : (int)FontSizes.Regular;
			button.name = name;
			button.transform.localPosition = position;
			label.text = text;

			button.onClick = new Button.ButtonClickedEvent();
			foreach (var action in actions)
			{
				button.onClick.AddListener(action);
			}

			button.gameObject.SetActive(true);

			Init();
		}

		public static WYDTextButton MakeBackButton(MenuUtil menu, Transform root, string backMenu = null)
		{
			return new WYDTextButton(
				"backButton", "Back", root.transform,
				new Vector2(170f, 90f),
				new UnityAction[] {
					delegate ()
					{
						if (backMenu != null)
						{
							menu.menuController.ChangeToMenu(menu.GetMenuId(backMenu));
						}
						else
						{
							menu.GoBack();
						}
					}
				}
			);
		}
	}
}
