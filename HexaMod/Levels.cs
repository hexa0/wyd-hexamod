using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HexaMapAssemblies;
using HexaMod.ScriptableObjects;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod
{
    internal static class Levels
    {
        internal static string levelsDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "levels");
        internal static Dictionary<string, AssetBundle> levelBundles = new Dictionary<string, AssetBundle>();

        internal static List<ModLevel> levels = new List<ModLevel>();
        internal static ModLevel titleLevel;

        public static class StaticAssets
        {
            public static GameObject outletExplosion;
            public static GameObject outletShockSound;
        }
        public static void CacheStaticWYDAssets()
        {
            // run before we clean up the default map
            var outlet = Object.FindObjectOfType<PowerOutlet>();
            StaticAssets.outletExplosion = outlet.explosion;
            StaticAssets.outletShockSound = outlet.shockSound;
        }

        public static void Init()
        {
            foreach (string file in Directory.GetFiles(levelsDir))
            {
                string filename = file.Substring(levelsDir.Length + 1);

                HexaMod.asyncLevelLoader.LoadLevel(filename, file);
            }

            titleLevel = HexaMod.coreBundle.LoadAsset<ModLevel>("Assets/ModResources/Core/Level/Title.asset");
        }

        public static void CleanupDefaultLevel()
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
                        Object.Destroy(child.transform.Find("Dadlympics").Find("PoolChoreObjs").gameObject);
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
                    Object.Destroy(child);
                }
            }
        }

        private static DadSpawn dadSpawn;
        private static BabySpawn babySpawn;
        private static LowSpawn lowSpawn;
        private static MidSpawn midSpawn;
        private static SpecialSpawn specialSpawn;
        private static KeySpawn keySpawn;
        private static LevelMusic customLevelMusic;
        public static Transform loadedLevelInstance;
        public static ModLevel loadedLevel;

        public static void HandleSpawnTeleport(FirstPersonController player)
        {
            if (dadSpawn && babySpawn)
            {
                Mod.Print($"handle player {player.name}");
                if (player.name.ToLower().StartsWith("dad"))
                {
                    Mod.Print($"teleport dad {player.name}");
                    player.transform.position = dadSpawn.spots[0].position;
                }
                else if (player.name.ToLower().StartsWith("baby"))
                {
                    Mod.Print($"teleport baby {player.name}");
                    player.transform.position = babySpawn.spots[0].position;
                }
            }
        }

        public static void LoadLevel(ModLevel level)
        {
            loadedLevel = level;

            if (loadedLevelInstance)
            {
                Object.Destroy(loadedLevelInstance);
            }

            GlobalPhotonFactory.currentNetId = GlobalPhotonFactory.startingNetId;
            Mod.Print($"attempting to load map {level.name}");

            Mod.Print("clearing default level.");

            CacheStaticWYDAssets();
            CleanupDefaultLevel();

            Mod.Print("fetch player spawns.");

            dadSpawn = level.levelPrefab.GetComponentInChildren<DadSpawn>();
            babySpawn = level.levelPrefab.GetComponentInChildren<BabySpawn>();

            if (dadSpawn)
            {
                HexaMod.networkManager.dadSpawnPos = dadSpawn.spots[0];
            }

            if (babySpawn)
            {
                HexaMod.networkManager.babySpawnPos = babySpawn.spots[0];
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
                itemSpawner.lowTierPos = new GameObject[lowSpawn.spots.Length];
                itemSpawner.midTierPos = new GameObject[midSpawn.spots.Length];
                itemSpawner.specialPos = specialSpawn.spots;
                itemSpawner.keySpawns = keySpawn.spots;

                for (int i = 0; i < lowSpawn.spots.Length; i++)
                {
                    itemSpawner.lowTierPos[i] = lowSpawn.spots[i].gameObject;
                }

                for (int i = 0; i < midSpawn.spots.Length; i++)
                {
                    itemSpawner.midTierPos[i] = midSpawn.spots[i].gameObject;
                }
            }

            // prevent StartClocks from throwing an error

            HexaMod.gameStateController.clocks = new GameObject[] { };
            HexaMod.gameStateController.radio = HexaMod.gameStateController.gameObject.AddComponent<AudioSource>();

            Mod.Print("done with item spawns.");

            Mod.Print("loading music.");

            customLevelMusic = level.levelPrefab.GetComponentInChildren<LevelMusic>();

            if (customLevelMusic)
            {
                HexaMod.networkManager.daddySong = customLevelMusic.DadTheme;
                HexaMod.networkManager.babySong = customLevelMusic.BabyTheme;
                HexaMod.networkManager.dadlympicSong = customLevelMusic.DadlympicTheme;
                HexaMod.networkManager.hgSong = customLevelMusic.HungryGamesTheme;
                HexaMod.networkManager.dadNightmareSongDad = customLevelMusic.DadNightmaresDadTheme;
                HexaMod.networkManager.dadNightmareSongBaby = customLevelMusic.DadNightmaresBabyTheme;
                if (!HexaMod.networkManager.gameStarted)
                {
                    HexaMod.networkManager.aud.clip = customLevelMusic.TitleTheme;
                    HexaMod.networkManager.aud.Play();
                }
            }

            Mod.Print("inserting map assets.");

            var loaded = Object.Instantiate(level.levelPrefab);
            loaded.name = level.levelPrefab.name;
            loadedLevelInstance = loaded.transform;
        }

        public static void AttemptToLoadCurrentLevel()
        {
            if (!PhotonNetwork.inRoom)
            {
                LoadLevel(titleLevel);
            }
            else
            {
                foreach (ModLevel level in levels)
                {
                    if (level.levelPrefab.name == HexaMod.persistentLobby.lobbySettings.mapName)
                    {
                        LoadLevel(level);
                        break;
                    }
                }
            }
        }
    }
}
