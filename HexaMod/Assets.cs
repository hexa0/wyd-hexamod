﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HexaMapAssemblies;
using HexaMod.ScriptableObjects;
using HexaMod.Scripts;
using HexaMod.Util;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using static HexaMod.Scripts.PunRpcExtensions.Lobby.HexaLobby;

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
		internal static string defaultLevelName = "compiled_Default";

		internal static ModShirt defaultShirt;

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

				daddySong = HexaGlobal.networkManager.daddySong;
				babySong = HexaGlobal.networkManager.babySong;
				hgSong = HexaGlobal.networkManager.hgSong;
				dadlympicSong = HexaGlobal.networkManager.dadlympicSong;
				dadNightmareSongDad = HexaGlobal.networkManager.dadNightmareSongDad;
				dadNightmareSongBaby = HexaGlobal.networkManager.dadNightmareSongBaby;
				if (!gameStarted)
				{
					titleSong = HexaGlobal.networkManager.aud.clip;
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

					if (bad && child.activeInHierarchy)
					{
						defaultLevelObjects.Add(child);
					}
				}
			}

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

			if (babySpawn.hgSpawns != null && HexaGlobal.networkManager.curGameMode == GameModes.GetId("hungryGames"))
			{
				spawn = babySpawn.hgSpawns;
			}

			return spawn.GetSpawn(HexaLobbyState.spawnIndex);
		}

		public static void HandleSpawnTeleport(FirstPersonController player)
		{
			if (dadSpawn && babySpawn)
			{
				Transform spawnTransform = GetSpawnTransform(player);

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

			dadSpawn = level.levelPrefab.GetComponentInChildren<DadSpawn>();
			babySpawn = level.levelPrefab.GetComponentInChildren<BabySpawn>();

			if (dadSpawn)
			{
				dadSpawn.Init();
				HexaGlobal.networkManager.dadSpawnPos = dadSpawn.GetSpawn(0);
			}

			if (babySpawn)
			{
				babySpawn.Init();
				HexaGlobal.networkManager.babySpawnPos = babySpawn.GetSpawn(0);

				if (babySpawn.hgSpawns)
				{
					babySpawn.hgSpawns.Init();
				}
			}

			lowSpawn = level.levelPrefab.GetComponentInChildren<LowSpawn>();
			midSpawn = level.levelPrefab.GetComponentInChildren<MidSpawn>();
			specialSpawn = level.levelPrefab.GetComponentInChildren<SpecialSpawn>();
			keySpawn = level.levelPrefab.GetComponentInChildren<KeySpawn>();

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
				// prevent StartClocks from throwing an error

				HexaGlobal.gameStateController.clocks = new GameObject[] { };
				HexaGlobal.gameStateController.radio = HexaGlobal.gameStateController.gameObject.AddComponent<AudioSource>();
			}

			customLevelMusic = level.levelPrefab.GetComponentInChildren<LevelMusic>();

			if (customLevelMusic != null)
			{
				HexaGlobal.networkManager.daddySong = customLevelMusic.DadTheme;
				HexaGlobal.networkManager.babySong = customLevelMusic.BabyTheme;
				HexaGlobal.networkManager.dadlympicSong = customLevelMusic.DadlympicTheme;
				HexaGlobal.networkManager.hgSong = customLevelMusic.HungryGamesTheme;
				HexaGlobal.networkManager.dadNightmareSongDad = customLevelMusic.DadNightmaresDadTheme;
				HexaGlobal.networkManager.dadNightmareSongBaby = customLevelMusic.DadNightmaresBabyTheme;
				if (!gameStarted)
				{
					HexaGlobal.networkManager.aud.clip = customLevelMusic.TitleTheme;
					HexaGlobal.networkManager.aud.Play();
				}
			}
			else
			{
				HexaGlobal.networkManager.daddySong = StaticAssets.daddySong;
				HexaGlobal.networkManager.babySong = StaticAssets.babySong;
				HexaGlobal.networkManager.dadlympicSong = StaticAssets.dadlympicSong;
				HexaGlobal.networkManager.hgSong = StaticAssets.hgSong;
				HexaGlobal.networkManager.dadNightmareSongDad = StaticAssets.dadNightmareSongDad;
				HexaGlobal.networkManager.dadNightmareSongBaby = StaticAssets.dadNightmareSongBaby;
				if (!gameStarted)
				{
					HexaGlobal.networkManager.aud.clip = StaticAssets.titleSong;
					HexaGlobal.networkManager.aud.Play();
				}
			}

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
			gameStarted = HexaGlobal.networkManager.gameStarted;

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
				if (level.levelPrefab.name == HexaPersistentLobby.instance.lobbySettings.mapName)
				{
					LoadLevel(level);
					return;
				}
			}

			ActivateDefaultLevel();
		}
	}
}
