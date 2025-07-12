using HexaMod.Scripts.Levels.Volume;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class LightingParamsManager : CameraTrackedVolumeManagerBehavior<LightingParamsVolume>
	{
		public bool smoothed = true;
		public float lerpFactor = 10f;

		[Tooltip("The ambient light color multiplier")]
		[Range(0f, 8f)]
		public float defaultSkyLightingIntensity = 1f;
		[Tooltip("The reflection light color multiplier")]
		[Range(0f, 1f)]
		public float defaultSkyReflectionsIntensity = 1f;

		float targetSkyLightingIntensity = 1f;
		float targetSkyReflectionsIntensity = 1f;

		float currentSkyLightingIntensity = 1f;
		float currentSkyReflectionsIntensity = 1f;

		public override void Awake()
		{
			base.Awake();
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
				targetSkyLightingIntensity = defaultSkyLightingIntensity;
				targetSkyReflectionsIntensity = defaultSkyReflectionsIntensity;
			}
			else
			{
				targetSkyLightingIntensity = highestPriorityVolume.skyLightingIntensity;
				targetSkyReflectionsIntensity = highestPriorityVolume.skyReflectionsIntensity;
			}
		}
	}
}
