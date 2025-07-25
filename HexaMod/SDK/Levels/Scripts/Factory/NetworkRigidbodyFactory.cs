using HexaMod.API.Util.Migration;
using HexaMod.Patches.Fixes;

namespace HexaMod.SDK.Levels.Scripts.Factory
{
	[UnityMigrationIdentifier("HexaMod.463b9184-0ddf-4d8f-b660-258740c57162")]
	public class NetworkRigidbodyFactory : MigratableMonoBehavior
	{
		void Start()
		{
			gameObject.AddComponent<RigidBodyReplication>();
			Destroy(this);
		}
	}
}
