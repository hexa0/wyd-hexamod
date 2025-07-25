using HexaMod.API.Util.Migration;

namespace HexaMod.API.Util.Volume
{
	[UnityMigrationIdentifier("HexaMod.b63021d5-3e03-4e8b-9f31-cdec0a42b87c")]
	public class VolumeManagerBehavior<VolumeBehaviorType> : MigratableMonoBehavior where VolumeBehaviorType : VolumeBehavior
	{
		public virtual void Awake()
		{
			volumes = GetComponentsInChildren<VolumeBehaviorType>();
		}

		internal VolumeBehaviorType[] volumes;
	}
}
