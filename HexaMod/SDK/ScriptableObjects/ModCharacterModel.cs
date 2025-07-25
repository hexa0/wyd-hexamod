using HexaMod.API.Util.Migration;
using UnityEngine;

namespace HexaMod.SDK.ScriptableObjects
{
	[UnityMigrationIdentifier("HexaMod.d074cfd7-0006-44d7-940b-c1b69a8533fe")]
	public class ModCharacterModelBase : MigratableScriptableObject
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
	[UnityMigrationIdentifier("HexaMod.a3da898f-2084-48e8-9a3d-549684aeb5f1")]
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
	[UnityMigrationIdentifier("HexaMod.dd1d0eb9-d904-4a20-ba29-af3fd8ee7bb5")]
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
