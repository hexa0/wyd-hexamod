using HexaModEditor.EditorSDK;
using UnityEditor;
using UnityEngine;

namespace HexaModEditor.Menus.Actions
{
	public class LevelCompiler
	{
		static readonly string directory = "Assets/CompiledLevelPrefabs";

		[MenuItem("Assets/Compile Level")]
		public static void CompileLevel()
		{
			LevelLink link = Object.FindObjectOfType<LevelLink>();
			if (link != null)
			{
				string compiledName = "compiled_" + link.name;

				foreach (var child in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
				{
					if (child.name == compiledName)
					{
						Object.DestroyImmediate(child);
					}
				}

				GameObject compiledLevelRoot = Object.Instantiate(link.gameObject);
				compiledLevelRoot.name = compiledName;

				// do level optimizations

				Object.DestroyImmediate(compiledLevelRoot.GetComponent<LevelLink>()); // only needed for this script to compile the correct object, so we remove it

				foreach (Transform t in compiledLevelRoot.GetComponentsInChildren<Transform>(true))
				{
					if (t && t.tag == "EditorOnly")
					{
						Object.DestroyImmediate(t.gameObject);
					}
				}

				// make it into a prefab

				string assetBundlePath = directory + "/" + compiledName + ".prefab";

				AssetDatabase.DeleteAsset(assetBundlePath);
				PrefabUtility.CreatePrefab(assetBundlePath, compiledLevelRoot);

				// set the level's scriptable object properties

				link.level.levelPrefab = null;
				link.level.levelPrefabPath = assetBundlePath;
				EditorUtility.SetDirty(link.level);

				// make prefab get included in the same asset bundle as the scripted object is in

				string levelAssetPath = AssetDatabase.GetAssetPath(link.level);

				AssetImporter levelAssetImporter = AssetImporter.GetAtPath(levelAssetPath);
				string assetBundleName = levelAssetImporter?.assetBundleName;
				string assetBundleVariant = levelAssetImporter?.assetBundleVariant;

				AssetImporter prefabAssetImporter = AssetImporter.GetAtPath(assetBundlePath);
				if (!string.IsNullOrEmpty(assetBundleName) && prefabAssetImporter != null)
				{
					prefabAssetImporter.assetBundleName = assetBundleName;
					prefabAssetImporter.assetBundleVariant = assetBundleVariant;
				}

				// save assets

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				// build bundles

				CreateAssetBundles.BuildAllAssetBundles();

				// cleanup compiled level object

				Object.DestroyImmediate(compiledLevelRoot);
			}
			else
			{
				throw new System.Exception("No LevelLink found.");
			}
		}
	}
}