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
using HexaMod.Patches.Hooks;
using System;
using System.Linq;
using Object = UnityEngine.Object;

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
		public static RpcChatExtended textChat;
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
				networkManager = Object.FindObjectOfType<PhotonNetworkManager>();
				if (!Environment.GetCommandLineArgs().Contains("ForceVanillaLobbies"))
				{
					networkManager.version = $"Game:\t{networkManager.version.Substring(1)}\nHexaMod:\t{Mod.VERSION}";
				}
				gameStateController = Object.FindObjectOfType<GameStateController>();
				eventSystem = Object.FindObjectOfType<EventSystem>();

				hexaLobby = networkManager.gameObject.AddComponent<HexaLobby>();
				hexaLobby.enabled = true;

				EnableInterpolationForAll();
				SnappierReplication();

				Menu.Init();
				mainUI = menuCanvas.gameObject.AddComponent<MainUI>();
				mainUI.Init();

				textChat = Object.FindObjectOfType<RpcChat>().gameObject.AddComponent<RpcChatExtended>();
				textChat.Init();

				if (!PhotonNetwork.inRoom)
				{
					persistentLobby.Reset();
				}
			}
		}

		public static void EnableInterpolationForAll()
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
			// PhotonNetwork.player.ID will be uninitialized at -1, 1 will always be our id in a test game so we set that
			persistentLobby.dads[1] = spawnAsDad;
			networkManager.isDad = spawnAsDad;
			Menus.title.menuController.DeactivateAll();
			testGameWaitingForConn = true;
			networkManager.curGameMode = GameModes.GetId("familyGathering");
		}
	}
}
