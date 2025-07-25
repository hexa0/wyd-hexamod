using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HexaMod.API.Util.Migration;
using UnityEditor;
using UnityEngine;

namespace HexaModEditor.Menus.UI.Migration
{
	public class UnityMigrationEditor : EditorWindow
	{
		private struct ScriptReferenceInfo
		{
			public string Guid;
			public string FileId;
		}

		private Vector2 _scrollPosition;
		private static readonly List<string> _logMessages = new List<string>();

		private string _allowPattern = "";
		private string _ignorePattern = "";

		[MenuItem("Tools/GUID Migration")]
		public static void ShowWindow()
		{
			GetWindow(typeof(UnityMigrationEditor), false, "GUID Migration");
		}

		void OnGUI()
		{
			EditorGUILayout.LabelField("Script & Asset Migration", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox(
				"This tool finds assets with missing or incorrect script references and attempts to relink them using the [UnityMigrationIdentifier] attribute.\n\nThis version correctly handles scripts located inside pre-compiled DLLs.",
				MessageType.Info);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Scan Options", EditorStyles.boldLabel);
			_allowPattern = EditorGUILayout.TextField(new GUIContent("Allow Paths", "Semicolon-separated list of paths to scan. Use * as a wildcard. Leave empty to scan all valid assets."), _allowPattern);
			_ignorePattern = EditorGUILayout.TextField(new GUIContent("Ignore Paths", "Semicolon-separated list of paths to ignore. Use * as a wildcard."), _ignorePattern);

			EditorGUILayout.Space();

			if (GUILayout.Button("Run Migration"))
			{
				if (EditorUtility.DisplayDialog("Confirm Migration",
					"This process will directly modify scene, prefab, and asset files. Please ensure you have a backup of your project before proceeding.",
					"Migrate", "Cancel"))
				{
					RunMigration(_allowPattern, _ignorePattern);
				}
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Logs:", EditorStyles.boldLabel);
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(position.height - 180));
			foreach (var msg in _logMessages)
			{
				EditorGUILayout.LabelField(msg, EditorStyles.wordWrappedLabel);
			}
			EditorGUILayout.EndScrollView();
		}

		private static void RunMigration(string allowPattern, string ignorePattern)
		{
			_logMessages.Clear();
			Log("Starting migration...");

			Dictionary<string, ScriptReferenceInfo> idToRefMap;
			List<string> assetsToScan = new List<string>();

			try
			{
				List<Type> migratableTypes = GetMigratableTypes();
				idToRefMap = BuildMigrationMap(migratableTypes);

				if (idToRefMap.Count == 0)
				{
					Log("No scripts with [UnityMigrationIdentifier] found. Nothing to migrate.");
					return;
				}
				Log($"Found {idToRefMap.Count} migratable scripts across all loaded assemblies.");

				string[] assetPaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
				string normalizedAllow = allowPattern.Replace("\\", "/");
				string normalizedIgnore = ignorePattern.Replace("\\", "/");

				foreach (var path in assetPaths)
				{
					string normalizedPath = path.Replace("\\", "/");

					if (!string.IsNullOrEmpty(normalizedIgnore) && PathMatches(normalizedPath, normalizedIgnore)) continue;
					if (!string.IsNullOrEmpty(normalizedAllow) && !PathMatches(normalizedPath, normalizedAllow)) continue;

					if (path.EndsWith(".unity") || path.EndsWith(".prefab") || path.EndsWith(".asset"))
					{
						assetsToScan.Add(path);
					}
				}

				int modifiedFileCount = 0;
				for (int i = 0; i < assetsToScan.Count; i++)
				{
					string assetPath = assetsToScan[i];
					EditorUtility.DisplayProgressBar("Applying Migration", $"Scanning: {Path.GetFileName(assetPath)}", (float)i / assetsToScan.Count);
					if (ProcessAssetFile(assetPath, idToRefMap))
					{
						modifiedFileCount++;
					}
				}

				Log($"Migration complete. Processed {assetsToScan.Count} assets and modified {modifiedFileCount} files.");

				if (modifiedFileCount > 0)
				{
					Log("Reloading assets to apply changes...");
					AssetDatabase.Refresh();
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>
		/// First pass: Gathers all types that have the migration attribute.
		/// </summary>
		private static List<Type> GetMigratableTypes()
		{
			var types = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			for (int i = 0; i < assemblies.Length; i++)
			{
				var assembly = assemblies[i];
				EditorUtility.DisplayProgressBar("Preparing Migration", $"Scanning assembly: {assembly.GetName().Name}", (float)i / assemblies.Length);

				string assemblyName = assembly.FullName.ToLower();
				if (assemblyName.StartsWith("unity") || assemblyName.StartsWith("system") || assemblyName.StartsWith("mono") || assemblyName.StartsWith("nunit"))
					continue;

				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (Attribute.IsDefined(type, typeof(UnityMigrationIdentifier), false))
						{
							if (typeof(MonoBehaviour).IsAssignableFrom(type) || typeof(ScriptableObject).IsAssignableFrom(type))
							{
								types.Add(type);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Log($"Could not process assembly {assembly.FullName}. Error: {ex.Message}");
				}
			}
			return types;
		}

		/// <summary>
		/// Second pass: Processes the gathered types and shows a detailed progress bar.
		/// </summary>
		private static Dictionary<string, ScriptReferenceInfo> BuildMigrationMap(List<Type> typesToProcess)
		{
			var map = new Dictionary<string, ScriptReferenceInfo>();
			var scriptRefRegex = new Regex(@"m_Script: \{fileID: (?<fileID>-?\d+), guid: (?<guid>[a-fA-F0-9]{32}), type: 3\}");

			for (int i = 0; i < typesToProcess.Count; i++)
			{
				Type type = typesToProcess[i];
				EditorUtility.DisplayProgressBar("Building Migration Map", $"({i + 1}/{typesToProcess.Count}) Processing: {type.Name}", (float)i / typesToProcess.Count);

				if (!(Attribute.GetCustomAttribute(type, typeof(UnityMigrationIdentifier), false) is UnityMigrationIdentifier attr)) continue; // Should not happen due to pre-filtering, but safe to check.

				string tempAssetPath = null;
				string yamlContent = null;

				if (typeof(ScriptableObject).IsAssignableFrom(type))
				{
					var instance = CreateInstance(type);
					tempAssetPath = "Assets/__TempSO.asset";
					AssetDatabase.CreateAsset(instance, tempAssetPath);
					yamlContent = File.ReadAllText(tempAssetPath);
				}
				else if (typeof(MonoBehaviour).IsAssignableFrom(type))
				{
					var tempGo = new GameObject("TempForMigration");
					tempGo.AddComponent(type);
					tempAssetPath = "Assets/__TempMigrationPrefab.prefab";
					PrefabUtility.CreatePrefab(tempAssetPath, tempGo);
					DestroyImmediate(tempGo);
					yamlContent = File.ReadAllText(tempAssetPath);
				}

				if (string.IsNullOrEmpty(yamlContent) || string.IsNullOrEmpty(tempAssetPath))
					continue;

				var match = scriptRefRegex.Match(yamlContent);
				if (match.Success)
				{
					var scriptRef = new ScriptReferenceInfo { FileId = match.Groups["fileID"].Value, Guid = match.Groups["guid"].Value };
					if (map.ContainsKey(attr.Id)) { Log($"Warning: Duplicate Migration ID '{attr.Id}' found."); } else { map[attr.Id] = scriptRef; }
				}
				AssetDatabase.DeleteAsset(tempAssetPath);
			}
			return map;
		}


		private static bool ProcessAssetFile(string filePath, Dictionary<string, ScriptReferenceInfo> idToRefMap)
		{
			string[] lines = File.ReadAllLines(filePath);
			bool fileModified = false;
			var scriptRefRegex = new Regex(@"(m_Script: \{fileID: )(?<fileID>-?\d+)(, guid: )(?<guid>[a-fA-F0-9]{32})(, type: 3\})");

			for (int i = 0; i < lines.Length; i++)
			{
				string trimmedLine = lines[i].Trim();
				if (trimmedLine.StartsWith("_serializedTypeId:"))
				{
					string migrationId = trimmedLine.Substring(trimmedLine.IndexOf(':') + 1).Trim();

					if (idToRefMap.TryGetValue(migrationId, out ScriptReferenceInfo correctRef))
					{
						for (int j = i - 1; j >= 0; j--)
						{
							var match = scriptRefRegex.Match(lines[j]);
							if (match.Success)
							{
								string oldFileId = match.Groups["fileID"].Value;
								string oldGuid = match.Groups["guid"].Value;

								if (oldFileId != correctRef.FileId || oldGuid.ToLower() != correctRef.Guid.ToLower())
								{
									lines[j] = $"  m_Script: {{fileID: {correctRef.FileId}, guid: {correctRef.Guid}, type: 3}}";
									fileModified = true;
									Log($"'{Path.GetFileName(filePath)}': Migrated component with ID '{migrationId}'.");
								}
								break;
							}
							if (lines[j].StartsWith("--- !u!")) break;
						}
					}
				}
			}

			if (fileModified)
			{
				File.WriteAllLines(filePath, lines);
				return true;
			}

			return false;
		}

		private static bool PathMatches(string path, string patterns)
		{
			var regexes = patterns.Split(';')
				.Select(p => p.Trim())
				.Where(p => !string.IsNullOrEmpty(p))
				.Select(p => new Regex(WildcardToRegex(p), RegexOptions.IgnoreCase));

			foreach (var regex in regexes)
			{
				if (regex.IsMatch(path))
				{
					return true;
				}
			}
			return false;
		}

		private static string WildcardToRegex(string pattern)
		{
			return "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
		}

		private static void Log(string message)
		{
			Debug.Log($"[MigrationTool] {message}");
			_logMessages.Add(message);
		}
	}
}