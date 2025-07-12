namespace HexaMod.Scripts.Levels.Volume
{
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
