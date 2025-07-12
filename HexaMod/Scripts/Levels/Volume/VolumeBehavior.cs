using UnityEngine;

namespace HexaMod.Scripts.Levels.Volume
{
	public class VolumeBehavior : MonoBehaviour
	{
		public BoundingBox3 boundingRegion;
		public BoundingBox3[] subRegions;

		void Awake()
		{
			VolumeUtils.ProcessVolumes(gameObject, out boundingRegion, out subRegions);
		}
	}
}
