using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Item
{
	[UnityMigrationIdentifier("HexaMod.273bb0d5-2a10-4f61-ac65-1ba8f44dfdf8")]
	public class LowSpawn : MigratableMonoBehavior
	{
		public Transform[] spots;
	}
}
