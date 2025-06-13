using System.Collections.Generic;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class DynamicLightCullingManager : MonoBehaviour
	{
		internal LightCullingVolume[] volumes;
		internal List<TrackedPoint3> trackers = new List<TrackedPoint3>();
		internal TrackedCamera camera;

		void Awake()
		{
			volumes = GetComponentsInChildren<LightCullingVolume>();
			camera = new TrackedCamera(this);
		}

		void Start()
		{
			trackers.Clear();

			foreach (Light light in FindObjectsOfType<Light>())
			{
				if (light.type != LightType.Directional && light.gameObject.activeSelf)
				{
					trackers.Add(
						new TrackedLight(light, this)
					);
				}
			}
		}

		void FixedUpdate()
		{
			camera.CheckForNewPosition();

			foreach (TrackedPoint3 tracker in trackers)
			{
				tracker.CheckForNewPosition();
				tracker.CullingCheck(camera);
			}
		}
	}
}