using HexaMod.Scripts.Levels.Volume;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class LightingParamsManager : CameraTrackedVolumeManagerBehavior<LightingParamsVolume>
	{
		[Header("General Parameters")]

		public bool smoothed = true;
		public float lerpFactor = 10f;

		[Header("Lighting Parameters")]

		[SerializeField]
		public LightingParamSettings[] settings = new LightingParamSettings[] {};

		float targetSkyLightingIntensity = 1f;
		float targetSkyReflectionsIntensity = 1f;

		float currentSkyLightingIntensity = 1f;
		float currentSkyReflectionsIntensity = 1f;

		public override void Awake()
		{
			base.Awake();

			LightingParamSettings defaultLightingParams = LightingParamSettings.GetSettings(settings);

			currentSkyLightingIntensity = defaultLightingParams.skyLightingIntensity;
			currentSkyReflectionsIntensity = defaultLightingParams.skyReflectionsIntensity;
		}

		void Update()
		{
			if (smoothed)
			{
				currentSkyLightingIntensity = Mathf.Lerp(currentSkyLightingIntensity, targetSkyLightingIntensity, Time.deltaTime * lerpFactor);
				currentSkyReflectionsIntensity = Mathf.Lerp(currentSkyReflectionsIntensity, targetSkyReflectionsIntensity, Time.deltaTime * lerpFactor);
			}
			else
			{
				currentSkyLightingIntensity = targetSkyLightingIntensity;
				currentSkyReflectionsIntensity = targetSkyReflectionsIntensity;
			}

			RenderSettings.ambientIntensity = currentSkyLightingIntensity;
			RenderSettings.reflectionIntensity = currentSkyReflectionsIntensity;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			LightingParamsVolume highestPriorityVolume = null;

			for (int i = 0; i < volumes.Length; i++)
			{
				if ((camera.Mask & (1 << i)) != 0)
				{
					if (highestPriorityVolume == null || volumes[i].priority > highestPriorityVolume.priority)
					{
						highestPriorityVolume = volumes[i];
					}
				}
			}

			if (!highestPriorityVolume)
			{
				LightingParamSettings defaultLightingParams = LightingParamSettings.GetSettings(settings);

				targetSkyLightingIntensity = defaultLightingParams.skyLightingIntensity;
				targetSkyReflectionsIntensity = defaultLightingParams.skyReflectionsIntensity;
			}
			else
			{
				LightingParamSettings lightingParams = LightingParamSettings.GetSettings(highestPriorityVolume.settings);

				targetSkyLightingIntensity = lightingParams.skyLightingIntensity;
				targetSkyReflectionsIntensity = lightingParams.skyReflectionsIntensity;
			}
		}
	}
}
