using UnityEngine;
using UnityEngine.Rendering;

namespace HexaMapAssemblies
{
	public static class CurrentLevelSkybox
	{
		public static LevelSkybox current;
	}

	[ExecuteInEditMode]
	public class LevelSkybox : MonoBehaviour
	{
		public bool useRenderSettings = true;

		public Cubemap reflectionCubemap;
		public AmbientMode ambientMode = AmbientMode.Skybox;
		public Color[] ambientProbeCoefficients = new Color[9];
		public Color ambientGroundColor;
		public Color ambientEquatorColor;
		public Color ambientSkyColor;
		public Material skybox;
		public CameraClearFlags clearFlags;
		public Light sunSource;
		public LevelSkybox dnmSkybox;

		void UpdateFromRenderSettings()
		{
			skybox = RenderSettings.skybox;
			reflectionCubemap = RenderSettings.customReflection;
			ambientMode = RenderSettings.ambientMode;
			ambientGroundColor = RenderSettings.ambientGroundColor;
			ambientEquatorColor = RenderSettings.ambientEquatorColor;
			ambientSkyColor = RenderSettings.ambientSkyColor;

			for (int i = 0; i < 9; i++)
			{
				ambientProbeCoefficients[i] = new Color(RenderSettings.ambientProbe[0, i], RenderSettings.ambientProbe[1, i], RenderSettings.ambientProbe[2, i], 1f);
			}
		}

		void SetupLevelLighting()
		{
			if (dnmSkybox != null && HexaMod.HexaGlobal.networkManager.curGameMode == HexaMod.Util.GameModes.GetId("daddysNightmare"))
			{
				sunSource.enabled = false;
				dnmSkybox.Start();
				return;
			}

			CurrentLevelSkybox.current = this;
			RenderSettings.skybox = skybox;
			RenderSettings.ambientMode = ambientMode;
			RenderSettings.ambientGroundColor = ambientGroundColor;
			RenderSettings.ambientEquatorColor = ambientEquatorColor;
			RenderSettings.ambientSkyColor = ambientSkyColor;

			if (ambientProbeCoefficients != null)
			{
				SphericalHarmonicsL2 ambientProbe = new SphericalHarmonicsL2();

				for (int i = 0; i < 9; i++)
				{
					ambientProbe[0, i] = ambientProbeCoefficients[i].r;
					ambientProbe[1, i] = ambientProbeCoefficients[i].g;
					ambientProbe[2, i] = ambientProbeCoefficients[i].b;
				}

				RenderSettings.ambientProbe = ambientProbe;
			}

			RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
			RenderSettings.customReflection = reflectionCubemap;

			if (sunSource)
			{
				sunSource.enabled = true;
				RenderSettings.sun = sunSource;
			}

			LightHolder lightHolder = FindObjectOfType<LightHolder>();

			lightHolder.sunLight = RenderSettings.sun;
			lightHolder.Start();
		}

		void Update()
		{
			if (Application.isEditor && useRenderSettings)
			{
				UpdateFromRenderSettings();
			}
		}

		void Start()
		{
			if (!Application.isEditor)
			{
				SetupLevelLighting();
			}
		}
	}
}
