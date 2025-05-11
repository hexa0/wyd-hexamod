using HexaMod.ScriptableObjects;
using UnityEngine;

namespace HexaMod.Util
{
	public class CharacterModelSwapper : MonoBehaviour
	{
		private Mesh defaultMesh;
		public Material[] defaultMaterials;

		private int skinMaterialIndex = -1;
		private int shirtMaterialIndex = -1;

		private Color currentShirtColor = HexToColor.GetColorFromHex("#E76F3D");
		private Color currentSkinColor = HexToColor.GetColorFromHex("#CC9485");

		public string initModel = "default";

		public Color initShirtColor = HexToColor.GetColorFromHex("#E76F3D");
		public Color initSkinColor = HexToColor.GetColorFromHex("#CC9485");

		private bool isSelf = false;
		private bool isDad = true;
		SkinnedMeshRenderer body;

		public void Start()
		{
			isSelf = HexaMod.networkManager.playerObj == transform.gameObject;
			var dadModel = transform.name == "Dad" ? transform : transform.Find("DadModel");
			if (dadModel)
			{
				isDad = true;
				body = dadModel.Find("generic_male_01.005").GetComponent<SkinnedMeshRenderer>();
				defaultMesh = body.sharedMesh;
				defaultMaterials = body.materials;
				skinMaterialIndex = 2;
				shirtMaterialIndex = 4;
			}
			else
			{
				isDad = false;
				var babyModel = transform.name == "Baby001" ? transform : transform.Find("Baby001");
				body = babyModel.GetComponentInChildren<SkinnedMeshRenderer>(true);
				defaultMesh = body.sharedMesh;
				defaultMaterials = body.materials;
			}

			currentShirtColor = initShirtColor;
			currentSkinColor = initSkinColor;
			SetCharacterModel(initModel);
		}

		public void SetCharacterModel(string modelName)
		{
			if (isDad)
			{
				bool foundMatch = false;

				foreach (ModCharacterModel model in Assets.characterModels)
				{
					if (model.isDad && model.modelNameReadable == modelName)
					{
						foundMatch = true;

						skinMaterialIndex = model.skinMaterialEditable ? model.skinMaterialId : -1;
						shirtMaterialIndex = model.shirtMaterialEditable ? model.shirtMaterialId : -1;

						body.sharedMesh = model.characterMesh;
						if (isSelf)
						{
							body.transform.parent.GetComponent<Animator>().cullingMode = AnimatorCullingMode.AlwaysAnimate;
							body.gameObject.layer = 12;
						}

						if (model.materials.Length > 0)
						{
							body.materials = model.materials;
						}
						else
						{
							body.materials = defaultMaterials;
						}
					}
				}

				if (!foundMatch)
				{
					body.gameObject.layer = 0;
					body.sharedMesh = defaultMesh;
					body.materials = defaultMaterials;
					skinMaterialIndex = 2;
					shirtMaterialIndex = 4;
				}
			}
			else
			{
				foreach (ModCharacterModel model in Assets.characterModels)
				{
					if (!model.isDad && model.modelNameReadable == modelName)
					{

					}
				}
			}

			SetShirtColor(currentShirtColor);
			SetSkinColor(currentSkinColor);
		}

		public void SetShirtColor(Color shirtColor)
		{
			currentShirtColor = shirtColor;

			if (shirtMaterialIndex >= 0 && shirtMaterialIndex < body.materials.Length)
			{
				if (isDad)
				{
					var material = body.materials[shirtMaterialIndex];
					material.color = shirtColor;
				}
				else
				{

				}
			}
		}

		public void SetSkinColor(Color skinColor)
		{
			currentSkinColor = skinColor;

			if (skinMaterialIndex >= 0 && skinMaterialIndex < body.materials.Length)
			{
				if (isDad)
				{
					var material = body.materials[skinMaterialIndex];
					material.color = skinColor;
				}
				else
				{

				}
			}
		}
	}
}
