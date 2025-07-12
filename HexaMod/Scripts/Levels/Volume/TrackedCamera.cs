using UnityEngine;

namespace HexaMod.Scripts.Levels.Volume
{
	public class TrackedCamera<VolumeBehaviorType> : TrackedPoint3<VolumeBehaviorType> where VolumeBehaviorType : VolumeBehavior
	{
		public override void CheckForNewPosition()
		{
			Vector3 p = Camera.current == null ? Vector3.zero : Camera.current.transform.position;

			x = p.x;
			y = p.y;
			z = p.z;

			Update();
		}

		public TrackedCamera(VolumeManagerBehavior<VolumeBehaviorType> manager) : base(manager) { }
	}
}
