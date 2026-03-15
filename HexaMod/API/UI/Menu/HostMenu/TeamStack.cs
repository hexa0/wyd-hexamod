using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Control.ScrollList;
using HexaMod.API.UI.Element.Label;
using HexaMod.API.UI.Element.Utility;
using HexaMod.API.Util.WhosYourDaddy;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Menu.HostMenu
{
	public class TeamStack : HexaUIElement
	{
		public WScrollStack playerStack;
		public WLabel teamLabel;

		public TeamStack(Team team) : base()
		{
			gameObject = new GameObject("teamStack", typeof(RectTransform));

			playerStack = new WScrollStack()
				.SetParent(rectTransform)
				.SetName("playerStack")
				.Resize(500, 0)
				.SetAnchorMin(0.5f, 0f)
				.SetAnchorMax(0.5f, 0.7f)
				.SetAnchorPosition(0f, 0f)
				.SetScrollbarSize(2f / 100f)
				.SetPivot(0.5f, 0.5f);

			teamLabel = new WLabel()
				.SetParent(playerStack.rectTransform)
				.SetName("teamLabel")
				.SetText(team.displayName)
				.SetTextFont(WUIGlobals.instance.resources.fontPrimary)
				.SetTextFontSize(70)
				.SetAnchorMin(0f, 0.8f)
				.SetAnchorMax(1f, 1f)
				.SetPivot(0.5f, 0.5f);
		}
	}
}
