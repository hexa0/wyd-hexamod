using HexaMod.Patches.Fixes;
using UnityEngine;

namespace HexaMapAssemblies
{
	public class NetworkRigidbodyFactory : MonoBehaviour
	{
		void Start()
		{
			var networkMovement = gameObject.AddComponent<RigidBodyReplication>();

			Destroy(this);
		}
	}
}
