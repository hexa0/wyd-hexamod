using HarmonyLib;
using UnityEngine;

namespace HexaMod
{
	public class PickUpRpcExtension : MonoBehaviour
	{
		[PunRPC]
		public void SetGrabbedDistance(float distance)
		{
			PickUp pickup = gameObject.GetComponent<PickUp>();
			Traverse.Create(pickup).Field("grabbedDis").SetValue(distance);
		}
	}
}
