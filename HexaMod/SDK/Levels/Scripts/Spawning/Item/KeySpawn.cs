using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Item
{
	[UnityMigrationIdentifier("HexaMod.c21c92f2-d37a-4458-8832-c740f713e3f7")]
	public class KeySpawn : MigratableMonoBehavior
	{
		public Transform[] spots;
	}
}
