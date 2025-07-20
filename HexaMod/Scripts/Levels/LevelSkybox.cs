using HexaMod.Util;
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
		public Color[] ambientProbeCoefficients = new Color[9];
		public Material skybox;
		public CameraClearFlags clearFlags;
		public Light sunSource;
		public LevelSkybox dnmSkybox;

		public void Update()
		{
			if (!Application.isEditor || !useRenderSettings)
			{
				return;
			}

			skybox = RenderSettings.skybox;
			reflectionCubemap = RenderSettings.customReflection;

			for (int i = 0; i < 9; i++)
			{
				ambientProbeCoefficients[i] = new Color(RenderSettings.ambientProbe[0, i], RenderSettings.ambientProbe[1, i], RenderSettings.ambientProbe[2, i], 1f);
			}
		}

		public void Start()
		{
			if (Application.isEditor)
			{
				return;
			}

			if (dnmSkybox != null && HexaMod.HexaGlobal.networkManager.curGameMode == GameModes.GetId("daddysNightmare"))
			{
				sunSource.enabled = false;
				dnmSkybox.Start();
				return;
			}

			CurrentLevelSkybox.current = this;
			RenderSettings.skybox = skybox;
			RenderSettings.ambientMode = AmbientMode.Skybox;

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
	}
}
