using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Item
{
	[UnityMigrationIdentifier("HexaMod.12535dd1-ebe8-4864-9c08-4d4a64edf3b8")]
	public class SpecialSpawn : MigratableMonoBehavior
	{
		public Transform[] spots;
	}
}
