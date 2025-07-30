using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HexaMod.Patches.Feature;
using HexaMod.Scripts.Persistent;
using HexaMod.SDK.Levels.Scripts.Customization;
using HexaMod.SDK.Levels.Scripts.Factory;
using HexaMod.SDK.Levels.Scripts.Spawning.Item;
using HexaMod.SDK.Levels.Scripts.Spawning.Player;
using HexaMod.SDK.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod
{
	internal static class Assets
	{
		internal static string assetsDir = PathJoin.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
		internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

		internal static List<ModLevel> levels = new List<ModLevel>();
		internal static List<ModRadioTrack> radioTracks = new List<ModRadioTrack>();
		internal static List<ModShirt> shirts = new List<ModShirt>();
		internal static List<ModCharacterModelBase> characterModels = new List<ModCharacterModelBase>();
		internal static List<ModCharacterModelBase> dadCharacterModels = new List<ModCharacterModelBase>();
		internal static List<ModCharacterModelBase> babyCharacterModels = new List<ModCharacterModelBase>();

		internal static ModLevel defaultLevel;
		internal static string defaultLevelName = "Assets/CompiledLevelPrefabs/compiled_Default.prefab";

		internal static ModShirt defaultShirt;

		public static bool loadedAssets = false;
		public static uint bundlesToLoad = 0;
		public static uint loadedBundles = 0;

		public static class StaticAssets
		{
			public static GameObject outletExplosion;
			public static GameObject outletShockSound;
			public static GameObject outletCoverPrefab;

			public static GameObject cabinetOpen;
			public static GameObject cabinetClose;
			public static GameObject cabinetLocked;
			public static GameObject doorOpen;
			public static GameObject doorClose;

			public static LevelSongEntry[] defaultLevelThemes = new LevelSongEntry[0];

			public static bool didCache = false;

			public static void CacheStaticWYDAssets()
			{
				if (didCache)
				{
					return;
				}

				FixDefaultLevel();

				Mod.Print($"caching static WYD assets.");

				// run before we clean up the default map
				var outlet = Object.FindObjectOfType<PowerOutlet>();
				outletExplosion = outlet.explosion;
				outletShockSound = outlet.shockSound;
				outletCoverPrefab = outlet.coverPrefab;


				foreach (Cabinet2 cabinet in Object.FindObjectsOfType<Cabinet2>())
				{
					if (cabinet.cabOpen != null)
					{
						cabinetOpen = cabinet.cabOpen;
						cabinetClose = cabinet.cabClose;
						cabinetLocked = cabinet.lockSound;
						break;
					}
				}

				var door = Object.FindObjectOfType<Door>();
				doorOpen = door.openSound;
				doorClose = door.closeSound;

				{
					List<LevelSongEntry> tempDefaultSongList = new List<LevelSongEntry>();
					tempDefaultSongList.AddRange(new LevelSongEntry[] {
						new LevelSongEntry()
						{
							gamemode = "title:A",
							value = HexaGlobal.networkManager.aud.clip
						},
						new LevelSongEntry()
						{
							gamemode = "default:A",
							value = HexaGlobal.networkManager.daddySong
						},
						new LevelSongEntry()
						{
							gamemode = "default:D",
							value = HexaGlobal.networkManager.daddySong
						},
						new LevelSongEntry()
						{
							gamemode = "default:B",
							value = HexaGlobal.networkManager.babySong
						},
						new LevelSongEntry()
						{
							gamemode = "hungryGames:A",
							value = HexaGlobal.networkManager.hgSong
						},
						new LevelSongEntry()
						{
							gamemode = "dadlympics:A",
							value = HexaGlobal.networkManager.dadlympicSong
						},
						new LevelSongEntry()
						{
							gamemode = "daddysNightmare:A",
							value = HexaGlobal.networkManager.dadNightmareSongDad
						},
						new LevelSongEntry()
						{
							gamemode = "daddysNightmare:D",
							value = HexaGlobal.networkManager.dadNightmareSongDad
						},
						new LevelSongEntry()
						{
							gamemode = "daddysNightmare:B",
							value = HexaGlobal.networkManager.dadNightmareSongBaby
						}
					});

					defaultLevelThemes = tempDefaultSongList.ToArray();
				}

				didCache = true;
			}
		}

		public static void ScanForAssets(string directory)
		{
			foreach (string file in Directory.GetFiles(directory))
			{
				string filename = Path.GetFileName(file);

				AsyncAssetLoader.instance.LoadAsset(filename, file);
			}

			foreach (string subDirectory in Directory.GetDirectories(directory))
			{
				ScanForAssets(subDirectory);
			}
		}

		public static void Init()
		{
			ScanForAssets(assetsDir);
		}

		public static List<GameObject> defaultLevelObjects = new List<GameObject>();
		public static bool clearDefaultLevelObjectsOnReady = false;

		public static void FixDefaultLevel()
		{
			Renderer[] renderers = Object.FindObjectsOfType<Renderer>();

			foreach (var renderer in renderers)
			{
				renderer.allowOcclusionWhenDynamic = true;

				foreach (var material in renderer.materials)
				{
					material.enableInstancing = true;
					material.doubleSidedGI = false;
				}

				if (renderer.material != null)
				{
					renderer.material.enableInstancing = true;
					renderer.material.doubleSidedGI = false;
				}
			}
		}

		public static void ActivateDefaultLevel()
		{
			if (loadedLevelInstance)
			{
				Object.Destroy(loadedLevelInstance.gameObject);
			}

			foreach (var levelObject in defaultLevelObjects)
			{
				levelObject.SetActive(true);
			}
		}

		public static void CleanupDefaultLevel()
		{
			if (defaultLevelObjects.Count == 0)
			{
				foreach (GameObject child in SceneManager.GetActiveScene().GetRootGameObjects())
				{
					var bad = false;

					switch (child.name)
					{
						case "Rooms":
							bad = true;
							break;
						case "VersionAdded":
							bad = true;
							break;
						case "Water":
							bad = true;
							break;
						case "Water4AdvancedReflectionSceneCamera":
							bad = true;
							break;
						case "UnderMapTrigger":
							bad = true;
							break;
						case "Misc":
							child.FindDeep("LightHolder").SetParent(GameObject.Find("BackendObjects"));
							bad = true;
							break;
						case "Halloween decs":
							bad = true;
							break;
						case "Pool":
							bad = true;
							break;
						case "PoolCover":
							bad = true;
							break;
						case "Pool Cover":
							bad = true;
							break;
						case "Baby Car":
							bad = true;
							break;
						case "DaddysNightmareSpawns":
							//bad = true;
							break;
						case "Daddys Nightmare":
							child.SetParent(GameObject.Find("BackendObjects"));
							break;
						case "Pets":
							bad = true;
							break;
						case "BackendObjects":
							defaultLevelObjects.Add(child.Find("Dadlympics").Find("PoolChoreObjs"));
							break;
					}

					if (child.name.StartsWith("Pool Cover"))
					{
						bad = true;
					}

					if (child.name.StartsWith("Baby Car"))
					{
						bad = true;
					}

					if (bad)
					{
						if (!child.activeInHierarchy)
						{
							Object.Destroy(child);
						}
						else
						{
							defaultLevelObjects.Add(child);
						}
					}
				}
			}

			foreach (Transform child in ItemSpawnerParent.parent.GetComponentInChildren<Transform>())
			{
				if (child.name.StartsWith("Pool Cover") || child.name.StartsWith("Baby Car"))
				{
					defaultLevelObjects.Add(child.gameObject);
				}
			}

			if (PhotonNetwork.room != null && !PhotonNetwork.room.IsOpen)
			{
				clearDefaultLevelObjectsOnReady = true;
			}

			foreach (var levelObject in defaultLevelObjects)
			{
				levelObject.SetActive(false);
			}
		}

		public static TeamSpawn teamSpawn;
		private static LowSpawn lowSpawn;
		private static MidSpawn midSpawn;
		private static SpecialSpawn specialSpawn;
		private static KeySpawn keySpawn;
		public static LevelMusic customLevelMusic;
		public static Transform loadedLevelInstance;
		public static ModLevel loadedLevel;

		public static Transform GetSpawnTransform(int teamPlayerId)
		{
			return teamSpawn.GetSpawn(teamPlayerId);
		}

		static void LoadLevel(ModLevel level)
		{
			if (loadedLevel && loadedLevel.name == level.name)
			{
				return;
			}

			if (level.levelPrefab == null)
			{
				level.levelPrefab = level.levelBundle.LoadAsset<GameObject>(level.levelPrefabPath);
			}

			loadedLevel = level;

			if (loadedLevelInstance)
			{
				Object.DestroyImmediate(loadedLevelInstance.gameObject);

				if (PhotonNetwork.isMasterClient)
				{
					PhotonNetwork.DestroyAll();
				}
			}

			GlobalPhotonFactory.Reset();

			if (level != defaultLevel)
			{
				CleanupDefaultLevel();
			}
			else
			{
				ActivateDefaultLevel();
			}

			loadedLevelInstance = Object.Instantiate(level.levelPrefab).transform;
			loadedLevelInstance.name = level.LevelIdentifier;

			teamSpawn = loadedLevelInstance.GetComponentInChildren<TeamSpawn>();

			lowSpawn = loadedLevelInstance.GetComponentInChildren<LowSpawn>();
			midSpawn = loadedLevelInstance.GetComponentInChildren<MidSpawn>();
			specialSpawn = loadedLevelInstance.GetComponentInChildren<SpecialSpawn>();
			keySpawn = loadedLevelInstance.GetComponentInChildren<KeySpawn>();

			if (lowSpawn && midSpawn && specialSpawn && keySpawn)
			{
				ItemSpawner itemSpawner = HexaGlobal.networkManager.itemSpawner.GetComponent<ItemSpawner>();
				itemSpawner.lowTierPos = new GameObject[itemSpawner.lowTierObj.Length];
				itemSpawner.midTierPos = new GameObject[itemSpawner.midTierObj.Length];
				itemSpawner.specialPos = specialSpawn.spots;

				itemSpawner.keySpawns = keySpawn.spots;

				for (int i = 0; i < itemSpawner.lowTierObj.Length; i++)
				{
					itemSpawner.lowTierPos[i] = lowSpawn.spots[i % lowSpawn.spots.Length].gameObject;
				}

				for (int i = 0; i < itemSpawner.midTierObj.Length; i++)
				{
					itemSpawner.midTierPos[i] = midSpawn.spots[i % midSpawn.spots.Length].gameObject;
				}

				itemSpawner.ShuffleOrder(itemSpawner.lowTierPos);
				itemSpawner.ShuffleOrder(itemSpawner.midTierPos);
			}

			if (level != defaultLevel)
			{
				HexaGlobal.gameStateController.clocks = new GameObject[] { };
				HexaGlobal.gameStateController.radio = HexaGlobal.gameStateController.gameObject.AddComponent<AudioSource>();
			}

			customLevelMusic = loadedLevelInstance.GetComponentInChildren<LevelMusic>();

			HexaGlobal.hexaLobby.SendReadyToMasterClient();
		}

		public static void InitScene()
		{
			teamSpawn = null;
			lowSpawn = null;
			midSpawn = null;
			specialSpawn = null;
			keySpawn = null;
			loadedLevel = null;
			loadedLevelInstance = null;
			defaultLevelObjects.Clear();
			clearDefaultLevelObjectsOnReady = false;

			if (PhotonNetwork.inRoom)
			{
				HexaGlobal.networkManager.gameStarted = true;
			}

			AttemptToLoadCurrentLevel();
		}

		public static void AttemptToLoadCurrentLevel()
		{
			StaticAssets.CacheStaticWYDAssets();

			foreach (ModLevel level in levels)
			{
				if (level.LevelIdentifier == HexaPersistentLobby.instance.lobbySettings.mapName)
				{
					LoadLevel(level);
					return;
				}
			}

			HexaGlobal.hexaLobby.SendReadyToMasterClient();
			ActivateDefaultLevel();
		}
	}
}
