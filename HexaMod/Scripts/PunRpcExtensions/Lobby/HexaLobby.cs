using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using HexaMod.SerializableObjects;
using HexaMod.UI;
using HexaMod.UI.Util;
using HexaMod.Util;
using HexaMod.Voice;
using UnityEngine;
using static HexaMod.UI.Util.Menu;

namespace HexaMod
{

	public class HexaLobby : MonoBehaviour
	{
		public static class HexaLobbyState
		{
			public static ushort spawnIndex;
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

		public void SendReadyToMasterClient()
		{
			PlayerConnectedObject player = new PlayerConnectedObject
			{
				isDad = HexaGlobal.networkManager.isDad
			};

			if (HexaPersistentLobby.instance.dads.ContainsKey(PhotonNetwork.player.ID))
			{
				player.isDad = HexaPersistentLobby.instance.dads[PhotonNetwork.player.ID];
			}

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
				SendReadyToMasterClient();
				HexaMenus.loadingOverlay.controller.SetTaskState("MatchLoad", true);
			}

			HexaPersistentLobby.instance.lobbySettingsChanged.AddListener(delegate ()
			{
				if (VoiceChatRoomsHook.inRoom)
				{
					VoiceChat.ConnectToRelay(HexaPersistentLobby.instance.lobbySettings.relay, HexaPersistentLobby.instance.lobbySettings.voiceRoom);
					VoiceChat.JoinVoiceRoom(HexaPersistentLobby.instance.lobbySettings.voiceRoom);
				}
			});
		}

		public void Start()
		{
			LoadLobbyLevel();
		}

		private bool waitingForTestRoom = false;

		public void Update()
		{
			if (HexaGlobal.testGameWaitingForConn || waitingForTestRoom)
			{
				if (!waitingForTestRoom)
				{
					if (PhotonNetwork.connectedAndReady)
					{
						HexaGlobal.testGameWaitingForConn = false;
						waitingForTestRoom = true;

						RoomOptions roomOptions = new RoomOptions
						{
							IsOpen = false,
							IsVisible = false,
							MaxPlayers = 1
						};
						PhotonNetwork.CreateRoom(
							"FG_" + HexaGlobal.networkManager.gameName + HexaGlobal.networkManager.gameNum,
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
						HexaGlobal.networkManager.StartMatch_FG();
					}
				}
			}
		}

		public void TryNetworkLobbySettings(LobbySettings newSettings)
		{
			newSettings.voiceRoom = HexaGlobal.instanceGuid;
			if (PhotonNetwork.isMasterClient && PhotonNetwork.inRoom)
			{
				netView.RPC("SetLobbySettingsRPC", PhotonTargets.Others, new object[] { LobbySettings.serializer.Serialize(newSettings) });
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

			Mod.Print($"got new ready player with name \"{GetPlayerName(info.sender)}\" and isDad = {player.isDad} {HexaLobbyState.loadedPlayers.Count}/{PhotonNetwork.room.PlayerCount}");

			if (HexaLobbyState.onPlayersLoadedAction != null && !HexaLobbyState.handledPlayersLoaded && HexaLobbyState.loadedPlayers.Count == PhotonNetwork.room.PlayerCount)
			{
				Mod.Print("Got all players.");
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
			HexaMenus.loadingOverlay.controller.SetTaskState("MatchLoad", true);

			Mod.Print("Waiting for players.");

			HexaLobbyState.onPlayersLoadedAction = delegate ()
			{
				if (HexaLobbyState.handledPlayersLoaded)
				{
					return;
				}

				HexaLobbyState.onPlayersLoadedAction = null;
				HexaLobbyState.loadedPlayers.Clear();

				Mod.Print("All players ready.");
				HexaLobbyState.handledPlayersLoaded = true;
				HexaMenus.loadingOverlay.controller.SetTaskState("MatchLoad", false);
				onPlayersLoaded();
			};

			if (HexaLobbyState.loadedPlayers.Count >= PhotonNetwork.room.PlayerCount)
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
				HexaGlobal.rematchHelper = null;
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

			Menu.WYDMenus.title.menuController.DeactivateAll();
			Destroy(HexaGlobal.rematchHelper);

			HexaGlobal.networkManager.fader.SendMessage("Fade");

			if (PhotonNetwork.isMasterClient)
			{
				HexaGlobal.hexaLobby.enabled = true;

				HexaGlobal.hexaLobby.WaitForPlayers(delegate ()
				{
					HexaGlobal.hexaLobby.StartMatch();
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
				MatchStartObject matchStartObject = new MatchStartObject()
					.DetermineSpawns(HexaPersistentLobby.instance.lobbySettings);

				netView.RPC("HexaModMatchStarted", PhotonTargets.All, new object[] { !PhotonNetwork.room.IsOpen, MatchStartObject.serializer.Serialize(matchStartObject) });

				var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];

				if (PhotonNetwork.room.IsOpen == false)
				{
					if (mode.respawnRPC != null)
					{
						HexaGlobal.networkManager.netView.RPC(mode.respawnRPC, PhotonTargets.All);
					}
					else
					{
						HexaGlobal.networkManager.RespawnPlayers();
					}
				}
			}
		}

		[PunRPC]
		public void HexaModMatchStarted(bool inGame, byte[] matchStartObjectData)
		{
			HexaGlobal.networkManager.fader.SendMessage("Fade");
			HexaGlobal.textChat.chat.CheckWho();

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

			var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];

			HexaMenus.loadingOverlay.controller.SetTaskState("MatchLoad", false);

			if (inGame)
			{
				VoiceChat.SetMicrophoneChannels(1);

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
				VoiceChat.SetMicrophoneChannels(2);

				var hostMenuId = WYDMenus.title.GetMenuId(mode.hostMenuName);

				WYDMenus.title.menuController.ChangeToMenu(hostMenuId);

				StartCoroutine(HexaModReturnedLobbyInit());
			}
		}

		IEnumerator HexaModReturnedLobbyInit()
		{
			var mode = GameModes.gameModes[HexaGlobal.networkManager.curGameMode];
			Transform hostMenu = Menu.WYDMenus.title.FindMenu(mode.hostMenuName);

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

		void OnCreatedRoom()
		{
			Mod.Print($"RoomCreated");
			VoiceChat.SetMicrophoneChannels(2);
			VoiceChat.ConnectToRelay(HexaPersistentLobby.instance.lobbySettings.relay);
			VoiceChat.JoinVoiceRoom(HexaGlobal.instanceGuid);
		}

		void OnJoinedRoom()
		{
			HexaLobbyState.handledPlayersLoaded = true;

			if (!PhotonNetwork.isMasterClient)
			{
				VoiceChat.SetMicrophoneChannels(2);
			}
		}

		void OnMasterClientSwitched(PhotonPlayer player)
		{
			Mod.Print($"master client switched to peer {player.ID}");
			if (player == PhotonNetwork.player)
			{
				HexaPersistentLobby.instance.SetInOtherLobby(false);

				//VoiceChat.ConnectToRelay(HexaPersistentLobby.instance.lobbySettings.relay);
				//VoiceChat.JoinVoiceRoom(HexaGlobal.instanceGuid);
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
