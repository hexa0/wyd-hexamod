using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HexaMod.API.UI;
using HexaMod.API.Util.Patching;
using HexaMod.API.Voice;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod.Scripts.Initialization
{
	public class IntroScript : MonoBehaviour
	{
		public void Awake()
		{
			// lower fps for the loading screen
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;

			HexaMenus.Init();

			InitHexaMod();
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

		Type[] allSharedPatches;
		Type[] allVanillaPatches;
		Type[] allModdedPatches;

		public struct OptionalPatchStruct
		{
			public OptionalPatch metadata;
			public Type patchClass;
			public ModPreference<bool> patchPreference;
		}

		readonly Dictionary<string, OptionalPatchStruct> optionalPatches = new Dictionary<string, OptionalPatchStruct>();

		IEnumerator DoPatches(bool vanillaMode = false)
		{
			int appliedPatchesCounter;
			int toPatch;

			SetLoadingText("Indexing Patches");
			yield return new WaitForEndOfFrame();

			{
				List<Type> allSharedPatchesList = new List<Type>();
				List<Type> allVanillaPatchesList = new List<Type>();
				List<Type> allModdedPatchesList = new List<Type>();

				foreach (Type type in AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()))
				{
					if (type.IsDefined(typeof(VanillaPatch), false))
					{
						allVanillaPatchesList.Add(type);
					}
					else if (type.IsDefined(typeof(ModdedPatch), false))
					{
						allModdedPatchesList.Add(type);
					}
					else if (type.IsDefined(typeof(HarmonyPatch), false))
					{
						allSharedPatchesList.Add(type);
					}
				}

				allSharedPatches = allSharedPatchesList.ToArray();
				allVanillaPatches = allVanillaPatchesList.ToArray();
				allModdedPatches = allModdedPatchesList.ToArray();
			}

			appliedPatchesCounter = 0;
			toPatch = vanillaMode ? allSharedPatches.Length + allVanillaPatches.Length : allSharedPatches.Length + allModdedPatches.Length;

			void PatchingGameStatus()
			{
				string appliedPatchesText = appliedPatchesCounter.ToString();
				string toPatchText = toPatch.ToString();
				SetLoadingText($"Patching Game\n({appliedPatchesText.PadLeft(toPatchText.Length - appliedPatchesText.Length, '0')}/{toPatchText})");
			}

			void ApplyPatch(Type type)
			{
				if (type.IsDefined(typeof(OptionalPatch), false))
				{
					OptionalPatch metadataAttribute = Attribute.GetCustomAttribute(type, typeof(OptionalPatch), false) as OptionalPatch;
					ModPreference<bool> preference = new ModPreference<bool>($"OptionalPatch_{metadataAttribute.Id}", metadataAttribute.OnByDefault);
					
					OptionalPatchStruct optionalPatchStruct = new OptionalPatchStruct()
					{
						metadata = metadataAttribute,
						patchClass = type,
						patchPreference = preference
					};

					optionalPatches.Add(metadataAttribute.Id, optionalPatchStruct);

					if (preference.Value)
					{
						Mod.instance.harmony.CreateClassProcessor(type).Patch();
					}
				}
				else
				{
					Mod.instance.harmony.CreateClassProcessor(type).Patch();
				}

				appliedPatchesCounter++;

				PatchingGameStatus();
			}

			PatchingGameStatus();
			yield return new WaitForEndOfFrame();

			foreach (Type type in allSharedPatches)
			{
				ApplyPatch(type);

				if (appliedPatchesCounter != 0)
				{
					if ((appliedPatchesCounter % 25) == 0)
					{
						yield return new WaitForEndOfFrame();
					}
				}
			}

			if (vanillaMode)
			{
				foreach (Type type in allVanillaPatches)
				{
					ApplyPatch(type);

					if (appliedPatchesCounter != 0)
					{
						if ((appliedPatchesCounter % 25) == 0)
						{
							yield return new WaitForEndOfFrame();
						}
					}
				}
			}
			else
			{
				foreach (Type type in allModdedPatches)
				{
					ApplyPatch(type);

					if (appliedPatchesCounter != 0)
					{
						if ((appliedPatchesCounter % 25) == 0)
						{
							yield return new WaitForEndOfFrame();
						}
					}
				}
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

			SetLoadingText("Init HexaMod");
			yield return new WaitForEndOfFrame();
			HexaGlobal.Init();
			yield return new WaitForEndOfFrame();
			HexaMenus.startupScreen.loadingText.enableLogging = false;
			SetLoadingText($"Loading Asset Bundles\n(?/?)");
			while (!Assets.loadedAssets)
			{
				SetLoadingText($"Loading Asset Bundles\n({Assets.loadedBundles}/{Assets.bundlesToLoad})");
				yield return new WaitForEndOfFrame();
			}
			yield return DoPatches(HexaGlobal.inVanillaMode);
			SetLoadingText("Start Scene Load");
			yield return new WaitForEndOfFrame();
			sceneLoadOperation = SceneManager.LoadSceneAsync(1);
			sceneLoadOperation.allowSceneActivation = false;
			SetLoadingText($"Loading Game\n(0%)");
			yield return new WaitForEndOfFrame();
			Application.targetFrameRate = 0;
			sceneLoadOperation.allowSceneActivation = true;
			while (!sceneLoadOperation.isDone)
			{
				SetLoadingText($"Loading Game\n({Math.Round(sceneLoadOperation.progress * 100, 2)}%)");
				yield return new WaitForEndOfFrame();
			}
		}
	}
}
