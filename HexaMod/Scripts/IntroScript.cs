using System;
using System.Collections;
using System.Reflection;
using HexaMod.Voice;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod
{
    public class IntroScript : MonoBehaviour
    {
        public void InitIntro()
        {
            // lower fps for the loading screen
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 240;

            // setup scene loaded hook
            SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
            {
                HexaMod.OnGameSceneStart();
            };

            // remove the old company logo faders
            foreach (var fader in GetComponentsInChildren<ImgFade>())
            {
                Destroy(fader);
            };


            // setup the loading text (fontless for now)
            textObject = new GameObject("loadText", typeof(RectTransform));
            textObject.transform.SetParent(transform, false);
            text = textObject.AddComponent<UnityEngine.UI.Text>();
            text.fontSize = 200;
            text.rectTransform.sizeDelta = new Vector2(5000f, 5000f);
            text.alignment = TextAnchor.MiddleCenter;

            // begin the startup routine
            if (!VoiceChat.testMode)
            {
                InitHexaMod();
            }
        }

        public void Start()
        {
            if (VoiceChat.testMode)
            {
                AudioSource mic = gameObject.AddComponent<AudioSource>();
                mic.playOnAwake = true;
                mic.volume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
                mic.spatialBlend = 0f;
                mic.spatialize = false;
                mic.spread = 1f;
                mic.bypassEffects = true;
                mic.loop = true;

                VoiceEmitter voiceEmitter = gameObject.AddComponent<VoiceEmitter>();
                voiceEmitter.clientId = 0;

                VoiceChat.SetRelay("127.0.0.1");
                VoiceChat.JoinVoiceRoom("VoiceChat.testMode room");
            }
        }

        private GameObject textObject;
        private UnityEngine.UI.Text text;

        void Update()
        {
            Camera.current.backgroundColor = new Color(0.05f, 0.05f, 0.05f);
        }

        string lastActionText = string.Empty;
        void ActionText(string actionText)
        {
            if (lastActionText != actionText)
            {

                Mod.Print(actionText);
                text.text = actionText;
                lastActionText = actionText;
            }

        }


        AsyncOperation sceneLoadOperation;

        void InitHexaMod()
        {
            StartCoroutine(Load());
        }

        bool AttemptToConnect()
        {
            try
            {

                return true;
            }
            catch
            {
                return false;
            }
        }

        IEnumerator Load()
        {
            ActionText("Loading HexaModInitResourcesBundle");
            yield return 0;
            HexaMod.InitStartupBundle();
            text.font = HexaMod.startupBundle.LoadAsset<Font>("Assets/ModResources/Init/Font/osd.ttf");
            var loadingAnimation = HexaMod.startupBundle.LoadAsset<GameObject>("Assets/ModResources/Init/LoadingUI/HexaLoadingAnimation.prefab");
            Instantiate(loadingAnimation).transform.SetParent(transform, false);
            ActionText("Loading HexaModCoreResourcesBundle");
            yield return 0;
            HexaMod.InitCoreBundle();
            ActionText("Patching Game");
            yield return 0;
            Mod.Instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
            ActionText("Init HexaMod");
            yield return 0;
            HexaMod.Init();
            yield return 0;
            ActionText($"Loading Level Bundles\n(?/?)");
            while (!Assets.loadedAssets)
            {
                ActionText($"Loading Level Bundles\n({Assets.loadedBundles}/{Assets.bundlesToLoad})");
                yield return 0;
            }
            ActionText("Start Scene Load");
            yield return 0;
            VoiceChat.InitUnityForVoiceChat(); // this causes a hard crash if we call it while the scene is waiting to activate due to a race condition
            sceneLoadOperation = SceneManager.LoadSceneAsync(1);
            sceneLoadOperation.allowSceneActivation = false;
            ActionText("Init VoiceChat\n(Transcode Process)");
            yield return 0;
            VoiceChat.InitTranscodeServerProcess();
            VoiceChat.CreateTranscodeServerConnection();
            ActionText("Init VoiceChat\n(Transcode Connection)\n(Attempt 0)");
            yield return 0;
            int attempts = 0;
            while (!VoiceChat.completedHandshake)
            {
                ActionText($"Init VoiceChat\n(Transcode Connection)\n(Attempt {attempts})");
                yield return 0;

                try
                {
                    VoiceChat.InitTranscodeServerConnection();
                }
                catch (Exception e)
                {
                    Mod.Warn(e);
                }

                yield return 0;

                try
                {
                    VoiceChat.SendTranscodeServerHandshake();
                }
                catch (Exception e)
                {
                    Mod.Warn(e);
                }

                yield return 0;

                attempts++;
            }
            ActionText("Init VoiceChat\n(Microphone)");
            yield return 0;
            VoiceChat.InitMicrophone();
            ActionText("Init VoiceChat\n(Relay)");
            yield return 0;
            try
            {
                VoiceChat.SetRelay(HexaMod.persistentLobby.lobbySettings.relay);
            }
            catch
            {

            }
            ActionText($"Loading Game\n(0%)");
            yield return 0;
            sceneLoadOperation.allowSceneActivation = true;
            while (!sceneLoadOperation.isDone)
            {
                ActionText($"Loading Game\n({Math.Round(sceneLoadOperation.progress * 100, 2)}%)");
                yield return 0;
            }
        }
    }
}
