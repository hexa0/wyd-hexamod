using HexaMod.Util;
using UnityEngine;

namespace HexaMapAssemblies
{
	public static class CurrentLevelSkybox
	{
		public static LevelSkybox current;
	}

	public class LevelSkybox : MonoBehaviour
	{
		public Cubemap cubemap;
		public Material skybox;
		public CameraClearFlags clearFlags;
		public Light sunSource;
		public LevelSkybox dnmSkybox;

		public void Start()
		{
			if (dnmSkybox != null && HexaMod.HexaMod.networkManager.curGameMode == GameModes.named["daddysNightmare"].id)
			{
				sunSource.enabled = false;
				dnmSkybox.Start();
				return;
			}

			CurrentLevelSkybox.current = this;
			RenderSettings.skybox = skybox;
			RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
			RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Skybox;
			RenderSettings.customReflection = cubemap;

			if (sunSource)
			{
				sunSource.enabled = true;
				RenderSettings.sun = sunSource;
			}

			LightHolder lightHolder = FindObjectOfType<LightHolder>();

			lightHolder.sunLight = RenderSettings.sun;
			// lightHolder.ToggleLights(PlayerPrefs.GetInt("UseDL", 1) == 1);
			// lightHolder.ToggleShadows(PlayerPrefs.GetInt("UseShadows", 0) == 1);
			lightHolder.Start();
		}
	}
}
