using UnityEngine;

namespace HexaMod.Scripts.Levels.Volume
{
	public class TrackedLight<VolumeBehaviorType> : TrackedTransform<VolumeBehaviorType> where VolumeBehaviorType : VolumeBehavior
	{
		internal Light l;

		internal override void DoCull(bool culled)
		{
			// when culled use vertex lighting as it is way less expensive then the absolutely busted PPL shader that unity made
			//l.renderMode = culled ? LightRenderMode.ForceVertex : LightRenderMode.ForcePixel;
			if (l)
			{
				l.enabled = !culled;
			}
		}

		public TrackedLight(Light light, VolumeManagerBehavior<VolumeBehaviorType> manager) : base(light.transform, manager)
		{
			l = light;
		}
	}
}
