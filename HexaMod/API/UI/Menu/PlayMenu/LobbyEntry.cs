using HexaMod.API.UI.Element;
using HexaMod.API.UI.Element.Label;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.API.UI.Menu.PlayMenu
{
	public class LobbyEntry : HexaUIElement
	{
		readonly LayoutElement layoutElement;

		readonly WLabel lobbyNameLabel; // top left
		readonly WLabel lobbyMotdLabel; // below name
		readonly WLabel lobbyMapLabel; // top right formatted as (playing on <map>)
		readonly WLabel lobbyPlayerCountLabel; // bottom right formatted as (<playercount>/<maxplayers>)

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
				lobbyMap = value;
				lobbyMapLabel.SetText($"(Playing on {lobbyMap})");
			}
		}

		int playerCount = 0;

		public int PlayerCount
		{
			get => playerCount;
			set
			{
				playerCount = value;
				lobbyPlayerCountLabel.SetText($"({playerCount}/{maxPlayers})");
			}
		}

		int maxPlayers = 8;

		public int MaxPlayers
		{
			get => maxPlayers;
			set
			{
				maxPlayers = value;
				lobbyPlayerCountLabel.SetText($"({playerCount}/{maxPlayers})");
			}
		}

		public LobbyEntry() : base()
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

		}
	}
}
