using HexaMod.API.Util.Migration;

namespace HexaMod.SDK.CustomCharacterModels
{
	[UnityMigrationIdentifier("HexaMod.c88fb365-1906-4d27-8dbe-a8736798459a")]
	public class CharacterSelfCuller : MigratableMonoBehavior
	{
		public void Cull()
		{
			gameObject.layer = 12;
		}
	}
}
