using System.IO;
using System.Reflection;
using UnityEngine;
using HexaMod.UI;
using HexaMod.UI.Util;
using HexaMod.Util;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using static HexaMod.UI.Util.Menu;
using static System.Guid;

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
		public static AsyncAssetLoader asyncAssetLoader;
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
			asyncAssetLoader = persistentGameObject.AddComponent<AsyncAssetLoader>();
			persistentLobby = persistentGameObject.AddComponent<HexaPersistentLobby>();

			Mod.Print("Setup Levels");
			Assets.Init();
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

				Menu.Init();
				mainUI = menuCanvas.gameObject.AddComponent<MainUI>();
				mainUI.Init();

				if (!PhotonNetwork.inRoom)
				{
					persistentLobby.Reset();
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
		public static string testGameGuid = NewGuid().ToString();
		public static bool testGameWaitingForConn = false;
		public static void MakeTestGame(bool spawnAsDad = true)
		{
			networkManager.ConnectToPhoton();
			networkManager.gameName = testGameGuid;
			persistentLobby.Reset();
			persistentLobby.dads[PhotonNetwork.player.ID] = spawnAsDad;
			networkManager.isDad = spawnAsDad;
			Menus.title.menuController.DeactivateAll();
			testGameWaitingForConn = true;
			networkManager.curGameMode = GameModes.named["familyGathering"].id;
		}
	}
}
