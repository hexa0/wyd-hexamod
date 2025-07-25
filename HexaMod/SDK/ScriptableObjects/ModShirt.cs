using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.ScriptableObjects
{
	[CreateAssetMenu(fileName = "ModShirt.asset", menuName = "HexaMod/ModShirt")]
	[UnityMigrationIdentifier("HexaMod.06a07392-8d6b-4a79-9ede-ba6ffe596fda")]
	public class ModShirt : MigratableScriptableObject
	{
		public Material shirtMaterial;
		public bool Recolorable = false;
	}
}
