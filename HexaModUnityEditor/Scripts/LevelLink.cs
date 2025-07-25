using HexaMod.API.Util.Migration;
using HexaMod.SDK.ScriptableObjects;

namespace HexaModEditor.Scripts
{
	[UnityMigrationIdentifier("HexaModEditor.6736061f")]
	public class LevelLink : MigratableMonoBehavior
	{
		public ModLevel level;
	}
}
