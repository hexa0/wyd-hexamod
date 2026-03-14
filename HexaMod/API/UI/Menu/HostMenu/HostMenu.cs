using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Utility;
using HexaMod.API.Util.WhosYourDaddy;
using UnityEngine;

namespace HexaMod.API.UI.Menu.HostMenu
{
	public class HostMenu : HexaUIElement
	{
		public UIElementStack teamsStack;

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
			
			teamsStack = new UIElementStack(5f)
				.SetParent(rectTransform)
				.SetName("teamsStack")
				.SetAnchors(0.5f, 0.5f)
				.SetPivot(0.5f, 0.5f)
				.SetAlignment(UIElementStack.StackAlignment.LeftToRight);

			// temp
			ClearTeams();

			AddTeam(Teams.dadTeam);
			AddTeam(Teams.babyTeam);
		}
	}
}
