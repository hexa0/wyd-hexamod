using HarmonyLib;

namespace HexaMod
{
	public class PickUpRpcExtension : Photon.MonoBehaviour
	{
		[PunRPC]
		public void SetGrabbedDistance(float distance, PhotonMessageInfo info)
		{
			if (!photonView.isMine)
			{
				PickUp pickup = gameObject.GetComponent<PickUp>();
				Traverse.Create(pickup).Field("grabbedDis").SetValue(distance);
			}
		}
	}
}
