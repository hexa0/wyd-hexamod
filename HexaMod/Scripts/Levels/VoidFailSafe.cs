using HexaMapAssemblies;
using UnityEngine;

namespace HexaMod.Scripts
{
	public class VoidFailSafe : MonoBehaviour
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
