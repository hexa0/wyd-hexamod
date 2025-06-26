using System;
using System.Collections;
using System.Reflection;
using HexaMod.UI;
using HexaMod.UI.Element.HexaMod.Loading;
using HexaMod.Voice;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod.Scripts
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

			// begin the startup routine
			if (!VoiceChat.testMode)
			{
				InitHexaMod();
			}
			else
			{
				SetLoadingText("Voice Chat Test Mode Enabled");
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

		void SetLoadingText(string loadingText) => HexaMenus.startupScreen.loadingText.SetText(loadingText);

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

			VoiceChat.Init();

			SetLoadingText("Loading HexaModInitResourcesBundle\n(Init)");
			yield return 0;
			HexaGlobal.InitStartupBundle();
			SetLoadingText("Loading HexaModInitResourcesBundle\n(Font)");
			var fontLoadRequest = HexaGlobal.startupBundle.LoadAssetAsync<Font>("Assets/ModResources/Init/Font/osd.ttf");
			var loadingAnimationRequest = HexaGlobal.startupBundle.LoadAssetAsync<GameObject>("Assets/ModResources/Init/LoadingUI/HexaLoadingAnimation.prefab");
			yield return fontLoadRequest;
			LoadingText.loadingFont = fontLoadRequest.asset as Font;
			SetLoadingText("Loading HexaModInitResourcesBundle\n(Loading Animation)");
			yield return loadingAnimationRequest;
			LoadingAnimation.loadingAnimation = loadingAnimationRequest.asset as GameObject;
			SetLoadingText("Loading HexaModCoreResourcesBundle");
			yield return 0;
			HexaGlobal.InitCoreBundle();
			SetLoadingText("Patching Game");
			yield return 0;
			Mod.instance.harmony.PatchAll(Assembly.GetExecutingAssembly());
			SetLoadingText("Init HexaMod");
			yield return 0;
			HexaGlobal.Init();
			yield return 0;
			HexaMenus.startupScreen.loadingText.enableLogging = false;
			SetLoadingText($"Loading Asset Bundles\n(?/?)");
			while (!Assets.loadedAssets)
			{
				SetLoadingText($"Loading Asset Bundles\n({Assets.loadedBundles}/{Assets.bundlesToLoad})");
				yield return 0;
			}
			SetLoadingText("Start Scene Load");
			yield return 0;
			sceneLoadOperation = SceneManager.LoadSceneAsync(1);
			sceneLoadOperation.allowSceneActivation = false;
			SetLoadingText($"Loading Game\n(0%)");
			yield return 0;
			Application.targetFrameRate = 0;
			sceneLoadOperation.allowSceneActivation = true;
			while (!sceneLoadOperation.isDone)
			{
				SetLoadingText($"Loading Game\n({Math.Round(sceneLoadOperation.progress * 100, 2)}%)");
				yield return 0;
			}
		}
	}
}
