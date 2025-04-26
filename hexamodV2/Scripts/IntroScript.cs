using System;
using System.Collections;
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
            if (!AudioInput.testMode)
            {
                StartCoroutine(InitHexaModAfterFrame());
            }
        }

        public void Start()
        {
            if (AudioInput.testMode)
            {
                AudioSource mic = gameObject.AddComponent<AudioSource>();
                mic.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
                mic.spatialBlend = 0f;
                mic.spatialize = false;
                mic.spread = 1f;
                mic.bypassEffects = true;
                mic.loop = true;

                gameObject.AddComponent<MicEmitter>();
            }
        }

        private GameObject textObject;
        private UnityEngine.UI.Text text;

        void Update()
        {
            //Camera.current.backgroundColor = new Color(0.29019607843137254901960784313725f, 0.18431372549019607843137254901961f, 0.21176470588235294117647058823529f);
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
/*                AudioSource elevtorMusic = gameObject.AddComponent<AudioSource>();
                elevtorMusic.clip = HexaMod.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/ElevatorWait.wav");
                elevtorMusic.volume = (PlayerPrefs.GetFloat("MasterVolume", 1f) * PlayerPrefs.GetFloat("MusicVolume", 1f)) / 2f;
                elevtorMusic.spatialBlend = 0f;
                elevtorMusic.spatialize = false;
                elevtorMusic.spread = 1f;
                elevtorMusic.bypassEffects = true;
                elevtorMusic.loop = true;
                elevtorMusic.Play();*/
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
