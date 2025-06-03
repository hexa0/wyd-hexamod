using System.Collections;
using System.IO;
using HexaMod.ScriptableObjects;
using UnityEngine;

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
			string type = Path.GetFileName(Path.GetDirectoryName(file));
			string from = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(file)));
			string withoutExtension = Path.GetFileNameWithoutExtension(filename);
			AssetBundleCreateRequest bundleLoadRequest = AssetBundle.LoadFromFileAsync(file);

			yield return bundleLoadRequest;
			var bundle = bundleLoadRequest.assetBundle;

			Assets.assetBundles.Add(filename, bundle);
			Mod.Print($"Loaded Asset Bundle {filename}");

			switch (type)
			{
				case "level":
					var allLevels = bundle.LoadAllAssetsAsync<ModLevel>();
					yield return allLevels;

					foreach (ModLevel level in allLevels.allAssets)
					{
						if (withoutExtension == "default_level" && from == "core")
						{
							Mod.Print($"Found default level {level.levelNameReadable}");
							Assets.defaultLevel = level;
							Assets.defaultLevelName = level.levelPrefab.name;
						}

						Assets.levels.Add(level);
						Mod.Print($"Found level {level.levelNameReadable}");
					}

					break;
				case "avatar":
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

					break;
				case "radio":
					var allRadioTracks = bundle.LoadAllAssetsAsync<ModRadioTrack>();
					yield return allRadioTracks;

					foreach (ModRadioTrack track in allRadioTracks.allAssets)
					{
						Assets.radioTracks.Add(track);
						Mod.Print($"Found radio track {track.name}");
					}

					break;
				case "shirt":
					var allShirts = bundle.LoadAllAssetsAsync<ModShirt>();
					yield return allShirts;

					foreach (ModShirt shirt in allShirts.allAssets)
					{
						if (withoutExtension == "default_shirt" && from == "core")
						{
							Mod.Print($"Found default shirt {shirt.name}");
							Assets.defaultShirt = shirt;
						}
						else
						{
							Mod.Print($"Found shirt {shirt.name}");
							Assets.shirts.Add(shirt);
						}
					}

					break;
			}

			Assets.loadedBundles++;

			if (Assets.loadedBundles >= Assets.bundlesToLoad)
			{
				Mod.Print("All Levels Loaded!");
				Assets.loadedAssets = true;
			}
		}
	}
}
