using HexaMod;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class DoorFactory : MonoBehaviour
	{
		public enum OpenDirection
		{
			Inwards,
			Outwards
		}
		void Start()
		{
			var door = gameObject.AddComponent<Door>();

			door.tag = "Open";
			door.locked = locked;
			door.oneWay = oneWay;
			door.rotPoint = rotPoint;
			door.startRot = startRot;
			door.targetRot = targetRot;
			door.useX = useX;
			door.useY = useY;
			if (openDirection == OpenDirection.Inwards)
			{
				door.dir = 1;
				door.dir2 = 1;
			}
			else
			{
				door.dir = -1;
				door.dir2 = -1;
			}

			door.openSound = openSound != null ? openSound : Assets.StaticAssets.doorOpen;
			door.closeSound = closeSound != null ? closeSound : Assets.StaticAssets.doorClose;
			door.occPortal = occPortal;
		}

		public bool locked = false;
		public bool oneWay = false;
		public GameObject openSound;
		public GameObject closeSound;
		public Vector3 rotPoint;
		public Quaternion startRot;
		public Quaternion targetRot;
		public bool useX = false;
		public bool useY = true;
		public OcclusionPortal occPortal;
		public OpenDirection openDirection = OpenDirection.Inwards;
	}
}
