using UnityEngine;

namespace HexaMod.ScriptableObjects
{
	[CreateAssetMenu(fileName = "ModShirt.asset", menuName = "HexaMod/ModShirt")]
	public class ModShirt : ScriptableObject
	{
		public Material shirtMaterial;
		public bool Recolorable = false;
	}
}
