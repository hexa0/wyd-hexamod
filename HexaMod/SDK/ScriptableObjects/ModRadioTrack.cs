using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.ScriptableObjects
{
	[CreateAssetMenu(fileName = "ModRadioTrack.asset", menuName = "HexaMod/ModRadioTrack")]
	[UnityMigrationIdentifier("HexaMod.1d66d872-748d-41a4-b896-43d1d28ef64c")]
	public class ModRadioTrack : MigratableScriptableObject
	{
		public AudioClip radioTrack;
	}
}
