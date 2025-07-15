using Boo.Lang;
using HarmonyLib;
using HexaMapAssemblies;
using HexaMod.ScriptableObjects;
using HexaMod.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Scripts.CustomCharacterModels
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
		public ModCharacterModelBase currentModel;
		public Camera currentCamera;

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
			isSelf = HexaGlobal.networkManager.playerObj == transform.gameObject;
			Transform dadModel = transform.Find("DadModel") ?? transform;
			Transform babyModel = transform.Find("Baby001") ?? transform;

			Transform dadMesh = dadModel.Find("generic_male_01.005");
			Transform babyMesh = babyModel.Find("skin") ?? babyModel.Find("BabyBodyMesh");

			if (dadMesh)
			{
				isDad = true;

				body = dadMesh.GetComponent<SkinnedMeshRenderer>();

				defaultMesh = body.sharedMesh;
				defaultMaterials = body.materials;
				skinMaterialIndex = 2;
				shirtMaterialIndex = 4;

				if (transform.Find("DadCam"))
				{
					currentCamera = transform.Find("DadCam").GetComponent<Camera>();
				}
			}
			else if (babyMesh)
			{
				isDad = false;

				body = babyMesh.GetComponent<SkinnedMeshRenderer>();
				defaultMesh = body.sharedMesh;
				defaultMaterials = body.materials;
				skinMaterialIndex = 0;
				shirtMaterialIndex = -1;

				if (transform.Find("BabyCam"))
				{
					currentCamera = transform.Find("BabyCam").GetComponent<Camera>();
				}
			}
			else
			{
				throw new System.Exception("Cannot initalize model swapper for player, meshes weren't recognized");
			}

			currentShirtColor = initShirtColor;
			currentSkinColor = initSkinColor;
			SetCharacterModel(initModel);
			SetShirt(initShirt);
		}

		public void InitialStateDone()
		{
			bool selfCulled = currentModel != null && currentModel is ModCharacterModel && (currentModel as ModCharacterModel).selfCulling;

			if (currentCamera)
			{
				Renderer[] renderers;
				CharacterSelfCuller[] selfCullers = currentV2Model ? currentV2Model.GetComponentsInChildren<CharacterSelfCuller>() : new CharacterSelfCuller[0];

				{
					List<Renderer> rendererList = new List<Renderer>();

					foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
					{
						if (renderer != body || (renderer == body && selfCulled))
						{
							rendererList.Add(renderer);
						}
					}

					renderers = rendererList.ToArray();
				}

				ShadowCastingMode[] shadowCastingModes = new ShadowCastingMode[renderers.Length];
				bool culled = false;

				Camera.onPreRender += camera =>
				{
					if (camera == currentCamera)
					{
						if (!culled)
						{
							culled = true;

							for (int i = 0; i < renderers.Length; i++)
							{
								shadowCastingModes[i] = renderers[i].shadowCastingMode;
								renderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
							}
						}
					}
					else
					{
						if (culled)
						{
							culled = false;

							for (int i = 0; i < renderers.Length; i++)
							{
								renderers[i].shadowCastingMode = shadowCastingModes[i];
							}
						}
					}
				};

				Camera.onPostRender += camera =>
				{
					if (camera == currentCamera)
					{
						if (culled)
						{
							culled = false;

							for (int i = 0; i < renderers.Length; i++)
							{
								renderers[i].shadowCastingMode = shadowCastingModes[i];
							}
						}
					}
				};
			}
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
					currentModel = baseModel;

					if (baseModel is ModCharacterModel)
					{
						ModCharacterModel model = baseModel as ModCharacterModel;

						body.GetComponent<SkinnedMeshRenderer>().enabled = true;

						skinMaterialIndex = model.skinMaterialEditable ? model.skinMaterialId : -1;
						shirtMaterialIndex = model.shirtMaterialEditable ? model.shirtMaterialId : -1;

						body.sharedMesh = model.characterMesh;

						if (model.materials.Length > 0)
						{
							body.materials = model.materials;

							if (!isDad)
							{
								BabyStats babyStats = GetComponentInChildren<BabyStats>();

								if (babyStats)
								{
									babyStats.healthyColor = body.materials[0].color;
								}
							}
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
				skinMaterialIndex = isDad ? 2 : 0;
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

			if (currentShirtRecolorable && shirtMaterialIndex >= 0 && shirtMaterialIndex <= body.materials.Length)
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

			if (skinMaterialIndex >= 0 && skinMaterialIndex <= body.materials.Length)
			{
				var material = body.materials[skinMaterialIndex];
				material.color = skinColor;

				if (!isDad)
				{
					BabyStats babyStats = GetComponentInChildren<BabyStats>();

					if (babyStats)
					{
						babyStats.healthyColor = skinColor;
					}
				}
			}
		}
	}
}
