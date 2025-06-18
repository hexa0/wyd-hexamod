using HarmonyLib;
using HexaMapAssemblies;
using HexaMod.ScriptableObjects;
using HexaMod.Scripts;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Util
{
	public class CharacterModelSwapper : MonoBehaviour
	{
		private Mesh defaultMesh;
		public Material[] defaultMaterials;

		private int skinMaterialIndex = -1;
		private int shirtMaterialIndex = -1;

		private Color currentShirtColor = new Color().FromHex("#E76F3D");
		private Color currentSkinColor = new Color().FromHex("#CC9485");

		public string currentShirtMaterial = "default";
		public bool currentShirtRecolorable = true;

		public string initModel = "default";
		public string initShirt = "default";

		public Color initShirtColor = new Color().FromHex("#E76F3D");
		public Color initSkinColor = new Color().FromHex("#CC9485");

		public GameObject currentV2Model;

		private bool isSelf = false;
		private bool isDad = true;
		SkinnedMeshRenderer body;

		public void Start()
		{
			isSelf = HexaMod.networkManager.playerObj == transform.gameObject;
			if (PhotonNetwork.offlineMode)
			{
				isSelf = false;
			}
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
				if (babyModel.Find("skin"))
				{
					body = babyModel.Find("skin").GetComponent<SkinnedMeshRenderer>();
				}
				else
				{
					body = babyModel.Find("BabyBodyMesh").GetComponent<SkinnedMeshRenderer>();
				}
				defaultMesh = body.sharedMesh;
				defaultMaterials = body.materials;
			}

			currentShirtColor = initShirtColor;
			currentSkinColor = initSkinColor;
			SetCharacterModel(initModel);
			SetShirt(initShirt);
		}

		public void SetCharacterModel(string modelName)
		{
			if (currentV2Model)
			{
				Destroy(currentV2Model);
				currentV2Model = null;
			}

			bool foundMatch = false;

			foreach (ModCharacterModelBase baseModel in Assets.characterModels)
			{
				if (baseModel.name == modelName && baseModel.isDad == isDad)
				{
					foundMatch = true;

					if (baseModel is ModCharacterModel)
					{
						ModCharacterModel model = baseModel as ModCharacterModel;

						body.GetComponent<SkinnedMeshRenderer>().enabled = true;

						skinMaterialIndex = model.skinMaterialEditable ? model.skinMaterialId : -1;
						shirtMaterialIndex = model.shirtMaterialEditable ? model.shirtMaterialId : -1;

						body.sharedMesh = model.characterMesh;
						if (isSelf)
						{
							body.transform.parent.GetComponent<Animator>().cullingMode = AnimatorCullingMode.AlwaysAnimate;
							foreach (var renderer in GetComponentsInChildren<Renderer>(true))
							{
								renderer.gameObject.layer = 12;
							}
							if (!model.selfCulling)
							{
								body.gameObject.layer = 1;
							}
						}
						else
						{
							foreach (var renderer in GetComponentsInChildren<Renderer>(true))
							{
								renderer.gameObject.layer = 0;
							}
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
					else if (baseModel is ModCharacterModelV2)
					{
						ModCharacterModelV2 model = baseModel as ModCharacterModelV2;

						currentV2Model = Instantiate(model.characterModel, transform);
						currentV2Model.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
						body.GetComponent<SkinnedMeshRenderer>().enabled = false;

						if (isDad)
						{
							Traverse animatorFields = Traverse.Create(GetComponentInChildren<DadAnimator>());
							Traverse<Animator> anim = animatorFields.Field<Animator>("anim");

							anim.Value = currentV2Model.GetComponentInChildren<Animator>();
						}
						else
						{
							Traverse animatorFields = Traverse.Create(GetComponentInChildren<BabyAnimator>());
							Traverse<Animator> anim = animatorFields.Field<Animator>("anim");

							anim.Value = currentV2Model.GetComponentInChildren<Animator>();
						}

						CharacterHands hands = currentV2Model.GetComponentInChildren<CharacterHands>();

						if (hands != null)
						{
							Transform originalLeftHand = body.transform.parent.Find("Armature").FindDeep(isDad ? "LeftDadHoldPos" : "LeftBabyHoldPos");
							Transform originalRightHand = body.transform.parent.Find("Armature").FindDeep(isDad ? "DadHoldPos" : "BabyHoldPos");

							if (originalLeftHand != null && originalRightHand != null)
							{
								originalLeftHand.name = "oldLeftHand";
								originalRightHand.name = "oldRightHand";
							}

							hands.leftHand.name = "LeftDadHoldPos";
							hands.rightHand.name = "DadHoldPos";
						}

						CharacterHats hats = currentV2Model.GetComponentInChildren<CharacterHats>();

						if (hats != null)
						{
							Transform armature = body.transform.parent.Find("Armature");
							Transform originalHats = armature.FindDeepChild("GameObject (1)");
							Transform originalShades = armature.FindDeepChild("Shades (1)");

							if (!originalHats && !originalShades)
							{
								originalHats = armature.FindDeepChild("GameObject");
								originalShades = armature.FindDeepChild("Shades");
							}

							if (originalHats && originalShades)
							{
								originalHats.SetParent(hats.hatRoot);
								originalHats.SetPositionAndRotation(hats.hatRoot.position, hats.hatRoot.rotation);
								originalHats.transform.localScale = Vector3.one;
								originalShades.SetParent(hats.shadesRoot);
								originalShades.SetPositionAndRotation(hats.shadesRoot.position, hats.shadesRoot.rotation);
								originalShades.transform.localScale = Vector3.one;
							}
						}

						FirstPersonController controller = GetComponentInChildren<FirstPersonController>();

						if (controller)
						{
							NetworkedSoundBehavior networkedSound = GetComponentInChildren<NetworkedSoundBehavior>();
							Traverse controllerFields = Traverse.Create(controller);

							if (model.footsteps != null && model.footsteps.Length > 0)
							{

								controllerFields.Field<AudioClip[]>("m_FootstepSounds").Value = model.footsteps;
							}

							if (model.jump != null)
							{
								controllerFields.Field<AudioClip>("m_JumpSound").Value = model.jump;
							}

							if (model.land != null)
							{
								controllerFields.Field<AudioClip>("m_LandSound").Value = model.land;
							}

							if (networkedSound)
							{
								networkedSound.UnregisterSounds();

								networkedSound.RegisterSound(controllerFields.Field<AudioClip>("m_JumpSound").Value);
								networkedSound.RegisterSound(controllerFields.Field<AudioClip>("m_LandSound").Value);
								networkedSound.RegisterSounds(controllerFields.Field<AudioClip[]>("m_FootstepSounds").Value);
							}

							if (isSelf)
							{
								foreach (CharacterSelfCuller culler in currentV2Model.GetComponentsInChildren<CharacterSelfCuller>())
								{
									culler.Cull();
								}
							}

							CharacterHeadBone headBone = currentV2Model.GetComponentInChildren<CharacterHeadBone>();

							if (headBone)
							{
								ParRotation headBoneRotation = headBone.headBone.gameObject.AddComponent<ParRotation>();
								headBoneRotation.target = controller.myCam.transform;
								headBoneRotation.Dad = isDad;
							}
						}
					}

					break;
				}
			}

			if (!foundMatch)
			{
				body.gameObject.layer = 0;
				body.sharedMesh = defaultMesh;
				body.materials = defaultMaterials;
				skinMaterialIndex = isDad ? 2 : -1;
				shirtMaterialIndex = isDad ? 4 : -1;
			}

			if (!isDad)
			{
				body.transform.parent.GetChild(1).gameObject.SetActive(!foundMatch);
				body.transform.parent.GetChild(2).gameObject.SetActive(!foundMatch);
				body.transform.parent.GetChild(3).gameObject.SetActive(!foundMatch);
				body.transform.parent.GetChild(5).gameObject.SetActive(!foundMatch);
				body.transform.parent.GetChild(7).gameObject.SetActive(!foundMatch);
				body.transform.parent.GetChild(8).gameObject.SetActive(!foundMatch);
				body.transform.parent.GetChild(9).gameObject.SetActive(!foundMatch);
			}

			SetShirtColor(currentShirtColor);
			SetSkinColor(currentSkinColor);
			SetShirt(currentShirtMaterial);
		}

		public void SetShirt(string shirtName)
		{
			currentShirtMaterial = shirtName;

			if (isDad)
			{
				bool foundMatch = false;

				foreach (ModShirt shirt in Assets.shirts)
				{
					if (shirt.name == shirtName)
					{
						foundMatch = true;

						if (shirtMaterialIndex >= 0)
						{
							Material[] bodyMaterials = body.materials;
							bodyMaterials[shirtMaterialIndex] = shirt.shirtMaterial;
							body.materials = bodyMaterials;

							currentShirtRecolorable = shirt.Recolorable;
						}
					}
				}

				if (!foundMatch)
				{
					currentShirtRecolorable = true;

					if (shirtMaterialIndex > 0)
					{
						Material[] bodyMaterials = body.materials;
						bodyMaterials[shirtMaterialIndex] = Assets.defaultShirt.shirtMaterial;
						body.materials = bodyMaterials;
					}
				}

				SetShirtColor(currentShirtColor);
				SetSkinColor(currentSkinColor);
			}
		}

		public void SetShirtColor(Color shirtColor)
		{
			currentShirtColor = shirtColor;

			if (currentShirtRecolorable && shirtMaterialIndex >= 0 && shirtMaterialIndex < body.materials.Length)
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
