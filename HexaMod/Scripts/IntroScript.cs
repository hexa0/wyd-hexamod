using System;
using System.Collections;
using System.Reflection;
using HexaMod.UI;
using HexaMod.UI.Element.HexaMod.Loading;
using HexaMod.Voice;
using HexaMod.Voice.Script;
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
			InitHexaMod();
		}

		public void Start()
		{

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
