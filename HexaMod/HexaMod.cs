using System.IO;
using System.Reflection;
using UnityEngine;
using HexaMod.UI;
using HexaMod.UI.Util;
using HexaMod.Util;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace HexaMod
{
    public static class HexaMod
    {
        public static string assetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static AssetBundle coreBundle;
        public static AssetBundle startupBundle;

        public static int sendRate = 60;
        public static int defaultMaxPlayers = 64;
        public static RigidbodyInterpolation preferedInterpolation = RigidbodyInterpolation.Interpolate;

        public static PhotonNetworkManager networkManager;
        public static GameStateController gameStateController;
        public static RematchHelper rematchHelper;
        public static EventSystem eventSystem;
        public static HexaLobby hexaLobby;
        public static HexaModPersistence persistentInstance;
        public static AsyncLevelLoader asyncLevelLoader;
        public static HexaPersistentLobby persistentLobby;
        public static MainUI mainUI;
        public static void Init()
        {
            Mod.Print("Setup Settings");

            GameModes.DefineStandardGameModes();

            Mod.Print("Setup HexaModPersistance");

            GameObject persistentGameObject = new GameObject();
            Object.DontDestroyOnLoad(persistentGameObject);

            persistentGameObject.name = "HexaModPersistent";
            persistentInstance = persistentGameObject.AddComponent<HexaModPersistence>();
            asyncLevelLoader = persistentGameObject.AddComponent<AsyncLevelLoader>();
            persistentLobby = persistentGameObject.AddComponent<HexaPersistentLobby>();

            Mod.Print("Setup Levels");
            Levels.Init();
            Mod.Print("Setup HexaPersistentLobby");
            persistentLobby.Init();
        }

        public static void InitCoreBundle()
        {
            coreBundle = AssetBundle.LoadFromFile(Path.Combine(assetDir, "HexaModCoreResourcesBundle"));
        }

        public static void InitStartupBundle()
        {
            startupBundle = AssetBundle.LoadFromFile(Path.Combine(assetDir, "HexaModInitResourcesBundle"));
        }

        public static void OnGameSceneStart()
        {
            var activeScene = SceneManager.GetActiveScene();
            Mod.Print($"HexaMod OnGameSceneStart {activeScene.name}");

            if (activeScene.name == "Game") {
                Application.targetFrameRate = 0;

                networkManager = Object.FindObjectOfType<PhotonNetworkManager>();
                gameStateController = Object.FindObjectOfType<GameStateController>();
                eventSystem = Object.FindObjectOfType<EventSystem>();
                networkManager.version = $"Game:\t{networkManager.version.Substring(1)}\nHexaMod:\t{Mod.VERSION}";

                hexaLobby = networkManager.gameObject.AddComponent<HexaLobby>();
                hexaLobby.enabled = true;

                FixRigidBodies();
                SnappierReplication();

                Menus.Init();
                mainUI = Menus.menuCanvas.gameObject.AddComponent<MainUI>();

                if (!PhotonNetwork.inRoom)
                {
                    persistentLobby.lobbySettings.roundNumber = 0;
                    persistentLobby.dads.Clear();
                    persistentLobby.CommitChanges();
                }
            }
        }

        public static void FixRigidBodies()
        {
            foreach (var rigidbody in Object.FindObjectsOfType<Rigidbody>())
            {
                rigidbody.interpolation = preferedInterpolation;
            }
        }

        public static void SnappierReplication()
        {
            PhotonNetwork.sendRate = sendRate;
        }

        public static bool testGameWaitingForConn = false;
        public static void MakeTestGame(bool spawnAsDad = true)
        {
            Menus.menuController.DeactivateAll();
            networkManager.ConnectToPhoton();
            testGameWaitingForConn = true;
            networkManager.isDad = spawnAsDad;
        }
    }
}
