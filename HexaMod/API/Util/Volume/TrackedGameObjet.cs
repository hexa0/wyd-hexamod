using UnityEngine;

namespace HexaMod.API.Util.Volume
{
	public class TrackedGameObject<VolumeBehaviorType> : TrackedTransform<VolumeBehaviorType> where VolumeBehaviorType : VolumeBehavior
	{
		internal GameObject o;

		internal override void DoCull(bool culled)
		{
			if (o)
			{
				o.SetActive(!culled);
			}
		}

		public TrackedGameObject(GameObject gameObject, VolumeManagerBehavior<VolumeBehaviorType> manager) : base(gameObject.transform, manager)
		{
			o = gameObject;
		}
	}
}
