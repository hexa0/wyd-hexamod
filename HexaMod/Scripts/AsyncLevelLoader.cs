using System.Collections;
using HexaMod.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace HexaMod
{
    public class AsyncLevelLoader : MonoBehaviour
    {
        public bool loadIsDone = false;
        public UnityEvent loadCompleted;

        private uint levelBundlesToLoad = 0;
        private uint loadedLevelBundles = 0;

        public void LoadLevel(string filename, string file)
        {
            Mod.Print($"load level bundle {filename}");

            levelBundlesToLoad++;
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
                Levels.levels.Add(level);
                Mod.Print($"Found level {level.levelNameReadable}");
            };

            loadedLevelBundles++;

            if (loadedLevelBundles >= levelBundlesToLoad)
            {
                loadIsDone = true;
                loadCompleted.Invoke();
            }
        }
    }
}
