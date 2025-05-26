using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	internal class WallClipFixBehavior : MonoBehaviour
	{
		public bool crouchingTarget = false;
		public bool proningTarget = false;
		public bool crouching = false;
		public bool proning = false;

		public float camStartX = 0f;
		public float camStartY = 0f;
		public float camStartZ = 0f;

		public float crouchHeight = 0f;
		public Vector3 cameraOffset = new Vector3();
		public float crouchMult = 0.5f;
		public float proneMult = 1f / 2.8f;
	}
}
