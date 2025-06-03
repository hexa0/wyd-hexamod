using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HexaMapAssemblies;
using HexaMod.ScriptableObjects;
using HexaMod.Util;
using NAudio.Gui;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using static HexaMod.HexaLobby;

namespace HexaMod
{
	internal static class Assets
	{
		internal static string assetsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "assets");
		internal static Dictionary<string, AssetBundle> assetBundles = new Dictionary<string, AssetBundle>();

		internal static List<ModLevel> levels = new List<ModLevel>();
		internal static List<ModRadioTrack> radioTracks = new List<ModRadioTrack>();
		internal static List<ModCharacterModel> characterModels = new List<ModCharacterModel>();
		internal static List<ModCharacterModel> dadCharacterModels = new List<ModCharacterModel>();
		internal static List<ModCharacterModel> babyCharacterModels = new List<ModCharacterModel>();

		internal static ModLevel titleLevel;
		internal static string titleName = "compiled_Default";

		public static bool loadedAssets = false;
		public static uint bundlesToLoad = 0;
		public static uint loadedBundles = 0;

		public static class StaticAssets
		{
			public static GameObject outletExplosion;
			public static GameObject outletShockSound;
			public static GameObject outletCoverPrefab;

			public static AudioClip titleSong;
			public static AudioClip daddySong;
			public static AudioClip babySong;
			public static AudioClip hgSong;
			public static AudioClip dadlympicSong;
			public static AudioClip dadNightmareSongDad;
			public static AudioClip dadNightmareSongBaby;

			public static GameObject cabinetOpen;
			public static GameObject cabinetClose;
			public static GameObject cabinetLocked;
			public static GameObject doorOpen;
			public static GameObject doorClose;

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

				daddySong = HexaMod.networkManager.daddySong;
				babySong = HexaMod.networkManager.babySong;
				hgSong = HexaMod.networkManager.hgSong;
				dadlympicSong = HexaMod.networkManager.dadlympicSong;
				dadNightmareSongDad = HexaMod.networkManager.dadNightmareSongDad;
				dadNightmareSongBaby = HexaMod.networkManager.dadNightmareSongBaby;
				if (!gameStarted)
				{
					titleSong = HexaMod.networkManager.aud.clip;
				}

				didCache = true;
			}
		}

		static bool gameStarted = false;

		public static void ScanForAssets(string directory)
		{
			foreach (string file in Directory.GetFiles(directory))
			{
				string filename = Path.GetFileName(file);

				HexaMod.asyncAssetLoader.LoadAsset(filename, file);
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

		public static void FixDefaultLevel()
		{
			//Rigidbody[] rigidbodies = Object.FindObjectsOfType<Rigidbody>();

			//foreach (var rigidbody in rigidbodies)
			//{
			//	if (rigidbody.GetComponent<NetworkMovementRB>() == null)
			//	{
			//		rigidbody.gameObject.AddComponent<NetworkMovementRB>();
			//	}

			//	if (rigidbody.GetComponent<PhotonView>() == null)
			//	{
			//		GlobalPhotonFactory.Register(rigidbody.gameObject, true);
			//	}
			//}
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
			Mod.Print($"defaultLevelObjects.Count = {defaultLevelObjects.Count}");
			if (defaultLevelObjects.Count == 0)
			{
				foreach (var child in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
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
							child.transform.Find("MoreObjects").Find("LightHolder").SetParent(GameObject.Find("BackendObjects").transform);
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
						case "BackendObjects":
							defaultLevelObjects.Add(child.transform.Find("Dadlympics").Find("PoolChoreObjs").gameObject);
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

					if (bad && child.activeInHierarchy)
					{
						defaultLevelObjects.Add(child);
					}
				}
			}

			Mod.Print($"NOW defaultLevelObjects.Count = {defaultLevelObjects.Count}");

			foreach (var levelObject in defaultLevelObjects)
			{
				levelObject.SetActive(false);
			}
		}

		public static DadSpawn dadSpawn;
		public static BabySpawn babySpawn;
		private static LowSpawn lowSpawn;
		private static MidSpawn midSpawn;
		private static SpecialSpawn specialSpawn;
		private static KeySpawn keySpawn;
		private static LevelMusic customLevelMusic;
		public static Transform loadedLevelInstance;
		public static ModLevel loadedLevel;

		public static Transform GetSpawnTransform(FirstPersonController player)
		{
			TeamSpawn spawn = player.name.ToLower().StartsWith("dad") ? (TeamSpawn)dadSpawn : (TeamSpawn)babySpawn;

			if (babySpawn.hgSpawns != null && HexaMod.networkManager.curGameMode == GameModes.named["hungryGames"].id)
			{
				spawn = babySpawn.hgSpawns;
			}

			return spawn.GetSpawn(HexaLobbyState.spawnIndex);
		}

		public static void HandleSpawnTeleport(FirstPersonController player)
		{
			if (dadSpawn && babySpawn)
			{
				Mod.Print($"handle player {player.name}");

				Transform spawnTransform = GetSpawnTransform(player);
				Mod.Print($"teleport player {player.name}");

				player.transform.position = spawnTransform.position;
				player.transform.rotation = spawnTransform.rotation;
				player.m_MouseLook.Init(player.transform, player.myCam.transform); // required or else character rotation isn't applied
			}
		}

		public static void LoadLevel(ModLevel level)
		{
			if (loadedLevel && loadedLevel.name == level.name)
			{
				return;
			}

			loadedLevel = level;

			if (loadedLevelInstance)
			{
				Object.Destroy(loadedLevelInstance.gameObject);
			}

			GlobalPhotonFactory.Reset();
			Mod.Print($"attempting to load map {level.name}");

			if (level != titleLevel)
			{
				Mod.Print("clearing default level.");

				CleanupDefaultLevel();
			}
			else
			{
				ActivateDefaultLevel();
			}

			Mod.Print("fetch player spawns.");

			dadSpawn = level.levelPrefab.GetComponentInChildren<DadSpawn>();
			babySpawn = level.levelPrefab.GetComponentInChildren<BabySpawn>();

			if (dadSpawn)
			{
				Mod.Print("got dad spawn.");
				dadSpawn.Init();
				HexaMod.networkManager.dadSpawnPos = dadSpawn.GetSpawn(0);
			}

			if (babySpawn)
			{
				Mod.Print("got baby spawn.");
				babySpawn.Init();
				HexaMod.networkManager.babySpawnPos = babySpawn.GetSpawn(0);

				if (babySpawn.hgSpawns != null)
				{
					Mod.Print("got hg baby spawn.");
					babySpawn.hgSpawns.Init();
				}
			}

			Mod.Print("done with (already connected) players!");

			Mod.Print("fetch item spawns.");

			lowSpawn = level.levelPrefab.GetComponentInChildren<LowSpawn>();
			midSpawn = level.levelPrefab.GetComponentInChildren<MidSpawn>();
			specialSpawn = level.levelPrefab.GetComponentInChildren<SpecialSpawn>();
			keySpawn = level.levelPrefab.GetComponentInChildren<KeySpawn>();

			if (lowSpawn && midSpawn && specialSpawn && keySpawn)
			{
				ItemSpawner itemSpawner = HexaMod.networkManager.itemSpawner.GetComponent<ItemSpawner>();
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

			if (level != titleLevel)
			{
				// prevent StartClocks from throwing an error

				HexaMod.gameStateController.clocks = new GameObject[] { };
				HexaMod.gameStateController.radio = HexaMod.gameStateController.gameObject.AddComponent<AudioSource>();
			}

			Mod.Print("done with item spawns.");

			Mod.Print("loading music.");

			customLevelMusic = level.levelPrefab.GetComponentInChildren<LevelMusic>();

			if (customLevelMusic != null)
			{
				HexaMod.networkManager.daddySong = customLevelMusic.DadTheme;
				HexaMod.networkManager.babySong = customLevelMusic.BabyTheme;
				HexaMod.networkManager.dadlympicSong = customLevelMusic.DadlympicTheme;
				HexaMod.networkManager.hgSong = customLevelMusic.HungryGamesTheme;
				HexaMod.networkManager.dadNightmareSongDad = customLevelMusic.DadNightmaresDadTheme;
				HexaMod.networkManager.dadNightmareSongBaby = customLevelMusic.DadNightmaresBabyTheme;
				if (!gameStarted)
				{
					HexaMod.networkManager.aud.clip = customLevelMusic.TitleTheme;
					HexaMod.networkManager.aud.Play();
				}
			}
			else
			{
				HexaMod.networkManager.daddySong = StaticAssets.daddySong;
				HexaMod.networkManager.babySong = StaticAssets.babySong;
				HexaMod.networkManager.dadlympicSong = StaticAssets.dadlympicSong;
				HexaMod.networkManager.hgSong = StaticAssets.hgSong;
				HexaMod.networkManager.dadNightmareSongDad = StaticAssets.dadNightmareSongDad;
				HexaMod.networkManager.dadNightmareSongBaby = StaticAssets.dadNightmareSongBaby;
				if (!gameStarted)
				{
					HexaMod.networkManager.aud.clip = StaticAssets.titleSong;
					HexaMod.networkManager.aud.Play();
				}
			}

			Mod.Print("inserting map assets.");

			var loaded = Object.Instantiate(level.levelPrefab);
			loaded.name = level.levelPrefab.name;
			loadedLevelInstance = loaded.transform;
		}

		public static void InitScene()
		{
			dadSpawn = null;
			babySpawn = null;
			lowSpawn = null;
			midSpawn = null;
			specialSpawn = null;
			keySpawn = null;
			loadedLevel = null;
			loadedLevelInstance = null;
			defaultLevelObjects.Clear();
			gameStarted = HexaMod.networkManager.gameStarted;

			AttemptToLoadCurrentLevel();
		}

		public static void AttemptToLoadCurrentLevel()
		{
			StaticAssets.CacheStaticWYDAssets();

			foreach (ModLevel level in levels)
			{
				if (level.levelPrefab.name == HexaMod.persistentLobby.lobbySettings.mapName)
				{
					LoadLevel(level);
					return;
				}
			}

			ActivateDefaultLevel();
		}
	}
}
