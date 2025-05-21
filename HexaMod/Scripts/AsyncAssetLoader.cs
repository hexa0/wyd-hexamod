using System.Collections;
using HexaMod.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace HexaMod
{
	public class AsyncAssetLoader : MonoBehaviour
	{
		public void LoadAsset(string filename, string file)
		{
			Mod.Print($"load asset bundle {filename}");

			Assets.bundlesToLoad++;
			StartCoroutine(LoadAssetsAsync(filename, file));
		}

		private IEnumerator LoadAssetsAsync(string filename, string file)
		{
			AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(file);

			yield return bundleLoadRequest;
			var bundle = bundleLoadRequest.assetBundle;

			Assets.assetBundles.Add(filename, bundle);
			Mod.Print($"Loaded Asset Bundle {filename}");

			var allLevels = bundle.LoadAllAssetsAsync<ModLevel>();
			yield return allLevels;

			foreach (ModLevel level in allLevels.allAssets)
			{
				if (filename == "title_screen")
				{
					Mod.Print($"Found title level {level.levelNameReadable}");
					Assets.titleLevel = level;
					Assets.titleName = level.levelPrefab.name;
				}

				Assets.levels.Add(level);
				Mod.Print($"Found level {level.levelNameReadable}");
			};

			var allCharacterModels = bundle.LoadAllAssetsAsync<ModCharacterModel>();
			yield return allCharacterModels;

			foreach (ModCharacterModel model in allCharacterModels.allAssets)
			{
				Assets.characterModels.Add(model);
				if (model.isDad)
				{
					Assets.dadCharacterModels.Add(model);
				}
				else
				{
					Assets.babyCharacterModels.Add(model);
				}
				Mod.Print($"Found model {model.modelNameReadable}");
			}
			;

			Assets.loadedBundles++;

			if (Assets.loadedBundles >= Assets.bundlesToLoad)
			{
				Mod.Print("All Levels Loaded!");
				Assets.loadedAssets = true;
			}
		}
	}
}
