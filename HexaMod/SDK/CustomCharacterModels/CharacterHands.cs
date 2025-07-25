using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.CustomCharacterModels
{
	[UnityMigrationIdentifier("HexaMod.a732cbd6-4dc5-4d23-9e3c-3ec9c9b08409")]
	public class CharacterHands : MigratableMonoBehavior
	{
		public Transform leftHand;
		public Transform rightHand;
	}
}
