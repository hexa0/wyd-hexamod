using static HexaMod.API.UI.Util.Menu;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	public class CreateLobbyEntry : BaseLobbyEntry
	{
		void CreateLobby()
		{
			// switch to the new create lobby menu
			WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("createLobby"));
		}

		public CreateLobbyEntry() : base()
		{
			interactButton.SetText("Create");
			LobbyName = "Create New Lobby";
			LobbyMotd = "Click the button to create a new lobby!";
			LobbyMap = "";
			MaxPlayers = 0;
			PlayerCount = 0;

			interactButton.AddListener(CreateLobby);
		}
	}
}
