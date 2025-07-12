using UnityEngine;

namespace HexaMod.Scripts.Levels.Volume
{
	public class VolumeManagerBehavior<VolumeBehaviorType> : MonoBehaviour where VolumeBehaviorType : VolumeBehavior
	{
		public virtual void Awake()
		{
			volumes = GetComponentsInChildren<VolumeBehaviorType>();
		}

		internal VolumeBehaviorType[] volumes;
	}
}
