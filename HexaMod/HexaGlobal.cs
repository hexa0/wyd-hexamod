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
using HexaMod.UI.Element.HexaMod.Loading;

namespace HexaMod
{
	public static class HexaGlobal
	{
		public static string assetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static AssetBundle coreBundle;
		public static AssetBundle startupBundle;

		public static int sendRate = 30;
		public static int defaultMaxPlayers = 64;
		public static RigidbodyInterpolation preferedInterpolation = RigidbodyInterpolation.Interpolate;

		public static PhotonNetworkManager networkManager;
		public static GameStateController gameStateController;
		public static RematchHelper rematchHelper;
		public static EventSystem eventSystem;
		public static RpcChatExtended textChat;
		public static HexaModPersistence hexaModPersistence = new GameObject("HexaModPersistent").AddComponent<HexaModPersistence>();
		public static HexaLobby hexaLobby;
		public static MainUI mainUI;

		public static void Load()
		{
			var activeScene = SceneManager.GetActiveScene();

			if (activeScene.name == "CompanyLogo")
			{
				Object.Destroy(GameObject.Find("Canvas"));
			}

			SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
			{
				OnGameSceneStart();
			};
		}

		public static void Init()
		{
			GameModes.DefineStandardGameModes();
			Assets.Init();
			HexaPersistentLobby.instance.Init();
		}

		public static void InitCoreBundle()
		{
			coreBundle = AssetBundle.LoadFromFile(PathJoin.Join(assetDir, "HexaModCoreResourcesBundle"));
		}

		public static void InitStartupBundle()
		{
			startupBundle = AssetBundle.LoadFromFile(PathJoin.Join(assetDir, "HexaModInitResourcesBundle"));
		}

		public static void OnGameSceneStart()
		{
			var activeScene = SceneManager.GetActiveScene();

			if (activeScene.name == "Game") {
				HexaMenus.startupScreen.fader.fadeState = false;
				HexaMenus.startupScreen.loadingText.SetText("Loaded Game");

				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;

				networkManager = Object.FindObjectOfType<PhotonNetworkManager>();
				if (Mod.GAME_VERSION == null)
				{
					Mod.GAME_VERSION = networkManager.version;
				}

				if (!Environment.GetCommandLineArgs().Contains("ForceVanillaLobbies"))
				{
					networkManager.version = $"hm:{BuildInfo.GitHash}";
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
					HexaPersistentLobby.instance.Reset();
				}
			}
			else if (activeScene.name == "CompanyLogo")
			{
				Object.Destroy(GameObject.Find("Canvas"));
			}

			HexaMenus.loadingOverlay.controller.SetTaskState("LevelLoad", false);
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
			PhotonNetwork.sendRateOnSerialize = sendRate;
		}
		public static readonly string instanceGuid = NewGuid().ToString();
		public static bool testGameWaitingForConn = false;
		public static void MakeTestGame(bool spawnAsDad = true)
		{
			networkManager.ConnectToPhoton();
			networkManager.gameName = instanceGuid;
			HexaPersistentLobby.instance.Reset();
			// PhotonNetwork.player.ID will be uninitialized at -1, 1 will always be our id in a test game so we set that
			HexaPersistentLobby.instance.dads[1] = spawnAsDad;
			networkManager.isDad = spawnAsDad;
			WYDMenus.title.menuController.DeactivateAll();
			testGameWaitingForConn = true;
			networkManager.curGameMode = GameModes.GetId("familyGathering");
		}
	}
}
