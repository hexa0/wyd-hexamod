using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.CustomCharacterModels
{
	[UnityMigrationIdentifier("HexaMod.946260cb-500c-430c-9293-c996c6f3595c")]
	public class CharacterHeadBone : MigratableMonoBehavior
	{
		public Transform headBone;
	}
}
