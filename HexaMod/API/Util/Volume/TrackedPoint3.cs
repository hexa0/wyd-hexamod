namespace HexaMod.API.Util.Volume
{
	public class TrackedPoint3<VolumeBehaviorType> where VolumeBehaviorType : VolumeBehavior
	{
		private readonly VolumeManagerBehavior<VolumeBehaviorType> manager;
		private bool isCulled = false;

		public float x;
		public float y;
		public float z;

		private ushort mask = 0;

		public ushort Mask {
			get => mask;
		}

		private void UpdateIsCulled(bool culled)
		{
			if (isCulled != culled)
			{
				isCulled = culled;
				DoCull(culled);
			}
		}

		public void CullingCheck(TrackedCamera<VolumeBehaviorType> camera)
		{
			bool rendered = (camera.mask & mask) != 0;

			UpdateIsCulled(!rendered);
		}

		internal virtual void DoCull(bool culled)
		{
			throw new System.Exception("DoCull(bool culled) wasn't implemented");
		}

		public virtual void CheckForNewPosition()
		{
			throw new System.Exception("CheckForNewPosition() wasn't implemented");
		}

		internal void Update()
		{
			mask = 0;

			for (int i = 0; i < manager.volumes.Length; i++)
			{
				VolumeBehavior volume = manager.volumes[i];

				if (volume.boundingRegion.In(x, y, z))
				{
					if (volume.subRegions.Length == 0)
					{
						mask |= (ushort)(1 << i);
					}
					else
					{
						foreach (BoundingBox3 subRegion in volume.subRegions)
						{
							if (subRegion.In(x, y, z))
							{
								mask |= (ushort)(1 << i);
								break;
							}
						}
					}
				}
			}

			if (mask == 0)
			{
				mask |= 1 << 15;
			}
		}

		internal TrackedPoint3(VolumeManagerBehavior<VolumeBehaviorType> manager)
		{
			this.manager = manager;
		}
	}
}
