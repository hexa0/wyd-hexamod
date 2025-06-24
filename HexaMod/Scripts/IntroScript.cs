using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using HexaMod.UI;
using HexaMod.UI.Element.HexaMod.Loading;
using HexaMod.Voice;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod
{
	public class IntroScript : MonoBehaviour
	{
		public void Awake()
		{
			// lower fps for the loading screen
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;

			// init hexa menus
			HexaMenus.Init();

			// setup scene loaded hook
			SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
			{
				HexaMenus.startupScreen.fader.fadeState = false;
				ActionText("Loaded Game");
				HexaGlobal.OnGameSceneStart();
			};

			// begin the startup routine
			if (!VoiceChat.testMode)
			{
				InitHexaMod();
			}
			else
			{
				ActionText("Voice Chat Test Mode Enabled");
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

				VoiceChat.ConnectToRelay("127.0.0.1");
				VoiceChat.JoinVoiceRoom("VoiceChat.testMode room");
			}
		}

		string lastActionText = string.Empty;
		void ActionText(string actionText)
		{
			if (lastActionText != actionText)
			{

				Mod.Print(actionText);
				HexaMenus.startupScreen.loadingText.SetText(actionText);
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
			Application.backgroundLoadingPriority = ThreadPriority.High;
			QualitySettings.pixelLightCount = 15;
			QualitySettings.realtimeReflectionProbes = false;
			QualitySettings.shadows = ShadowQuality.HardOnly;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			QualitySettings.shadowDistance *= 0.75f;
			QualitySettings.shadowProjection = ShadowProjection.CloseFit;

			ActionText("Loading HexaModInitResourcesBundle\n(Init)");
			yield return 0;
			HexaGlobal.InitStartupBundle();
			ActionText("Loading HexaModInitResourcesBundle\n(Font)");
			var fontLoadRequest = HexaGlobal.startupBundle.LoadAssetAsync<Font>("Assets/ModResources/Init/Font/osd.ttf");
			yield return fontLoadRequest;
			LoadingText.loadingFont = fontLoadRequest.asset as Font;
			ActionText("Loading HexaModInitResourcesBundle\n(Loading Animation)");
			var loadingAnimationRequest = HexaGlobal.startupBundle.LoadAssetAsync<GameObject>("Assets/ModResources/Init/LoadingUI/HexaLoadingAnimation.prefab");
			yield return loadingAnimationRequest;
			LoadingAnimation.loadingAnimation = loadingAnimationRequest.asset as GameObject;
			if (!Environment.GetCommandLineArgs().Contains("SkipTranscodeProcessStart"))
			{
				ActionText("Init VoiceChat\n(Transcode Process)");
				yield return 0;
				VoiceChat.Init();
			}
			else
			{
				ActionText("Init VoiceChat\n(Without Transcode Process)");
				yield return 0;
				VoiceChat.InitWithoutTranscodeProcess();
			}
			ActionText("Loading HexaModCoreResourcesBundle");
			yield return 0;
			HexaGlobal.InitCoreBundle();
			ActionText("Patching Game");
			yield return 0;
			Mod.instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
			ActionText("Init HexaMod");
			yield return 0;
			HexaGlobal.Init();
			yield return 0;
			ActionText($"Loading Asset Bundles\n(?/?)");
			while (!Assets.loadedAssets)
			{
				ActionText($"Loading Asset Bundles\n({Assets.loadedBundles}/{Assets.bundlesToLoad})");
				yield return 0;
			}
			ActionText("Start Scene Load");
			yield return 0;
			sceneLoadOperation = SceneManager.LoadSceneAsync(1);
			sceneLoadOperation.allowSceneActivation = false;
			ActionText("Init VoiceChat\n(Transcode Connection)\n(Attempt 0)");
			yield return 0;
			int attempts = 0;
			while (!VoiceChat.transcodeServerReady)
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
					if (!VoiceChat.transcodeServerReady)
					{
						VoiceChat.SendTranscodeServerHandshake();
					}
					else
					{
						Mod.Print("Cancelled Handshake, Already Completed Handshake");
					}
				}
				catch (Exception e)
				{
					Mod.Warn(e);
				}

				yield return 0;

				attempts++;
			}
			ActionText($"Loading Game\n(0%)");
			yield return 0;
			Application.targetFrameRate = 0;
			sceneLoadOperation.allowSceneActivation = true;
			while (!sceneLoadOperation.isDone)
			{
				ActionText($"Loading Game\n({Math.Round(sceneLoadOperation.progress * 100, 2)}%)");
				yield return 0;
			}
		}
	}
}
