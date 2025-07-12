using UnityEngine;

namespace HexaMod.Scripts.Levels.Volume
{
	public static class VolumeUtils
	{
		public static void ProcessVolumes(GameObject volume, out BoundingBox3 subRegionBoundingBox, out BoundingBox3[] subRegions)
		{
			float minX = float.PositiveInfinity;
			float minY = float.PositiveInfinity;
			float minZ = float.PositiveInfinity;

			float maxX = float.NegativeInfinity;
			float maxY = float.NegativeInfinity;
			float maxZ = float.NegativeInfinity;

			BoxCollider[] triggers = volume.GetComponentsInChildren<BoxCollider>();
			subRegions = new BoundingBox3[triggers.Length];

			int i = 0;

			foreach (BoxCollider trigger in triggers)
			{
				BoundingBox3 region = new BoundingBox3(
					trigger.center.x - (trigger.size.x * 0.5f),
					trigger.center.y - (trigger.size.y * 0.5f),
					trigger.center.z - (trigger.size.z * 0.5f),
					trigger.center.x + (trigger.size.x * 0.5f),
					trigger.center.y + (trigger.size.y * 0.5f),
					trigger.center.z + (trigger.size.z * 0.5f)
				);

				if (region.xn < minX)
				{
					minX = region.xn;
				}

				if (region.yn < minY)
				{
					minY = region.yn;
				}

				if (region.zn < minZ)
				{
					minZ = region.zn;
				}

				if (region.xp > maxX)
				{
					maxX = region.xp;
				}

				if (region.yp > maxY)
				{
					maxY = region.yp;
				}

				if (region.zp > maxZ)
				{
					maxZ = region.zp;
				}

				Object.Destroy(trigger.gameObject);

				subRegions[i++] = region;
			}

			if (subRegions.Length == 1)
			{
				subRegions = new BoundingBox3[0];
			}

			subRegionBoundingBox = new BoundingBox3(
				minX,
				minY,
				minZ,
				maxX,
				maxY,
				maxZ
			);
		}
	}
}
