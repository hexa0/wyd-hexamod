using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Control;
using HexaMod.API.UI.Element.Control.TextButton;
using HexaMod.API.UI.Element.Label;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	public class BaseLobbyEntry : HexaUIElement
	{
		readonly LayoutElement layoutElement;

		readonly WLabel lobbyNameLabel; // top left
		readonly WLabel lobbyMotdLabel; // below name
		readonly WLabel lobbyMapLabel; // top right formatted as (playing on <map>)
		readonly WLabel lobbyPlayerCountLabel; // bottom right formatted as (<playercount>/<maxplayers>)
		internal readonly WTextButton interactButton; // right side

		// these shouldn't end up being seen normally as you'll be forced to pick a name when creating a lobby but this will be here when testing the UI without setting the values

		static readonly string[] defaultLobbyFirstName = new string[]
		{
			"Cool", "Fun", "Epic", "Silly", "Crazy", "Wild", "Mysterious", "Legendary", "Funky", "Wacky"
		};

		static readonly string[] defaultLobbyLastName = new string[]
		{
			"Gamers", "Adventurers", "Heroes", "Legends", "Warriors", "Champions", "Explorers", "Ninjas", "Pirates", "Samurai"
		};

		string lobbyName = $"{defaultLobbyFirstName[Random.Range(0, defaultLobbyFirstName.Length)]} {defaultLobbyLastName[Random.Range(0, defaultLobbyLastName.Length)]}";
		public string LobbyName
		{
			get => lobbyName;
			set
			{
				if (lobbyName == value) {
					return;
				}

				lobbyName = value;
				lobbyNameLabel.SetText(lobbyName);
			}
		}

		string lobbyMotd = "Message Of The Day!";
		public string LobbyMotd
		{
			get => lobbyMotd;
			set
			{
				if (lobbyMotd == value) {
					return;
				}
				
				lobbyMotd = value;
				lobbyMotdLabel.SetText(lobbyMotd);
			}
		}

		string lobbyMap = "Map";
		public string LobbyMap
		{
			get => lobbyMap;
			set
			{
				if (lobbyMap == value) {
					return;
				}

				lobbyMap = value;
				if (value != "")
				{
					lobbyMapLabel.SetText($"(Playing on {lobbyMap})");
				}
				else
				{
					lobbyMapLabel.SetText("");
				}
			}
		}

		void UpdatePlayerCountLabel()
		{
			if (MaxPlayers > 0)
			{
				lobbyPlayerCountLabel.SetText($"({playerCount}/{maxPlayers})");
			}
			else
			{
				lobbyPlayerCountLabel.SetText("");
			}
		}

		int playerCount = 0;

		public int PlayerCount
		{
			get => playerCount;
			set
			{
				if (playerCount == value) {
					return;
				}

				playerCount = value;
				UpdatePlayerCountLabel();
			}
		}

		int maxPlayers = 8;

		public int MaxPlayers
		{
			get => maxPlayers;
			set
			{
				if (maxPlayers == value) {
					return;
				}

				maxPlayers = value;
				UpdatePlayerCountLabel();
			}
		}

		public BaseLobbyEntry() : base()
		{
			gameObject = new GameObject("lobbyEntry", typeof(RectTransform));
			rectTransform.sizeDelta = new Vector2(0f, 100f);
			rectTransform.anchorMin = new Vector2(0f, 0f);
			rectTransform.anchorMax = new Vector2(1f, 0f);

			layoutElement = gameObject.AddComponent<LayoutElement>();
			layoutElement.preferredHeight = rectTransform.sizeDelta.y;

			// background
			gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

			lobbyNameLabel = new WLabel()
				.SetParent(rectTransform)
				.SetName("lobbyNameLabel")
				.SetAnchorMin(0f, 0f)
				.SetAnchorMax(1f, 1f)
				.SetPivot(0f, 1f)
				.SetAnchorPosition(10f, -10f)
				.SetText(lobbyName)
				.SetRichTextEnabled(true)
				.SetTextFontSize(24)
				.SetTextAligment(TextAnchor.UpperLeft);

			lobbyMotdLabel = new WLabel()
				.SetParent(rectTransform)
				.SetName("lobbyMotdLabel")
				.SetAnchorMin(0f, 0f)
				.SetAnchorMax(0.5f, 0.5f)
				.SetPivot(0f, 1f)
				.SetAnchorPosition(10f, 0f)
				.SetText(lobbyMotd)
				.SetRichTextEnabled(true)
				.SetTextFontSize(14)
				.SetTextColor(Color.gray)
				.SetTextAligment(TextAnchor.UpperLeft);

			lobbyMapLabel = new WLabel()
				.SetParent(rectTransform)
				.SetName("lobbyMapLabel")
				.SetAnchorMin(0f, 1f)
				.SetAnchorMax(1f, 1f)
				.SetPivot(1f, 1f)
				.SetAnchorPosition(-10f, -10f)
				.SetText($"(Playing on {lobbyMap})")
				.SetRichTextEnabled(true)
				.SetTextFontSize(14)
				.SetTextAligment(TextAnchor.UpperRight);

			lobbyPlayerCountLabel = new WLabel()
				.SetParent(rectTransform)
				.SetName("lobbyPlayerCountLabel")
				.SetAnchorMin(0f, 0f)
				.SetAnchorMax(1f, 0f)
				.SetPivot(1f, 0f)
				.SetAnchorPosition(-10f, 10f)
				.SetText($"({playerCount}/{maxPlayers})")
				.SetRichTextEnabled(false)
				.SetTextFontSize(14)
				.SetTextAligment(TextAnchor.LowerRight);

			interactButton = new WTextButton()
				.SetParent(rectTransform)
				.SetName("interactButton")
				.SetAnchorMin(1f, 0.5f)
				.SetAnchorMax(1f, 0.5f)
				.SetPivot(1f, 0.5f)
				.SetAnchorPosition(-10f, 0f)
				.Resize(100f, 30f)
				.SetTextAuto("Action")
				.SetFontSize(18)
				.SetButtonDownSound(UISound.None)
				.SetButtonUpSound(UISound.Yes);

		}
	}
}
