using HexaMod.API.Util.Migration;
using HexaMod.SDK.ScriptableObjects;

namespace HexaModEditor.EditorSDK
{
	[UnityMigrationIdentifier("HexaModEditor.6736061f")]
	public class LevelLink : MigratableMonoBehavior
	{
		public ModLevel level;
	}
}
