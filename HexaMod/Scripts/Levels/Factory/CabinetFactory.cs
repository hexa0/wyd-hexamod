using HexaMod;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class CabinetFactory : MonoBehaviour
	{
		void Start()
		{
			var cabinet = gameObject.AddComponent<Cabinet2>();
			cabinet.locked = locked;
			cabinet.dadLocked = dadLocked;
			cabinet.cabOpen = openSound != null ? openSound : Assets.StaticAssets.cabinetOpen;
			cabinet.cabClose = closeSound != null ? closeSound : Assets.StaticAssets.cabinetClose;
			cabinet.lockSound = lockedSound != null ? lockedSound : Assets.StaticAssets.cabinetLocked;
			cabinet.turnAng = 92;
			cabinet.startRot = startRot;
			cabinet.rotPoint = rotPoint;
			cabinet.state = false;
			cabinet.dir = 1;
			cabinet.dir2 = 1;
		}

		public bool locked = false;
		public bool dadLocked = false;
		public GameObject openSound;
		public GameObject closeSound;
		public GameObject lockedSound;
		public Quaternion startRot;
		public Vector3 rotPoint;
	}
}
