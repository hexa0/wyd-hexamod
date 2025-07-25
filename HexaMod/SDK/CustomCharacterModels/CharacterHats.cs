using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.CustomCharacterModels
{
	[UnityMigrationIdentifier("HexaMod.7a54adb0-90ce-4ca1-9219-87b30c723e66")]
	public class CharacterHats : MigratableMonoBehavior
	{
		public Transform hatRoot;
		public Transform shadesRoot;
	}
}
