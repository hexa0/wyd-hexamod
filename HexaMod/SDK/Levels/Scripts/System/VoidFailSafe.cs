using HexaMod.API.Util.Migration;
using HexaMod.Scripts.Util;
using HexaMod.SDK.Levels.Scripts.Factory;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.System
{
	[UnityMigrationIdentifier("HexaMod.acc73343-61ee-463d-845c-a0fdffa8812d")]
	public class VoidFailSafe : MigratableMonoBehavior
	{
		public Vector3 voidSpot = Vector3.zero;
		public Vector3 respawnSpot = Vector3.up;
		public AudioClip respawnSound;

		private NetworkedSoundBehavior netSound;

		void Start()
		{
			GlobalPhotonFactory.Register(gameObject);
			netSound = gameObject.AddComponent<NetworkedSoundBehavior>();
			netSound.RegisterSound(respawnSound);
		}

		public void Teleport(Transform toTeleport)
		{
			toTeleport.position = respawnSpot;
			netSound.Play(respawnSound);
		}
	}
}
