using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Control;
using HexaMod.API.UI.Element.Control.ScrollList;
using HexaMod.API.UI.Element.Control.TextButton;
using HexaMod.API.UI.Element.Utility;
using UnityEngine;
using UnityEngine.UI;
using static HexaMod.API.UI.Util.Menu;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	public class PlayMenu : HexaUIElement
	{
		readonly UIElementStack bottomBarStack;
		readonly WScrollStack serverListScrollStack;

		public PlayMenu() : base()
		{
			gameObject = new GameObject("playMenu", typeof(RectTransform));

			bottomBarStack = new UIElementStack(WTextButton.padding.x)
				.SetParent(rectTransform)
				.SetName("bottomBarStack")
				.SetAnchors(0.5f, 0f)
				.SetPivot(0.5f, 0f)
				.SetAnchorPosition(0f, WTextButton.padding.y)
				.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

			bottomBarStack.AddChild(new WTextButton()
				.SetName("backButton")
				.SetTextAuto("Back")
				.SetButtonSound(UISound.Back)
				.SetPivot(0f, 0f)
				.AddListener(() =>
				{
					WYDMenus.title.GoBack();
				}));

			bottomBarStack.AddChild(new WTextButton()
				.SetName("customizeButton")
				.SetTextAuto("Player\nCustomization")
				.SetPivot(0f, 0f)
				.AddListener(() =>
				{
					WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("CharacterCustomizationMenu"));
				}));

			bottomBarStack.AddChild(new MatchSettingsButton()
				.AddListener(() =>
				{
					WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("MatchSettings"));
				}));

			bottomBarStack.AddChild(new PlayOfflineButton()
				.SetName("offlineButton")
				.SetPivot(0f, 0f));

			serverListScrollStack = new WScrollStack()
				.SetParent(rectTransform)
				.SetName("serverListStack")
				.SetAnchorMin(0.25f, 0.2f)
				.SetAnchorMax(0.75f, 0.95f)
				.SetPivot(0.5f, 0.5f)
				.SetAnchorPosition(0f, 0f)
				.SetScrollbarSize(2f / 100f)
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry())
				.AddChild(new LobbyEntry());

		}
	}
}
