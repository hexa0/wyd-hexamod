using System;
using System.Collections;
using System.Diagnostics;
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
                StartCoroutine(InitHexaModAfterFrame());
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

        IEnumerator InitHexaModAfterFrame()
        {
            yield return 0;
            InitHexaMod();
        }

        void InitHexaMod()
        {
            void InitStartupResources()
            {
                HexaMod.InitStartupBundle();
                text.font = HexaMod.startupBundle.LoadAsset<Font>("Assets/ModResources/Init/Font/osd.ttf");
                var loadingAnimation = HexaMod.startupBundle.LoadAsset<GameObject>("Assets/ModResources/Init/LoadingUI/HexaLoadingAnimation.prefab");
                Instantiate(loadingAnimation).transform.SetParent(transform, false);
            }

            void InitStartupResourcesDone()
            {
                DoActionThenWait(InitCoreResources, "Loading HexaModCoreResourcesBundle", InitCoreResourcesDone);
            }

            void InitCoreResources()
            {
                HexaMod.InitCoreBundle();
            }

            void InitCoreResourcesDone()
            {
                DoActionThenWait(PatchGame, "Patching Game", PatchGameDone);
            }

            void PatchGame()
            {
                Mod.Instance.harmony.PatchAll();
            }

            void PatchGameDone()
            {
                DoActionThenWait(InitGlobalState, "Init HexaMod", InitGlobalStateDone);
            }

            void InitGlobalState()
            {
                HexaMod.Init();
            }

            void InitGlobalStateDone()
            {
                DoActionThenWait(LoadGame, "Loading Game", LoadGameDone);
            }

            void LoadGame()
            {
                SceneManager.LoadSceneAsync(1);
            }

            void LoadGameDone()
            {

            }

            DoActionThenWait(InitStartupResources, "Init HexaModInitResourcesBundle", InitStartupResourcesDone);
        }

        void DoActionThenWait(Action callback, string actionText, Action onDone)
        {
            IEnumerator Wait()
            {
                yield return 0;
                callback();
                onDone();
            }

            Mod.Print(actionText);
            text.text = actionText;

            StartCoroutine(Wait());
        }
    }
}
