using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using HexaMod.API.UI;
using HexaMod.API.Util.WhosYourDaddy;
using HexaMod.Scripts.Multiplayer.SerializableObjects;
using HexaMod.Scripts.Persistent;
using UnityEngine;
using static HexaMod.API.UI.Util.Menu;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace HexaMod.Scripts.Multiplayer.Lobby
{

	public class HexaLobby : MonoBehaviour
	{
		public static class HexaLobbyState
		{
			public static List<int> loadedPlayers = new List<int>();
			public static bool handledPlayersLoaded = false;
			public static Action onPlayersLoadedAction;
		}

		public PhotonView netView;
		public float lastSettingsUpdate;

		public static string GetPlayerName(PhotonPlayer player, string fallback = null)
		{
			string nickname = player.NickName;

			if (nickname == string.Empty)
			{
				if (fallback == null)
				{
					return $"unknown player: {player.ID}";
				}
				else
				{
					return fallback;
				}
			}
			else
			{
				return nickname;
			}
		}
		
		public void OnConnectedToPhoton()
		{
			PhotonNetwork.sendRate = 30;
			PhotonNetwork.sendRateOnSerialize = 30;
		}

		public void SendReadyToMasterClient()
		{
			if (!PhotonNetwork.inRoom) { return; }

			PlayerConnectedObject player = new PlayerConnectedObject
			{
				isDad = HexaGlobal.networkManager.isDad
			};

			if (HexaPersistentLobby.instance.dads.ContainsKey(PhotonNetwork.player.ID))
			{
				player.isDad = HexaPersistentLobby.instance.dads[PhotonNetwork.player.ID];
			}

			Hashtable hash = PhotonNetwork.player.CustomProperties;

			if (hash.ContainsKey("Team"))
			{
				hash.Remove("Team");
			}

			hash.Add("Team", player.isDad ? "D" : "B");

			PhotonNetwork.player.SetCustomProperties(hash);

			netView.RPC("PlayerLoadedRPC", PhotonTargets.MasterClient, PlayerConnectedObject.serializer.Serialize(player));

			if (PhotonNetwork.isMasterClient)
			{
				netView.RPC("MasterReadyRPC", PhotonTargets.Others);
			}
		}

		public void Awake()
		{
			netView = GetComponent<PhotonView>();

			HexaLobbyState.loadedPlayers.Clear();
			HexaLobbyState.handledPlayersLoaded = false;

			if (PhotonNetwork.inRoom)
			{
				HexaMenus.startupScreen.loadingText.SetText("Waiting for others to load");
			}
		}

		public void Start()
		{
			LoadLobbyLevel();
		}

		public void TryNetworkLobbySettings(LobbySettings newSettings)
		{
			if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom)
			{
				netView.RPC("SetLobbySettingsRPC", PhotonTargets.Others, LobbySettings.serializer.Serialize(newSettings));
			}
		}

		public void SetLobbySettings(LobbySettings newSettings)
		{
			TryNetworkLobbySettings(newSettings);
		}

		[PunRPC]
		public void MasterReadyRPC()
		{
			if (!PhotonNetwork.isMasterClient && !HexaLobbyState.handledPlayersLoaded)
			{
				SendReadyToMasterClient();
			}
		}

		[PunRPC]
		public void PlayerLoadedRPC(byte[] playerConnectedData, PhotonMessageInfo info)
		{
			if (HexaLobbyState.handledPlayersLoaded) { return; }
			if (HexaLobbyState.loadedPlayers.Contains(info.sender.ID)) { return; }

			PlayerConnectedObject player = PlayerConnectedObject.serializer.Deserialize(playerConnectedData);
			HexaLobbyState.loadedPlayers.Add(info.sender.ID);

			if (HexaLobbyState.onPlayersLoadedAction != null && !HexaLobbyState.handledPlayersLoaded && HexaLobbyState.loadedPlayers.Count == PhotonNetwork.room.PlayerCount)
			{
				HexaLobbyState.onPlayersLoadedAction();
			}

			if (PhotonNetwork.room.IsOpen && info.sender != PhotonNetwork.masterClient)
			{
				var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];
				Transform hostMenu = WYDMenus.title.FindMenu(mode.hostMenuName);
				PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

				if (player.isDad)
				{
					playerList.AddDaddy(GetPlayerName(info.sender), info.sender);
				}
				else
				{
					playerList.AddBaby(GetPlayerName(info.sender), info.sender);
				}
			}
		}

		public void WaitForPlayers(Action onPlayersLoaded, float timeoutSeconds = 5f)
		{
			HexaMenus.startupScreen.loadingText.SetText("Waiting for others to load");

			HexaLobbyState.onPlayersLoadedAction = delegate ()
			{
				if (HexaLobbyState.handledPlayersLoaded)
				{
					return;
				}

				HexaLobbyState.onPlayersLoadedAction = null;
				HexaLobbyState.loadedPlayers.Clear();

				HexaLobbyState.handledPlayersLoaded = true;
				HexaMenus.startupScreen.loadingText.SetText("Loaded");
				HexaMenus.startupScreen.fader.fadeState = false;
				onPlayersLoaded();
			};

			if (HexaLobbyState.loadedPlayers.Count >= PhotonNetwork.room.PlayerCount)
			{
				HexaLobbyState.onPlayersLoadedAction();
			}
			else
			{
				StartCoroutine(WaitForPlayersRoutine(timeoutSeconds));
			}
		}

		public IEnumerator WaitForPlayersRoutine(float timeoutSeconds)
		{
			yield return new WaitForSeconds(timeoutSeconds);

			if (!HexaLobbyState.handledPlayersLoaded)
			{
				Mod.Warn("WaitForPlayers TIMED OUT!");
				HexaLobbyState.onPlayersLoadedAction();
			}
		}

		[PunRPC]
		public void SetLobbySettingsRPC(byte[] newSettings)
		{
			HexaPersistentLobby.instance.lobbySettings = LobbySettings.serializer.Deserialize(newSettings);
			HexaPersistentLobby.instance.CommitChanges();
		}

		public void SetupMatch()
		{
			RematchHelper rematchHelper = HexaGlobal.rematchHelper;
			PhotonNetworkManager networkManager = HexaGlobal.networkManager;
			GameStateController gameStateController = HexaGlobal.gameStateController;
			HexaPersistentLobby lobby = HexaPersistentLobby.instance;
			LobbySettings lobbySettings = lobby.lobbySettings;


			rematchHelper.allowSpec = lobbySettings.allowSpectating && !GameModes.gameModes[rematchHelper.curGameMode].twoPlayer;
			rematchHelper.allMustDie = lobbySettings.allMustDie && !GameModes.gameModes[rematchHelper.curGameMode].twoPlayer;

			networkManager.allowSpectate = rematchHelper.allowSpec;
			gameStateController.allMustDie = rematchHelper.allMustDie;
			networkManager.curGameMode = rematchHelper.curGameMode;
			networkManager.alternateCharacters = lobbySettings.shufflePlayers == ShufflePlayersMode.Alternate;

			if (PhotonNetwork.isMasterClient)
			{
				gameStateController.GetComponent<PhotonView>().RPC("SetAllMustDie", PhotonTargets.Others, gameStateController.allMustDie);
			}

			if (!lobby.dads.ContainsKey(PhotonNetwork.player.ID))
			{
				lobby.dads[PhotonNetwork.player.ID] = rematchHelper.isDadStart;
			}

			GameMode gameMode = GameModes.gameModes[rematchHelper.curGameMode];

			if (lobbySettings.shufflePlayers == ShufflePlayersMode.Shuffle && gameMode.canShuffle)
			{
				// TODO: implement shuffle
				networkManager.isDad = lobby.dads[PhotonNetwork.player.ID];
			}
			else
			{
				if (gameMode.canShuffle && lobbySettings.shufflePlayers == ShufflePlayersMode.Alternate)
				{
					if (lobbySettings.roundNumber % 2 == 0)
					{
						networkManager.isDad = !lobby.dads[PhotonNetwork.player.ID];
					}
					else
					{
						networkManager.isDad = lobby.dads[PhotonNetwork.player.ID];
					}
				}
				else
				{
					networkManager.isDad = lobby.dads[PhotonNetwork.player.ID];
				}
			}

			WYDMenus.title.menuController.DeactivateAll();
			HexaGlobal.networkManager.fader.SendMessage("Fade");

			if (PhotonNetwork.isMasterClient)
			{
				HexaGlobal.hexaLobby.enabled = true;

				HexaGlobal.hexaLobby.WaitForPlayers(delegate ()
				{
					HexaGlobal.hexaLobby.StartMatch();
					Destroy(HexaGlobal.rematchHelper);
				}, 5f);
			}
		}

		public void LoadLobbyLevel()
		{
			Assets.InitScene();

			if (HexaGlobal.rematchHelper != null && PhotonNetwork.inRoom)
			{
				SetupMatch();
			}
		}

		public void StartMatch()
		{
			if (PhotonNetwork.isMasterClient)
			{
				netView.RPC("HexaModMatchStarted", PhotonTargets.All, !PhotonNetwork.room.IsOpen);

				if (PhotonNetwork.room.IsOpen == false)
				{
					if (SplitscreenUtil.IsInSplitscreen())
					{
						var privateRematchFields = Traverse.Create(HexaGlobal.rematchHelper);
						var p1isDad = privateRematchFields.Field<bool>("p1isDad");

						if (!p1isDad.Value)
						{
							HexaGlobal.networkManager.CreateOfflineGame();
						}
						else
						{
							HexaGlobal.networkManager.CreateOfflineGame2();
						}

						Destroy(HexaGlobal.rematchHelper.gameObject);
						HexaGlobal.rematchHelper = null;
					}
					else
					{
						netView.RPC("SpawnPlayers", PhotonTargets.AllBuffered);
					}
				}
			}
		}

		[PunRPC]
		public void HexaModMatchStarted(bool inGame)
		{
			HexaGlobal.networkManager.fader.SendMessage("Fade");
			HexaGlobal.textChat.chat.CheckWho();

			HexaLobbyState.handledPlayersLoaded = true;

			var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];

			HexaMenus.startupScreen.loadingText.SetText("Loaded");
			HexaMenus.startupScreen.fader.fadeState = false;

			if (Assets.clearDefaultLevelObjectsOnReady)
			{
				StartCoroutine(FullyDestroyDefaultLevelAssets());
			}

			if (inGame)
			{
				HexaGlobal.networkManager.fader.SendMessage("Fade");

				if (mode.name == "daddysNightmare")
				{
					Countdown countdown = WYDMenus.title.FindMenu("GameStart").Find("Countdown").gameObject.GetComponent<Countdown>();

					Instantiate(countdown.sound);
					countdown.sky.SendMessage("Switch");
					GameObject.Find("LightHolder").SendMessage("AllGoOut");
				}
			}

			if (!inGame)
			{
				var hostMenuId = WYDMenus.title.GetMenuId(mode.hostMenuName);

				WYDMenus.title.menuController.ChangeToMenu(hostMenuId);

				StartCoroutine(HexaModReturnedLobbyInit());
			}
		}

		IEnumerator FullyDestroyDefaultLevelAssets()
		{
			yield return new WaitForEndOfFrame();

			foreach (var levelObject in Assets.defaultLevelObjects)
			{
				Destroy(levelObject);
			}
		}

		IEnumerator HexaModReturnedLobbyInit()
		{
			var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];
			Transform hostMenu = WYDMenus.title.FindMenu(mode.hostMenuName);

			yield return new WaitForEndOfFrame();

			PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

			if (PhotonNetwork.isMasterClient)
			{
				if (mode.canShuffle)
				{
					if (!HexaGlobal.networkManager.isDad)
					{
						playerList.AddDaddy(PhotonNetwork.playerName, PhotonNetwork.player);
					}
					else
					{
						playerList.AddBaby(PhotonNetwork.playerName, PhotonNetwork.player);
					}
				}
				else
				{
					if (HexaGlobal.networkManager.isDad)
					{
						playerList.AddDaddy(PhotonNetwork.playerName, PhotonNetwork.player);
					}
					else
					{
						playerList.AddBaby(PhotonNetwork.playerName, PhotonNetwork.player);
					}
				}
			}
		}

		void OnJoinedRoom()
		{
			HexaLobbyState.handledPlayersLoaded = true;
		}

		void OnMasterClientSwitched(PhotonPlayer player)
		{
			Mod.Print($"master client switched to peer {player.ID}");

			if (player == PhotonNetwork.player)
			{
				HexaPersistentLobby.instance.SetInOtherLobby(false);
			}
		}

		void OnPhotonPlayerConnected(PhotonPlayer player)
		{
			Mod.Print($"player \"{GetPlayerName(player)}\" joined the lobby");

			if (PhotonNetwork.isMasterClient)
			{
				HexaGlobal.hexaLobby.TryNetworkLobbySettings(HexaPersistentLobby.instance.lobbySettings);

				var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];
				Transform hostMenu = WYDMenus.title.FindMenu(mode.hostMenuName);
				PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

				if (mode.defaultTeamIsDad)
				{
					playerList.AddDaddy(GetPlayerName(player), player);
				}
				else
				{
					playerList.AddBaby(GetPlayerName(player), player);
				}

				HexaGlobal.textChat.SendUnformattedChatMessage($"<color=lime>►</color> <b><color=\"#ed6553\">{GetPlayerName(player)}</color></b> joined.");
			}
		}

		IEnumerator OnPhotonPlayerDisconnected(PhotonPlayer player)
		{
			Mod.Print($"player \"{GetPlayerName(player)}\" left the lobby");
			
			if (PhotonNetwork.isMasterClient)
			{
				// player left/all players left chat messages

				HexaGlobal.textChat.SendUnformattedChatMessage($"<color=red>◄</color> <b><color=\"#ed6553\">{GetPlayerName(player)}</color></b> left.");

				if (!WYDMenus.title.menuController.menus[WYDMenus.title.menuController.curMenu].activeInHierarchy && PhotonNetwork.playerList.Length <= 1)
				{
					HexaGlobal.textChat.SendServerMessage("All players have left the game.");
				}

				// lobby player list

				var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];
				Transform hostMenu = WYDMenus.title.FindMenu(mode.hostMenuName);
				PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

				for (int i = 0; i < playerList.daddyPlayerIds.Count; i++)
				{
					if (playerList.daddyPlayerIds[i] == player)
					{
						playerList.daddyPlayerIds.RemoveAt(i);
						playerList.daddyPlayerNames.RemoveAt(i);
					}
				}

				for (int i = 0; i < playerList.babyPlayerIds.Count; i++)
				{
					if (playerList.babyPlayerIds[i] == player)
					{
						playerList.babyPlayerIds.RemoveAt(i);
						playerList.babyPlayerNames.RemoveAt(i);
					}
				}

				playerList.GetComponent<PhotonView>().RPC("SendPlayerLists", PhotonTargets.All, playerList.daddyPlayerNames.ToArray(), playerList.daddyPlayerIds.ToArray(), playerList.babyPlayerNames.ToArray(), playerList.babyPlayerIds.ToArray());

				// abandoned screen

				if (GameModes.gameModes[HexaGlobal.networkManager.curGameMode].twoPlayer)
				{
					GameObject BabyCam = GameObject.Find("BabyCam");
					GameObject DadCam = GameObject.Find("DadCam");
					HexaGlobal.gameStateController.DisableInGameUI();

					if (BabyCam)
					{
						BabyCam.SendMessage("TurnOffPlayer");
						BabyCam.GetComponent<NetworkMovement>().enabled = false;
						BabyCam.transform.parent.GetComponent<NetworkMovement>().enabled = false;
						BabyCam.SendMessage("ActivateWinCam");

						if (!HexaGlobal.networkManager.isDad)
						{
							WYDMenus.inGame.menuController.ChangeToMenu(3);
						}
					}

					if (DadCam)
					{
						DadCam.SendMessage("TurnOffPlayer");
						DadCam.GetComponent<NetworkMovement>().enabled = false;
						DadCam.transform.parent.GetComponent<NetworkMovement>().enabled = false;
						DadCam.SendMessage("ActivateWinCam");

						if (HexaGlobal.networkManager.isDad)
						{
							WYDMenus.inGame.menuController.ChangeToMenu(4);
						}
					}

					if (!BabyCam && !DadCam)
					{
						yield return new WaitForSeconds(3f);
						HexaGlobal.networkManager.SomeoneDisconnected();
					}
				}
			}
		}

		[PunRPC]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0031:Use null propagation", Justification = "because it doesn't work dumbass, it throws errors")]
		public void SpawnPlayers()
		{
			GameMode gameMode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];

			HexaGlobal.networkManager.gameStarted = true;

			WYDMenus.title.menuController.CloseMenu();
			HexaGlobal.networkManager.fader.SendMessage("Fade");

			List<List<PhotonPlayer>> playerTeams = new List<List<PhotonPlayer>>
			{
				new List<PhotonPlayer>(),
				new List<PhotonPlayer>()
			};

			foreach (PhotonPlayer player in PhotonNetwork.playerList)
			{
				Hashtable hash = player.CustomProperties;
				hash.TryGetValue("Team", out object teamValue);
				string team = (string)teamValue;
				int teamId = gameMode.teams.GetTeamId(team);
				playerTeams[teamId].Add(player);
			}

			for (int teamId = 0; teamId < playerTeams.Count; teamId++)
			{
				List<PhotonPlayer> teamList = playerTeams[teamId];

				for (int teamPlayerId = 0; teamPlayerId < teamList.Count; teamPlayerId++)
				{
					PhotonPlayer player = teamList[teamPlayerId];

					if (player == PhotonNetwork.player)
					{
						Transform initialSpawn = Assets.GetSpawnTransform(teamPlayerId);
						Team teamInfo = gameMode.teams.GetTeamFromId(teamId);
						IntroMessage introMessage = gameMode.GetIntroMessage(teamId);

						GameObject playerObject = PhotonNetwork.Instantiate(teamInfo.prefabName, initialSpawn.position, initialSpawn.rotation, 0);

						Transform finalSpawn = Assets.GetSpawnTransform(teamPlayerId);
						playerObject.transform.SetPositionAndRotation(finalSpawn.position, finalSpawn.rotation);

						ActionText titleActionText = GameObject.Find("CenterBig")?.GetComponent<ActionText>();
						ActionText bodyActionText = GameObject.Find("CenterSmall")?.GetComponent<ActionText>();

						if (titleActionText && bodyActionText)
						{
							titleActionText.ActionDone(introMessage.title);
							bodyActionText.ActionDone(introMessage.body);
						}
					}
				}
			}

			HexaGlobal.networkManager.itemSpawner.SendMessage("NetworkSpawnObjects");
			if (HexaGlobal.networkManager.petsObj != null)
			{
				HexaGlobal.networkManager.petsObj.SetActive(gameMode.petsAllowed);
			}
			HexaGlobal.gameStateController.StartClocks();
		}

		[PunRPC]
		public void ReturnToLobby()
		{
			if (PhotonNetwork.isMasterClient)
			{
				HexaPersistentLobby.instance.ResetRound();
				HexaPersistentLobby.instance.dads[PhotonNetwork.player.ID] = HexaGlobal.networkManager.isDad;
			}

			Camera currentCamera = Camera.current;

			GameObject menuCamera = GameObject.Find("BackendObjects").Find("MenuCamera");

			Camera menuCameraComponent = menuCamera.GetComponent<Camera>();
			menuCameraComponent.enabled = true;
			menuCameraComponent.fieldOfView = currentCamera.fieldOfView;
			menuCameraComponent.farClipPlane = currentCamera.farClipPlane;
			menuCameraComponent.nearClipPlane = currentCamera.nearClipPlane;
			menuCameraComponent.orthographic = currentCamera.orthographic;
			menuCamera.transform.position = currentCamera.transform.position;
			menuCamera.transform.rotation = currentCamera.transform.rotation;

			menuCamera.SetActive(true);

			if (PhotonNetwork.isMasterClient)
			{
				PhotonNetwork.room.IsOpen = true;
				PhotonNetwork.room.IsVisible = true;
				netView.RPC("ReturnToLobby", PhotonTargets.Others);
				HexaGlobal.networkManager.netView.RPC("Rematch", PhotonTargets.All);
				PhotonNetwork.DestroyAll();
			}
		}
	}
}
