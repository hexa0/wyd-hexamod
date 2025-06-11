using System;
using System.Collections;
using HarmonyLib;
using HexaMod.SerializableObjects;
using HexaMod.UI.Util;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod
{

	public class HexaLobby : MonoBehaviour
	{
		public static class HexaLobbyState
		{
			public static ushort spawnIndex;
			public static int loadedPlayers = 0;
			public static bool handledPlayersLoaded = false;
			public static Action onPlayersLoadedAction;
		}

		public PhotonView netView;
		public float lastSettingsUpdate;

		public void Awake()
		{
			netView = GetComponent<PhotonView>();

			HexaLobbyState.loadedPlayers = 0;
			HexaLobbyState.handledPlayersLoaded = false;

			if (PhotonNetwork.inRoom)
			{
				PlayerConnectedObject player = new PlayerConnectedObject
				{
					isDad = HexaMod.networkManager.isDad
				};

				if (HexaMod.persistentLobby.dads.ContainsKey(PhotonNetwork.player.ID))
				{
					player.isDad = HexaMod.persistentLobby.dads[PhotonNetwork.player.ID];
				}

				netView.RPC("PlayerLoadedRPC", PhotonTargets.MasterClient, PlayerConnectedObject.serializer.Serialize(player));
				HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", true);
			}
		}

		public void Start()
		{
			LoadLobbyLevel();
		}

		private bool waitingForTestRoom = false;

		public void Update()
		{
			if (HexaMod.testGameWaitingForConn || waitingForTestRoom)
			{
				if (!waitingForTestRoom)
				{
					if (PhotonNetwork.connectedAndReady)
					{
						HexaMod.testGameWaitingForConn = false;
						waitingForTestRoom = true;

						RoomOptions roomOptions = new RoomOptions
						{
							IsOpen = false,
							IsVisible = false,
							MaxPlayers = 1
						};
						PhotonNetwork.CreateRoom(
							"FG_" + HexaMod.networkManager.gameName + HexaMod.networkManager.gameNum,
							roomOptions,
							PhotonNetwork.lobby
						);
					}
				}
				else
				{
					if (PhotonNetwork.room != null)
					{
						waitingForTestRoom = false;
						HexaMod.networkManager.StartMatch_FG();
					}
				}
			}
		}

		public void TryNetworkLobbySettings(LobbySettings newSettings)
		{
			if (PhotonNetwork.isMasterClient && (PhotonNetwork.inRoom || PhotonNetwork.insideLobby))
			{
				netView.RPC("SetLobbySettingsRPC", PhotonTargets.Others, new object[] { LobbySettings.serializer.Serialize(newSettings) });
			}
		}

		public void SetLobbySettings(LobbySettings newSettings, LobbySettings oldSettings)
		{
			TryNetworkLobbySettings(newSettings);
		}

		[PunRPC]
		public void PlayerLoadedRPC(byte[] playerConnectedData, PhotonMessageInfo info)
		{
			PlayerConnectedObject player = PlayerConnectedObject.serializer.Deserialize(playerConnectedData);
			HexaLobbyState.loadedPlayers++;

			Mod.Print($"got new ready player with name \"{info.sender.NickName}\" and isDad = {player.isDad} {HexaLobbyState.loadedPlayers}/{PhotonNetwork.room.PlayerCount}");

			if (HexaLobbyState.onPlayersLoadedAction != null && !HexaLobbyState.handledPlayersLoaded && HexaLobbyState.loadedPlayers == PhotonNetwork.room.PlayerCount)
			{
				Mod.Print("Got all players.");
				HexaLobbyState.onPlayersLoadedAction();
			}

			if (PhotonNetwork.room.IsOpen && info.sender != PhotonNetwork.masterClient)
			{
				var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];
				Transform hostMenu = Menu.Menus.title.FindMenu(mode.hostMenuName);
				PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

				if (playerList.daddyPlayerIds.Contains(info.sender) || playerList.babyPlayerIds.Contains(info.sender))
				{
					return;
				}

				if (player.isDad)
				{
					playerList.AddDaddy(info.sender.NickName, info.sender);
				}
				else
				{
					playerList.AddBaby(info.sender.NickName, info.sender);
				}
			}
		}

		public void WaitForPlayers(Action onPlayersLoaded, float timeoutSeconds = 5f)
		{
			HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", true);

			Mod.Print("Waiting for players.");

			HexaLobbyState.onPlayersLoadedAction = delegate ()
			{
				if (HexaLobbyState.handledPlayersLoaded)
				{
					return;
				}

				HexaLobbyState.onPlayersLoadedAction = null;
				HexaLobbyState.loadedPlayers = 0;

				Mod.Print("All players ready.");
				HexaLobbyState.handledPlayersLoaded = true;
				HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", false);
				onPlayersLoaded();
			};

			if (HexaLobbyState.loadedPlayers >= PhotonNetwork.room.PlayerCount)
			{
				Mod.Print("We already have all players.");
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
			HexaMod.persistentLobby.lobbySettings = LobbySettings.serializer.Deserialize(newSettings);
			HexaMod.persistentLobby.CommitChanges();
		}

		public void SetupMatch()
		{
			RematchHelper rematchHelper = HexaMod.rematchHelper;
			PhotonNetworkManager networkManager = HexaMod.networkManager;
			GameStateController gameStateController = HexaMod.gameStateController;
			HexaPersistentLobby lobby = HexaMod.persistentLobby;
			LobbySettings lobbySettings = lobby.lobbySettings;

			var privateRematchFields = Traverse.Create(rematchHelper);

			if (PhotonNetwork.offlineMode)
			{
				var p1isDad = privateRematchFields.Field<bool>("p1isDad");

				if (!p1isDad.Value)
				{
					networkManager.CreateOfflineGame();
				}
				else
				{
					networkManager.CreateOfflineGame2();
				}

				Destroy(rematchHelper.gameObject);
				HexaMod.rematchHelper = null;
			}

			rematchHelper.allowSpec = lobbySettings.allowSpectating && !GameModes.gameModes[rematchHelper.curGameMode].twoPlayer;
			rematchHelper.allMustDie = lobbySettings.allMustDie && !GameModes.gameModes[rematchHelper.curGameMode].twoPlayer;

			networkManager.allowSpectate = rematchHelper.allowSpec;
			gameStateController.allMustDie = rematchHelper.allMustDie;
			networkManager.curGameMode = rematchHelper.curGameMode;
			networkManager.alternateCharacters = lobbySettings.shufflePlayers == ShufflePlayersMode.Alternate;

			if (PhotonNetwork.isMasterClient)
			{
				gameStateController.GetComponent<PhotonView>().RPC("SetAllMustDie", PhotonTargets.Others, new object[] { gameStateController.allMustDie });
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

			Menu.Menus.title.menuController.DeactivateAll();
			Destroy(HexaMod.rematchHelper);

			HexaMod.networkManager.fader.SendMessage("Fade");

			if (PhotonNetwork.isMasterClient)
			{
				HexaMod.hexaLobby.enabled = true;

				HexaMod.hexaLobby.WaitForPlayers(delegate ()
				{
					HexaMod.hexaLobby.StartMatch();
				}, 5f);
			}
		}

		public void LoadLobbyLevel()
		{
			Assets.InitScene();

			if (HexaMod.rematchHelper != null && PhotonNetwork.inRoom)
			{
				SetupMatch();
			}
		}

		public void StartMatch()
		{
			if (PhotonNetwork.isMasterClient)
			{
				MatchStartObject matchStartObject = new MatchStartObject()
					.DetermineSpawns(HexaMod.persistentLobby.lobbySettings);

				netView.RPC("HexaModMatchStarted", PhotonTargets.All, new object[] { !PhotonNetwork.room.IsOpen, MatchStartObject.serializer.Serialize(matchStartObject) });

				var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];

				if (PhotonNetwork.room.IsOpen == false)
				{
					if (mode.respawnRPC != null)
					{
						HexaMod.networkManager.netView.RPC(mode.respawnRPC, PhotonTargets.All);
					}
					else
					{
						HexaMod.networkManager.RespawnPlayers();
					}
				}
			}
		}

		[PunRPC]
		public void HexaModMatchStarted(bool inGame, byte[] matchStartObjectData)
		{
			HexaMod.networkManager.fader.SendMessage("Fade");
			HexaMod.textChat.chat.CheckWho();

			HexaLobbyState.loadedPlayers = PhotonNetwork.room.PlayerCount;
			HexaLobbyState.handledPlayersLoaded = true;

			MatchStartObject matchStartObject = MatchStartObject.serializer.Deserialize(matchStartObjectData);

			HexaLobbyState.spawnIndex = 0;

			try
			{
				HexaLobbyState.spawnIndex = matchStartObject.spawns[(ushort)PhotonNetwork.player.ID];
			}
			catch (Exception e)
			{
				Mod.Warn("fail to set HexaLobbyState.spawnIndex:\n", e);
			}

			var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];

			HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", false);
			if (inGame)
			{
				HexaMod.networkManager.fader.SendMessage("Fade");
				if (mode.name == "daddysNightmare")
				{
					Countdown countdown = Menu.Menus.title.FindMenu("GameStart").Find("Countdown").gameObject.GetComponent<Countdown>();

					Instantiate(countdown.sound);
					countdown.sky.SendMessage("Switch");
					GameObject.Find("LightHolder").SendMessage("AllGoOut");
				}
			}

			if (!inGame)
			{
				var hostMenuId = Menu.Menus.title.GetMenuId(mode.hostMenuName);

				Menu.Menus.title.menuController.ChangeToMenu(hostMenuId);

				StartCoroutine(HexaModReturnedLobbyInit());
			}
		}

		IEnumerator HexaModReturnedLobbyInit()
		{
			var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];
			Transform hostMenu = Menu.Menus.title.FindMenu(mode.hostMenuName);

			yield return new WaitForEndOfFrame();

			PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

			if (PhotonNetwork.isMasterClient)
			{
				if (mode.canShuffle)
				{
					if (!HexaMod.networkManager.isDad)
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
					if (HexaMod.networkManager.isDad)
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

		void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
		{
			Mod.Print($"player \"{newPlayer.NickName}\" joined the lobby");

			if (PhotonNetwork.isMasterClient)
			{
				var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];
				Transform hostMenu = Menu.Menus.title.FindMenu(mode.hostMenuName);
				PlayerNames playerList = hostMenu.GetComponentInChildren<PlayerNames>(true);

				if (mode.defaultTeamIsDad)
				{
					playerList.AddDaddy(newPlayer.NickName, newPlayer);
				}
				else
				{
					playerList.AddBaby(newPlayer.NickName, newPlayer);
				}
			}
		}

		void OnPhotonPlayerDisconnected(PhotonPlayer oldPlayer)
		{
			Mod.Print($"player \"{oldPlayer.NickName}\" left the lobby");
		}

		[PunRPC]
		public void ReturnToLobby()
		{
			HexaMod.persistentLobby.ResetRound();
			HexaMod.persistentLobby.dads[PhotonNetwork.player.ID] = HexaMod.networkManager.isDad;
			GameObject menuCamera = GameObject.Find("BackendObjects").transform.Find("MenuCamera").gameObject;
			menuCamera.SetActive(true);
			Camera currentCamera = Camera.current;
			Camera menuCameraComponent = menuCamera.GetComponent<Camera>();
			menuCameraComponent.enabled = true;
			menuCameraComponent.fieldOfView = currentCamera.fieldOfView;
			menuCameraComponent.farClipPlane = currentCamera.farClipPlane;
			menuCameraComponent.nearClipPlane = currentCamera.nearClipPlane;
			menuCameraComponent.orthographic = currentCamera.orthographic;
			menuCamera.transform.position = currentCamera.transform.position;
			menuCamera.transform.rotation = currentCamera.transform.rotation;

			if (PhotonNetwork.isMasterClient)
			{
				PhotonNetwork.room.IsOpen = true;
				PhotonNetwork.room.IsVisible = true;
				netView.RPC("ReturnToLobby", PhotonTargets.Others);
				HexaMod.networkManager.netView.RPC("Rematch", PhotonTargets.All);
				PhotonNetwork.DestroyAll();
			}
		}
	}
}
