using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod
{
	[BepInPlugin(GUID, NAME, VERSION)]
	internal class Mod : BaseUnityPlugin
	{
		public const string GUID = "com.hexa0.hexamod";
		public const string NAME = "com.hexa0.hexamod";
		public const string VERSION = "0.0.0";

		private static string[] GetLogs(params object[] messages)
		{
			return string.Concat(messages).Split('\n');
		}

		internal readonly Harmony harmony = new Harmony(GUID);

		internal static Mod instance;
		internal static ManualLogSource log;

		internal static int iFuckedUpAndNeedToDebugCounter = 0;

		internal static void ResetDebugCounter()
		{
			iFuckedUpAndNeedToDebugCounter = 0;
		}

		internal static void DebugCounter(params object[] label)
		{
			log.LogInfo(string.Concat(label) + $": {iFuckedUpAndNeedToDebugCounter}");
			iFuckedUpAndNeedToDebugCounter++;
		}

		internal static void Print(params object[] messages)
		{
			foreach (string message in GetLogs(messages))
			{
				log.LogInfo(message);
			}
		}

		internal static void PrintDebug(params object[] messages)
		{
			foreach (string message in GetLogs(messages))
			{
				log.LogDebug(message);
			}
		}

		internal static void Warn(params object[] messages)
		{
			foreach (string message in GetLogs(messages))
			{
				log.LogWarning(message);
			}
		}

		internal static void Error(params object[] messages)
		{
			foreach (string message in GetLogs(messages))
			{
				log.LogError(message);
			}
		}

		internal static void Fatal(params object[] messages)
		{
			foreach (string message in GetLogs(messages))
			{
				log.LogFatal(message);
			}
		}

		void Awake()
		{
			// Plugin startup logic
			instance = this;

			log = BepInEx.Logging.Logger.CreateLogSource(GUID);
			Print($"Loading plugin {GUID}!");
			HexaGlobal.Load();
			Print($"Plugin {GUID} is loaded!");

			var activeScene = SceneManager.GetActiveScene();

			if (activeScene.name == "CompanyLogo")
			{
				Destroy(GameObject.Find("Canvas"));
			}

			SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
			{
				OnGameSceneStart();
			};
		}

		public static void OnGameSceneStart()
		{
			var activeScene = SceneManager.GetActiveScene();

			if (activeScene.name == "CompanyLogo")
			{
				Destroy(GameObject.Find("Canvas"));
			}
		}
	}
}
