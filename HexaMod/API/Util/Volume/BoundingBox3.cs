using UnityEngine;

namespace HexaMod.API.Util.Volume
{
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
			return x >= xn && y >= yn && z >= zn && x <= xp && y <= yp && z <= zp;
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
}
