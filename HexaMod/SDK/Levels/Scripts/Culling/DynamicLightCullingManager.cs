using System.Collections.Generic;
using HexaMod.API.Util.Migration;
using HexaMod.API.Util.Volume;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Culling
{
	[UnityMigrationIdentifier("HexaMod.337c4356-12e0-425f-bafa-ff8e62681f7f")]
	public class DynamicLightCullingManager : CameraTrackedVolumeManagerBehavior<LightCullingVolume>
	{
		internal List<TrackedPoint3<LightCullingVolume>> trackers = new List<TrackedPoint3<LightCullingVolume>>();

		public override void Awake()
		{
			base.Awake();
		}

		void Start()
		{
			trackers.Clear();

			foreach (Light light in FindObjectsOfType<Light>())
			{
				if (light.type != LightType.Directional && light.gameObject.activeSelf)
				{
					trackers.Add(
						new TrackedLight<LightCullingVolume>(light, this)
					);
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			foreach (TrackedPoint3<LightCullingVolume> tracker in trackers)
			{
				tracker.CheckForNewPosition();
				tracker.CullingCheck(camera);
			}
		}
	}
}