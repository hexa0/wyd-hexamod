using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Spawning.Item
{
	[UnityMigrationIdentifier("HexaMod.083aec92-174a-4c16-932f-5b3c43ad5af7")]
	public class MidSpawn : MigratableMonoBehavior
	{
		public Transform[] spots;
	}
}
