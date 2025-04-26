using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HexaMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    internal class Mod : BaseUnityPlugin
    {
        public const string GUID = "com.hexa0.hexamod";
        public const string NAME = "com.hexa0.hexamod";
        public const string VERSION = "0.0.0";

        internal readonly Harmony harmony = new Harmony(GUID);

        internal static Mod Instance;
        internal static ManualLogSource log;

        internal static void Print(params object[] messages) {
            log.LogInfo(string.Concat(messages));
        }

        internal static void PrintDebug(params object[] messages)
        {
            log.LogDebug(string.Concat(messages));
        }

        internal static void Warn(params object[] messages)
        {
            log.LogWarning(string.Concat(messages));
        }

        internal static void Error(params object[] messages)
        {
            log.LogError(string.Concat(messages));
        }

        internal static void Fatal(params object[] messages)
        {
            log.LogFatal(string.Concat(messages));
        }

        void Awake()
        {
            // Plugin startup logic
            if (Instance == null)
            {
                Instance = this;
            }

            log = BepInEx.Logging.Logger.CreateLogSource(GUID);

            log.LogInfo($"Plugin {GUID} is loaded!");

            AudioInput.InitUnityForVoiceChat();
            AudioInput.InitMicrophone();
            GameObject.Find("Canvas").AddComponent<IntroScript>().InitIntro();
        }
    }
}
