using System.Collections;
using System.IO;
using HexaMod.ScriptableObjects;
using HexaMod.UI.Element;
using HexaMod.UI.Element.HexaMod.Loading;
using UnityEngine;

namespace HexaMod.Scripts
{
	public class AsyncAssetLoader : MonoBehaviour
	{
		public static AsyncAssetLoader instance;

		void Awake()
		{
			instance = this;
		}

		public void LoadAsset(string filename, string file)
		{
			Mod.Debug($"load asset bundle {filename}");

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
			Mod.Debug($"Loaded Asset Bundle {filename}");

			switch (type)
			{
#pragma warning disable IDE0220 // Add explicit cast
				case "level":
					var allLevels = bundle.LoadAllAssetsAsync<ModLevel>();
					yield return allLevels;

					foreach (ModLevel level in allLevels.allAssets)
					{
						if (withoutExtension == "default_level" && from == "core")
						{
							Mod.Debug($"Found default level {level.levelNameReadable}");
							Assets.defaultLevel = level;
							Assets.defaultLevelName = level.levelPrefab.name;
						}

						Assets.levels.Add(level);
						Mod.Debug($"Found level {level.levelNameReadable}");
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
						Mod.Debug($"Found v1 model {model.modelNameReadable}");
					}

					break;
				case "avatar2":
					var allV2CharacterModels = bundle.LoadAllAssetsAsync<ModCharacterModelV2>();
					yield return allV2CharacterModels;

					foreach (ModCharacterModelV2 model in allV2CharacterModels.allAssets)
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
						Mod.Debug($"Found v2 model {model.modelNameReadable}");
					}

					break;
				case "radio":
					var allRadioTracks = bundle.LoadAllAssetsAsync<ModRadioTrack>();
					yield return allRadioTracks;

					foreach (ModRadioTrack track in allRadioTracks.allAssets)
					{
						Assets.radioTracks.Add(track);
						Mod.Debug($"Found radio track {track.name}");
					}

					break;
				case "shirt":
					var allShirts = bundle.LoadAllAssetsAsync<ModShirt>();
					yield return allShirts;

					foreach (ModShirt shirt in allShirts.allAssets)
					{
						if (withoutExtension == "default_shirt" && from == "core")
						{
							Mod.Debug($"Found default shirt {shirt.name}");
							Assets.defaultShirt = shirt;
						}
						else
						{
							Mod.Debug($"Found shirt {shirt.name}");
							Assets.shirts.Add(shirt);
						}
					}

					break;
#pragma warning restore IDE0220 // Add explicit cast
				case "init":
					if (from != "core")
					{
						throw new System.Exception(type + " asset bundles can only be loaded from core, this has been blocked.");
					}

					var fontLoadRequest = bundle.LoadAssetAsync<Font>("Assets/ModResources/Init/Font/osd.ttf");
					var loadingAnimationRequest = bundle.LoadAssetAsync<GameObject>("Assets/ModResources/Init/LoadingUI/HexaLoadingAnimation.prefab");

					fontLoadRequest.completed += (request) =>
					{
						LoadingText.loadingFont = fontLoadRequest.asset as Font;
						Mod.Debug("got loadingFont");
					};

					loadingAnimationRequest.completed += (request) =>
					{
						LoadingAnimation.loadingAnimation = loadingAnimationRequest.asset as GameObject;
						Mod.Debug("got loadingAnimation");
					};

					break;
				case "resources":
					if (from != "core")
					{
						throw new System.Exception(type + " asset bundles can only be loaded from core, this has been blocked.");
					}

					HexaGlobal.coreBundle = bundle;
					WUIGlobals.instance = new WUIGlobals();

					yield return bundle.LoadAllAssetsAsync();

					break;
			}

			Assets.loadedBundles++;

			if (Assets.loadedBundles >= Assets.bundlesToLoad)
			{
				Mod.Debug("All Bundles Loaded!");
				Assets.loadedAssets = true;
			}
		}
	}
}
