using UnityEngine;

namespace HexaMod.ScriptableObjects
{
	[CreateAssetMenu(fileName = "ModCharacterModel.asset", menuName = "HexaMod/ModCharacterModel")]
	public class ModCharacterModel : ScriptableObject
	{
		[field: Header("Model Metadata")]
		[field: Space(1)]
		public string modelNameReadable = "Modded Character";
		public string modelDescriptionReadable = "A custom character model!";
		[field: Header("Data")]
		[field: Space(1)]
		public bool isDad = true;
		public Mesh characterMesh;
		public bool shirtMaterialEditable = true;
		public bool skinMaterialEditable = true;
		public int shirtMaterialId = 0;
		public int skinMaterialId = 0;
		public bool selfCulling = true;
		public Material[] materials;
	}
}
