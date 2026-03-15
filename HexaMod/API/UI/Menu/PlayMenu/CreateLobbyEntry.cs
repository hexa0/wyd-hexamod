using System;
using HexaMod.API.Util.Unity.Settings;
using static HexaMod.API.UI.Util.Menu;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	public class CreateLobbyEntry : BaseLobbyEntry
	{
		/// <summary>
		/// generate a unique room code so that private rooms can have easy to read and write codes
		/// </summary>
		private string GenerateRoomCode(int length)
		{
			const string charset = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

			char[] code = new char[length];
			for (int i = 0; i < length; i++)
			{
				code[i] = charset[UnityEngine.Random.Range(0, charset.Length)];
			}

			return new string(code);
		}

		void CreateLobby()
		{
			// switch to the new create lobby menu
			WYDMenus.title.menuController.ChangeToMenu(WYDMenus.title.GetMenuId("hostMenu"));
			// actually make the lobby
			RoomOptions options = new RoomOptions
			{
				MaxPlayers = 8,
				IsOpen = HexaModPreferences.defaultRoomAccesibilityIsOpen.Value,

				// THIS is how private rooms are actually intended to be created, unlike what the base game does
				IsVisible = true,
				
				// although we'll use other custom properties in the actual lobby, "i" will be the packed data that needs to be displayed
				CustomRoomPropertiesForLobby = new string[] { "i" }
			};

			string code = "hm_" + GenerateRoomCode(5);
			Mod.Debug($"CreateRoom: {code}");
			PhotonNetwork.CreateRoom(code, options, PhotonNetwork.lobby);
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
