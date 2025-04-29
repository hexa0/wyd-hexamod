using System;
using System.Collections;
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
            Application.targetFrameRate = 30;

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
                mic.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
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

        void ActionText(string actionText)
        {
            Mod.Print(actionText);
            text.text = actionText;
        }


        AsyncOperation sceneLoadOperation;

        void InitHexaMod()
        {
            StartCoroutine(Load());
        }

        IEnumerator Load()
        {
            Mod.Warn(Application.installerName);
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
            Mod.Instance.harmony.PatchAll();
            ActionText("Start Scene Load");
            yield return new WaitForSeconds(0.5f); // avoid a crash?
            sceneLoadOperation = SceneManager.LoadSceneAsync(1);
            sceneLoadOperation.allowSceneActivation = false;
            ActionText("Init VoiceChat\n(Settings)");
            yield return 0;
            VoiceChat.InitUnityForVoiceChat();
            ActionText("Init VoiceChat\n(Transcode Process)");
            yield return 0;
            VoiceChat.InitTranscodeServerProcess();
            ActionText("Init VoiceChat\n(Transcode Connection)");
            yield return new WaitForSeconds(1f);
            VoiceChat.InitTranscodeServerConnection();
            ActionText("Init VoiceChat\n(Microphone)");
            yield return 0;
            VoiceChat.InitMicrophone();
            ActionText("Init HexaMod");
            yield return 0;
            HexaMod.Init();
            ActionText($"Loading Game\n({Math.Round(sceneLoadOperation.progress * 100, 2)}%)");
            yield return 0;
            sceneLoadOperation.allowSceneActivation = true;
            while (!sceneLoadOperation.isDone)
            {
                ActionText($"Loading Game\n({Math.Round(sceneLoadOperation.progress * 100, 2)}%)");
                yield return 0;
            }
            ActionText($"Loading Game Done");
        }
    }
}
