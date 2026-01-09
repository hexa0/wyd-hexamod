using static HexaMod.API.UI.Util.Menu;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	internal class JoinLobbyEntry : BaseLobbyEntry
	{
		RoomInfo room;

		public RoomInfo RoomInfo
		{
			get => room;
			set
			{
				room = value;
				interactButton.SetInteractable(room != null);

				if (room != null)
				{
					// since room names need to be unique when joining rooms, we'll use custom properties to store it as well as the other properties
					// however since that isn't implemented yet, this is commented out
					// LobbyName = room.Name;
					// LobbyMotd = room.Motd;
					// LobbyMap = room.Map;
					LobbyMotd = $"{room.Name}";
					MaxPlayers = room.MaxPlayers;
					PlayerCount = room.PlayerCount;
				}
				else
				{
					LobbyName = "null RoomInfo";
					LobbyMotd = "RoomInfo must be set";
					LobbyMap = "";
					MaxPlayers = 0;
					PlayerCount = 0;
				}
			}
		}

		void JoinLobby()
		{
			if (RoomInfo != null)
			{
				// switch to the new inside lobby menu
				WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("insideLobby"));

				PhotonNetwork.JoinRoom(RoomInfo.Name);
			}
		}

		public JoinLobbyEntry() : base()
		{
			interactButton.SetText("Join");
			RoomInfo = null;

			interactButton.AddListener(JoinLobby);
		}
	}
}
