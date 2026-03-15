using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Control.TextButton;
using HexaMod.API.UI.Element.Utility;
using HexaMod.API.Util.WhosYourDaddy;
using UnityEngine;
using static HexaMod.API.UI.Util.Menu;

namespace HexaMod.API.UI.Menu.HostMenu
{
	public class HostMenu : HexaUIElement
	{
		public UIElementStack teamsStack;
		public UIElementStack bottomRightStack;

		public HostMenu AddTeam(Team team)
		{
			teamsStack.AddChild(new TeamStack(team)
				.SetName(team.selectorName)
				.SetAnchors(0.5f, 0.5f)
				.SetPivot(0.5f, 0.5f)
			);

			return this;
		}

		public HostMenu AddTeams(Team[] teams)
		{
			foreach (Team team in teams)
			{
				AddTeam(team);
			}

			return this;
		}

		public HostMenu ClearTeams()
		{
			teamsStack.Clear();

			return this;
		}

		public override void Shown()
		{
			
		}

		public HostMenu() : base()
		{
			gameObject = new GameObject("hostMenu", typeof(RectTransform));
			
			teamsStack = new UIElementStack(500f)
				.SetParent(rectTransform)
				.SetName("teamsStack")
				.SetAnchors(0.5f, 1f) // top-center
				.SetPivot(0.5f, 1f)
				.Resize(0, 1080)
				.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

			bottomRightStack = new UIElementStack(5f)
				.SetParent(rectTransform)
				.SetName("bottomRightStack")
				.SetAnchors(1f, 0f)
				.SetPivot(1f, 0f)
				.SetAnchorPosition(-WTextButton.padding.x, WTextButton.padding.y)
				.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

			bottomRightStack.AddChild(new PhotonRoomAccesibilityButton()
				.SetName("accesibilityButton")
				.SetFontSize(12)
				.Resize(150, 80)
				.SetPivot(0f, 0f));

			bottomRightStack.AddChild(new WTextButton()
				.SetName("inLobbyPlayerCustomizationButton")
				.SetText("Player\nCustomization")
				.SetFontSize(12)
				.Resize(150, 80)
				.SetButtonSound(Element.Control.UISound.Back)
				.SetPivot(0f, 0f)
				.AddListener(() =>
				{
					WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("CharacterCustomizationMenu"));
				}));

			bottomRightStack.AddChild(new MatchSettingsButton()
				.SetFontSize(15)
				.Resize(150, 80)
				.AddListener(() =>
				{
					WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("MatchSettings"));
				}));

			bottomRightStack.AddChild(new WTextButton()
				.SetName("backButton")
				.SetText("Back")
				.SetFontSize(15)
				.Resize(150, 80)
				.SetButtonSound(Element.Control.UISound.Back)
				.SetPivot(0f, 0f)
				.AddListener(() =>
				{
					PhotonNetwork.LeaveRoom();
					WYDMenus.title.GoBack();
				}));

			// temp
			ClearTeams();

			AddTeam(Teams.dadTeam);
			AddTeam(Teams.babyTeam);
		}
	}
}
