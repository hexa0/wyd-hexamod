using HexaMod.API.Util.Migration;

namespace HexaMod.API.Util.Volume
{
	[UnityMigrationIdentifier("HexaMod.473fddd5-4419-457f-8455-f97db1da36f2")]
	public class VolumeBehavior : MigratableMonoBehavior
	{
		public BoundingBox3 boundingRegion;
		public BoundingBox3[] subRegions;

		void Awake()
		{
			VolumeUtils.ProcessVolumes(gameObject, out boundingRegion, out subRegions);
		}
	}
}
