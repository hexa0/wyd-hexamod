using System;
using System.Collections;
using HarmonyLib;
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

            StartCoroutine(LoadLobbyLevel());
        }

        private bool waitingForTestRoom = false;

        public void Update()
        {
/*            if (Time.time > lastSettingsUpdate + 5f)
            {
                lastSettingsUpdate = Time.time;
                TryNetworkLobbySettings(HexaMod.persistentLobby.lobbySettings);
            }*/

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
                netView.RPC("SetLobbySettingsRPC", PhotonTargets.Others, new object[] { LobbySettings.Serialize(newSettings) });
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
            HexaMod.persistentLobby.lobbySettings = LobbySettings.Deserialize(newSettings);
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

            gameStateController.allMustDie = lobbySettings.allMustDie;
            networkManager.curGameMode = rematchHelper.curGameMode;
            networkManager.alternateCharacters = rematchHelper.alternateChars;
            networkManager.allowSpectate = rematchHelper.allowSpec;

            if (PhotonNetwork.isMasterClient)
            {
                gameStateController.GetComponent<PhotonView>().RPC("SetAllMustDie", PhotonTargets.Others, new object[] { lobby.lobbySettings.allMustDie && !GameModes.gameModes[rematchHelper.curGameMode].twoPlayer });
            }

            if (!lobby.dads.ContainsKey(PhotonNetwork.player.ID))
            {
                lobby.dads[PhotonNetwork.player.ID] = rematchHelper.isDadStart;
            }

            GameMode gameMode = GameModes.gameModes[rematchHelper.curGameMode];

            if (lobby.lobbySettings.shufflePlayers && gameMode.canAlternate)
            {
                networkManager.isDad = lobby.dads[PhotonNetwork.player.ID];
            }
            else
            {
                if (gameMode.canAlternate && networkManager.alternateCharacters)
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

                UI.Util.Menus.menuController.DeactivateAll();
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

        public IEnumerator LoadLobbyLevel()
        {
            yield return 0;

            Levels.loadedLevel = null;
            Levels.loadedLevelInstance = null;
            Levels.AttemptToLoadCurrentLevel();

            if (HexaMod.rematchHelper != null && PhotonNetwork.inRoom) {
                SetupMatch();
            }
        }

        public void StartMatch()
        {
            StartCoroutine(StartMatchEnumerator());
        }

        [PunRPC]
        public void HexaModMatchStarted()
        {
            HexaMod.mainUI.loadingController.SetTaskState("MatchLoad", false);
            HexaMod.networkManager.fader.SendMessage("Fade");
        }


        private IEnumerator StartMatchEnumerator()
        {
            netView.RPC("HexaModMatchStarted", PhotonTargets.All, new object[] { });
            // yield return 0;

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

            yield return 0;
        }
    }
}
