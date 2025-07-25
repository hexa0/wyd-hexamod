using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HexaMod.API.UI;
using HexaMod.API.UI.Util;
using HexaMod.API.Util.Unity;
using HexaMod.API.Util.WhosYourDaddy;
using HexaMod.API.Voice.Script;
using HexaMod.Patches.Feature;
using HexaMod.Patches.Hooks;
using HexaMod.Scripts.Character;
using HexaMod.Scripts.Multiplayer.Lobby;
using HexaMod.Scripts.Persistent;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using static System.Guid;
using static HexaMod.API.UI.Util.Menu;
using Object = UnityEngine.Object;

namespace HexaMod
{
	public static class HexaGlobal
	{
		public static string assetDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public static AssetBundle coreBundle;

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

			ExtendPrefabs();

			SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
			{
				OnGameSceneStart();
			};
		}

		public static void Init()
		{
			GameModes.DefineStandardGameModes();
			GameModes.DefineDefaultModdedGameModes();
			Assets.Init();
			HexaPersistentLobby.instance.Init();
		}

		public static void OnGameSceneStart()
		{
			var activeScene = SceneManager.GetActiveScene();

			if (activeScene.name == "Game")
			{
				PlayerControllers.parent = new GameObject("Players").GetComponent<Transform>();
				ItemSpawnerParent.parent = new GameObject("Items").GetComponent<Transform>();

				if (!PhotonNetwork.inRoom)
				{
					HexaMenus.startupScreen.loadingText.SetText("Loaded Game");
					HexaMenus.startupScreen.fader.fadeState = false;
				}

				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;

				networkManager = Object.FindObjectOfType<PhotonNetworkManager>();
				networkManager.aud.Stop();
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
		}

		public static void EnableInterpolationForAll()
		{
			foreach (var rigidbody in Object.FindObjectsOfType<Rigidbody>())
			{
				rigidbody.interpolation = preferedInterpolation;
			}
		}

		public static void ExtendPrefabs()
		{
			Object.DontDestroyOnLoad(PrefabExtensionUtils.customPrefabStorage);
			PrefabExtensionUtils.customPrefabStorage.SetActive(false);

			GameObject ExtendCharacter<PlayerController>(GameObject character) where PlayerController : HexaPlayerController
			{
				character.AddComponent<CharacterModelSwapper>();
				character.AddComponent<PlayerVoiceEmitterRPC>();
				character.AddComponent<CameraController>();
				character.AddComponent<CharacterItemInteraction>();
				ComponentSwapper.SwapComponents<FirstPersonController, PlayerController>(character);
				ComponentSwapper.SwapComponents<NetworkMovement, CharacterReplication>(character);

				return character;
			}

			GameObject dad = PrefabExtensionUtils.GetCachedNetworkPrefab("dadObj");
			GameObject baby = PrefabExtensionUtils.GetCachedNetworkPrefab("babyObj");

			ExtendCharacter<HexaDadController>(dad);
			ExtendCharacter<HexaBabyController>(baby);

			{
				GameObject luigi = Object.Instantiate(dad, PrefabExtensionUtils.Storage, false);
				luigi.name = "luigiObj";
				luigi.transform.localScale = new Vector3(0.95f, 1f, 0.95f);
				ComponentSwapper.SwapComponents<FirstPersonController, HexaLuigiController>(luigi);
				SkinnedMeshRenderer luigiMesh = luigi.transform.FindDeep("generic_male_01.005").GetComponent<SkinnedMeshRenderer>();
				Material shirt = luigiMesh.materials[4];
				shirt.color = new Color(0.2f, 0.6f, 0.2f, 1f);
				luigiMesh.materials[4] = shirt;
				PrefabExtensionUtils.RegisterCustomPrefab("luigiObj", luigi);
			}

			{
				GameObject ghost = Object.Instantiate(dad, PrefabExtensionUtils.Storage, false);
				ghost.name = "ghostObj";
				HexaPlayerController ghostController = ghost.GetComponent<HexaPlayerController>();
				ghostController.teamSelector = "G";
				PrefabExtensionUtils.RegisterCustomPrefab("ghostObj", ghost);
			}

			{
				GameObject prop = Object.Instantiate(dad, PrefabExtensionUtils.Storage, false);
				prop.name = "propObj";
				HexaPlayerController propController = prop.GetComponent<HexaPlayerController>();
				propController.teamSelector = "P";
				PrefabExtensionUtils.RegisterCustomPrefab("propObj", prop);
			}
		}

		public static void SnappierReplication()
		{
			PhotonNetwork.sendRate = sendRate;
			PhotonNetwork.sendRateOnSerialize = sendRate;
		}
		public static readonly string instanceGuid = NewGuid().ToString();
		public static void MakeTestGame(bool spawnAsDad = true)
		{
			PhotonNetwork.offlineMode = true;
			//networkManager.ConnectToPhoton();
			networkManager.gameName = instanceGuid;
			HexaPersistentLobby.instance.Reset();
			// PhotonNetwork.player.ID will be uninitialized at -1, 1 will always be our id in a test game so we set that
			HexaPersistentLobby.instance.dads[1] = spawnAsDad;
			networkManager.isDad = spawnAsDad;
			WYDMenus.title.menuController.DeactivateAll();
			networkManager.curGameMode = GameModes.GetId("familyGathering");

			RoomOptions roomOptions = new RoomOptions
			{
				IsOpen = false,
				IsVisible = false,
				MaxPlayers = 1
			};

			PhotonNetwork.CreateRoom(
				instanceGuid,
				roomOptions,
				PhotonNetwork.lobby
			);

			networkManager.StartMatch_FG();
		}
	}
}
