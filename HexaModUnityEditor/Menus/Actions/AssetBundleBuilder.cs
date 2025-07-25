using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HexaModEditor.Menus.Actions
{
	public class CreateAssetBundles
	{
		static readonly string directory = "AssetBundles";

		[MenuItem("Assets/Build AssetBundles")]
		public static void BuildAllAssetBundles()
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			BuildPipeline.BuildAssetBundles(directory, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

			string[] assetNames = AssetDatabase.GetAllAssetBundleNames();

			string projectDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

			foreach (var file in Directory.GetFiles(Path.Combine(projectDirectory, directory)))
			{
				string filename = Path.GetFileName(file);

				if (!filename.EndsWith(".manifest") && !assetNames.Contains(filename))
				{
					Debug.Log("removing old bundle " + filename);
					File.Delete(file);
					File.Delete(Path.ChangeExtension(file, ".manifest"));
				}
			}
		}

		[MenuItem("Assets/Get AssetBundle names")]
		static void GetNames()
		{
			var names = AssetDatabase.GetAllAssetBundleNames();
			foreach (var name in names)
				Debug.Log("AssetBundle: " + name);
		}
	}
}
