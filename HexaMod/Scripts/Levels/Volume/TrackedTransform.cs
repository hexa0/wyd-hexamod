using HexaMapAssemblies;
using UnityEngine;

namespace HexaMod.Scripts.Levels.Volume
{
	public class TrackedTransform<VolumeBehaviorType> : TrackedPoint3<VolumeBehaviorType> where VolumeBehaviorType : VolumeBehavior
	{
		readonly Transform t;
		Vector3 lp = Vector3.zero;

		public override void CheckForNewPosition()
		{
			Vector3 p = t ? t.position : Vector3.zero;

			if (!p.AlmostEquals(lp, 0.1f))
			{
				x = p.x;
				y = p.y;
				z = p.z;

				Update();
			}
		}

		public TrackedTransform(Transform transform, VolumeManagerBehavior<VolumeBehaviorType> manager) : base(manager)
		{
			t = transform;
		}
	}
}
