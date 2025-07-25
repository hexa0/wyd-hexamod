using HexaMod.API.Util.Migration;

namespace HexaMod.API.Util.Volume
{
	[UnityMigrationIdentifier("HexaMod.2bfb3d2d-0672-4353-bffd-8980e4b1944a")]
	public class CameraTrackedVolumeManagerBehavior<VolumeBehaviorType> : VolumeManagerBehavior<VolumeBehaviorType> where VolumeBehaviorType : VolumeBehavior
	{
		internal TrackedCamera<VolumeBehaviorType> camera;

		public override void Awake()
		{
			base.Awake();
			camera = new TrackedCamera<VolumeBehaviorType>(this);
		}

		public virtual void FixedUpdate()
		{
			camera.CheckForNewPosition();
		}
	}
}
