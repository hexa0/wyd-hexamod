using HexaMod;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class TrackedPoint3
	{
		private readonly DynamicLightCullingManager lightCullingManager;
		private bool isCulled = false;

		public float x;
		public float y;
		public float z;

		private ushort mask = 0;
		// public string maskDebugString = "";

		private void UpdateIsCulled(bool culled)
		{
			if (isCulled != culled)
			{
				isCulled = culled;
				DoCull(culled);
			}
		}

		public void CullingCheck(TrackedCamera camera)
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
			// maskDebugString = "";

			for (int i = 0; i < lightCullingManager.volumes.Length; i++)
			{
				LightCullingVolume volume = lightCullingManager.volumes[i];

				if (volume.boundingRegion.In(x, y, z))
				{
					if (volume.subRegions.Length == 0)
					{
						mask |= (ushort)(1 << i);
						// maskDebugString += volume.name + "; ";
					}
					else
					{
						foreach (BoundingBox3 subRegion in volume.subRegions)
						{
							if (subRegion.In(x, y, z))
							{
								mask |= (ushort)(1 << i);
								// maskDebugString += volume.name + ", ";
								break;
							}
						}
					}
				}
			}

			/*if (mask != 0)
			{
				maskDebugString = maskDebugString.Substring(0, maskDebugString.Length - 2);
			}*/

			if (mask == 0)
			{
				mask |= (ushort)(1 << 15);
			}
		}

		internal TrackedPoint3(DynamicLightCullingManager lightCullingManager)
		{
			this.lightCullingManager = lightCullingManager;
		}
	}

	public class TrackedTransform : TrackedPoint3
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

		public TrackedTransform(Transform transform, DynamicLightCullingManager lightCullingManager): base(lightCullingManager)
		{
			t = transform;
		}
	}

	public class TrackedGameObject : TrackedTransform
	{
		internal GameObject o;

		internal override void DoCull(bool culled)
		{
			if (o)
			{
				o.SetActive(!culled);
			}
		}

		public TrackedGameObject(GameObject gameObject, DynamicLightCullingManager lightCullingManager) : base(gameObject.transform, lightCullingManager)
		{
			o = gameObject;
		}
	}

	public class TrackedLight : TrackedTransform
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

		public TrackedLight(Light light, DynamicLightCullingManager lightCullingManager) : base(light.transform, lightCullingManager)
		{
			l = light;
		}
	}

	public class TrackedCamera : TrackedPoint3
	{
		public override void CheckForNewPosition()
		{
			Vector3 p = Camera.current == null ? Vector3.zero : Camera.current.transform.position;

			x = p.x;
			y = p.y;
			z = p.z;

			Update();
		}

		public TrackedCamera(DynamicLightCullingManager lightCullingManager) : base(lightCullingManager) { }
	}

	public class BoundingBox3
	{
		// minimum
		public readonly float xn;
		public readonly float yn;
		public readonly float zn;

		// maximum
		public readonly float xp;
		public readonly float yp;
		public readonly float zp;

		public bool In(float x, float y, float z)
		{
			return (x >= xn) && (y >= yn) && (z >= zn) && (x <= xp) && (y <= yp) && (z <= zp);
		}

		public bool In(Vector3 position)
		{
			return In(position.x, position.y, position.z);
		}

		public BoundingBox3(float xn, float yn, float zn, float xp, float yp, float zp)
		{
			this.xn = xn;
			this.yn = yn;
			this.zn = zn;
			this.xp = xp;
			this.yp = yp;
			this.zp = zp;
		}
	}

	public class LightCullingVolume : MonoBehaviour
	{
		public BoundingBox3 boundingRegion;
		public BoundingBox3[] subRegions;

		void Awake()
		{
			float minX = float.PositiveInfinity;
			float minY = float.PositiveInfinity;
			float minZ = float.PositiveInfinity;

			float maxX = float.NegativeInfinity;
			float maxY = float.NegativeInfinity;
			float maxZ = float.NegativeInfinity;

			BoxCollider[] triggers = GetComponentsInChildren<BoxCollider>();
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

				Destroy(trigger.gameObject);

				subRegions[i++] = region;
			}

			if (subRegions.Length == 1)
			{
				subRegions = new BoundingBox3[0];
			}

			boundingRegion = new BoundingBox3(
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
