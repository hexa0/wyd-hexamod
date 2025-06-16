using UnityEngine;

namespace HexaMod.ScriptableObjects
{
	public class ModCharacterModelBase : ScriptableObject
	{
		[field: Header("Model Metadata")]
		[field: Space(1)]
		public string modelNameReadable = "Modded Character";
		public string modelDescriptionReadable = "A custom character model!";
		[field: Header("Base Model Data")]
		[field: Space(1)]
		public bool isDad = true;
	}

	[CreateAssetMenu(fileName = "ModCharacterModel.asset", menuName = "HexaMod/ModCharacterModel (V1)")]
	public class ModCharacterModel : ModCharacterModelBase
	{
		[field: Header("V1 Data")]
		[field: Space(1)]
		public Mesh characterMesh;
		public bool shirtMaterialEditable = true;
		public bool skinMaterialEditable = true;
		public int shirtMaterialId = 0;
		public int skinMaterialId = 0;
		public bool selfCulling = true;
		public Material[] materials;
	}

	[CreateAssetMenu(fileName = "ModCharacterModelV2.asset", menuName = "HexaMod/ModCharacterModel (V2)")]
	public class ModCharacterModelV2 : ModCharacterModelBase
	{
		[field: Header("V2 Data")]
		[field: Space(1)]
		public GameObject characterModel;
		public AudioClip[] footsteps;
		public AudioClip jump;
		public AudioClip land;
	}
}
