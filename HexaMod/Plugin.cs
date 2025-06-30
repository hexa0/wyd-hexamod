using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace HexaMod
{
	[BepInPlugin(BuildInfo.AssemblyName, BuildInfo.AssemblyTitle, BuildInfo.Version)]
	internal class Mod : BaseUnityPlugin
	{
		public static string GAME_VERSION;
		public static string LOCATION = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

		private static string[] GetLogs(params object[] messages)
		{
			return string.Concat(messages).Split('\n');
		}

		internal readonly Harmony harmony = new Harmony(BuildInfo.AssemblyName);
		internal static Mod instance;
		internal static ManualLogSource log;

		internal static void Print(params object[] messages)
		{
			foreach (string message in GetLogs(messages))
			{
				log.LogInfo(message);
			}
		}

		internal static void Debug(params object[] messages)
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
			log = BepInEx.Logging.Logger.CreateLogSource(BuildInfo.AssemblyName);
			
			Print($"Starting plugin {BuildInfo.AssemblyTitle} ({BuildInfo.AssemblyName}).");
			HexaGlobal.Load();
			Print($"Plugin {BuildInfo.AssemblyTitle} ({BuildInfo.AssemblyName}) has started.");
		}
	}
}
