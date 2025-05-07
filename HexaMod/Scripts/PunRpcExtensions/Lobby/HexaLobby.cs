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
				netView.RPC("PlayerLoadedRPC", PhotonTargets.MasterClient, new object[] { });
				HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", true);
			}

			//StartCoroutine(LoadLobbyLevel());
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

						RoomOptions roomOptions = new RoomOptions();
						roomOptions.IsOpen = false;
						roomOptions.IsVisible = false;
						roomOptions.MaxPlayers = 1;
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
		public void PlayerLoadedRPC()
		{
			HexaLobbyState.loadedPlayers++;

			Mod.Print($"got new connected player {HexaLobbyState.loadedPlayers}/{PhotonNetwork.room.PlayerCount}");

			if (HexaLobbyState.onPlayersLoadedAction != null && !HexaLobbyState.handledPlayersLoaded && HexaLobbyState.loadedPlayers == PhotonNetwork.room.PlayerCount)
			{
				Mod.Print("Got all players.");
				HexaLobbyState.onPlayersLoadedAction();
			}
		}

		public void WaitForPlayers(Action onPlayersLoaded, float timeoutSeconds = 5f)
		{
			HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", true);

			Mod.Print("Waiting for players.");

			HexaLobbyState.onPlayersLoadedAction = delegate()
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
			}

			gameStateController.allMustDie = lobbySettings.allMustDie && !GameModes.gameModes[rematchHelper.curGameMode].twoPlayer;
			networkManager.curGameMode = rematchHelper.curGameMode;
			networkManager.alternateCharacters = lobby.lobbySettings.shufflePlayers == ShufflePlayersMode.Alternate;
			networkManager.allowSpectate = rematchHelper.allowSpec;

			if (PhotonNetwork.isMasterClient)
			{
				gameStateController.GetComponent<PhotonView>().RPC("SetAllMustDie", PhotonTargets.Others, new object[] { gameStateController.allMustDie });
			}

			if (!lobby.dads.ContainsKey(PhotonNetwork.player.ID))
			{
				lobby.dads[PhotonNetwork.player.ID] = rematchHelper.isDadStart;
			}

			GameMode gameMode = GameModes.gameModes[rematchHelper.curGameMode];

			if (lobby.lobbySettings.shufflePlayers == ShufflePlayersMode.Shuffle && gameMode.canShuffle)
			{
				networkManager.isDad = lobby.dads[PhotonNetwork.player.ID];
			}
			else
			{
				if (gameMode.canShuffle && networkManager.alternateCharacters)
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

				Menu.Menus.title.menuController.DeactivateAll();
			}

			Destroy(HexaMod.rematchHelper);

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
			// yield return 0;

			Assets.InitScene();

			if (HexaMod.rematchHelper != null && PhotonNetwork.inRoom) {
				SetupMatch();
			}
		}

		public void StartMatch()
		{
			netView.RPC("HexaModMatchStarted", PhotonTargets.All, new object[] { });

			if (PhotonNetwork.isMasterClient)
			{
				var mode = GameModes.gameModes[HexaMod.networkManager.curGameMode];

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

		[PunRPC]
		public void HexaModMatchStarted()
		{
			HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", false);
			HexaMod.networkManager.fader.SendMessage("Fade");
			if (HexaMod.networkManager.curGameMode == GameModes.named["daddysNightmare"].id)
			{
				Countdown countdown = Menu.Menus.title.FindMenu("GameStart").Find("Countdown").gameObject.GetComponent<Countdown>();
				
				Instantiate(countdown.sound);
				countdown.sky.SendMessage("Switch");
				GameObject.Find("LightHolder").SendMessage("AllGoOut");
			}
		}
	}
}
