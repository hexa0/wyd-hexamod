using UnityEngine;

namespace HexaMod.Scripts.PunRpcExtensions
{
	public class DoorSoundRPC : MonoBehaviour
	{
		void Start()
		{
			door = GetComponent<Door>();
			netView = GetComponent<PhotonView>();
		}

		public void MakeSound(string type)
		{
			byte asByte = (byte)(type == "Open" ? 0x01 : 0x00);
			netView.RPC("RPCMakeSound", PhotonTargets.All, asByte);
		}

		[PunRPC]
		public void RPCMakeSound(byte type)
		{
			Instantiate(type == 0x01 ? door.openSound : door.closeSound, transform.position, transform.rotation);
		}

		Door door;
		PhotonView netView;
	}
}
