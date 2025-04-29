using System.Collections;
using HexaMod.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace HexaMod
{
    public class AsyncLevelLoader : MonoBehaviour
    {
        public UnityEvent loadCompleted;

        public void LoadLevel(string filename, string file)
        {
            Mod.Print($"load level bundle {filename}");

            Levels.levelBundlesToLoad++;
            StartCoroutine(LoadLevelsAsync(filename, file));
        }

        private IEnumerator LoadLevelsAsync(string filename, string file)
        {
            AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(file);

            yield return bundleLoadRequest;
            var bundle = bundleLoadRequest.assetBundle;

            Levels.levelBundles.Add(filename, bundle);
            Mod.Print($"Loaded Level Asset Bundle {filename}");

            var allLevels = bundle.LoadAllAssetsAsync<ModLevel>();
            yield return allLevels;

            foreach (ModLevel level in allLevels.allAssets)
            {
                if (filename == "title_screen")
                {
                    Mod.Print($"Found title level {level.levelNameReadable}");
                    Levels.titleLevel = level;
                }
                else
                {
                    Levels.levels.Add(level);
                    Mod.Print($"Found level {level.levelNameReadable}");
                }
            };

            Levels.loadedLevelBundles++;

            if (Levels.loadedLevelBundles >= Levels.levelBundlesToLoad)
            {
                Mod.Print("All Levels Loaded!");
                Levels.loadedLevels = true;
                // loadCompleted.Invoke();
            }
        }
    }
}
