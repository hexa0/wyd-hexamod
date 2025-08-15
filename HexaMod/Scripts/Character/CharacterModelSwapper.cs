using System;
using System.Collections.Generic;
using HarmonyLib;
using HexaMod.Scripts.Character.Controller.Character;
using HexaMod.Scripts.Util;
using HexaMod.SDK.CustomCharacterModels;
using HexaMod.SDK.ScriptableObjects;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Scripts.Character
{
	[Serializable]
	public class CharacterModelSwapper : MonoBehaviour
	{
		public HexaPlayerController PlayerController => GetComponent<HexaPlayerController>();

		public Mesh defaultMesh;
		public Material[] defaultMaterials;

		public int initSkinMaterialIndex = -1;
		public int initShirtMaterialIndex = -1;

		public int skinMaterialIndex = -1;
		public int shirtMaterialIndex = -1;

		public Color currentShirtColor = Color.magenta;
		public Color currentSkinColor = Color.magenta;

		public string currentShirtMaterial = "default";
		public bool currentShirtRecolorable = true;
		public ModCharacterModelBase currentModel;
		public Camera currentCamera;

		public string initModel = "default";
		public string initShirt = "default";

		public Color initShirtColor;
		public Color initSkinColor;

		public GameObject currentV2Model;

		SkinnedMeshRenderer body;

		public void SetDadModel()
		{
			Transform dadModel = transform.Find("DadModel") ?? transform;

			Transform dadMesh = dadModel.Find("generic_male_01.005");

			body = dadMesh.GetComponent<SkinnedMeshRenderer>();
			defaultMesh = body.sharedMesh;
			defaultMaterials = body.materials;
			initSkinMaterialIndex = 2;
			initShirtMaterialIndex = 4;
			skinMaterialIndex = initSkinMaterialIndex;
			shirtMaterialIndex = initShirtMaterialIndex;

			if (transform.Find("DadCam"))
			{
				currentCamera = transform.Find("DadCam").GetComponent<Camera>();
			}
		}

		public void SetBabyModel()
		{
			Transform babyModel = transform.Find("Baby001") ?? transform;

			Transform babyMesh = babyModel.Find("skin") ?? babyModel.Find("BabyBodyMesh");

			body = babyMesh.GetComponent<SkinnedMeshRenderer>();
			defaultMesh = body.sharedMesh;
			defaultMaterials = body.materials;
			initSkinMaterialIndex = 0;
			initShirtMaterialIndex = -1;
			skinMaterialIndex = initSkinMaterialIndex;
			shirtMaterialIndex = initShirtMaterialIndex;

			if (transform.Find("BabyCam"))
			{
				currentCamera = transform.Find("BabyCam").GetComponent<Camera>();
			}
		}

		public void Init()
		{
			if (initShirtMaterialIndex > 0)
			{
				currentShirtColor = initShirtColor != null ? initShirtColor : defaultMaterials[shirtMaterialIndex].color;
			}

			if (initSkinMaterialIndex > 0)
			{
				currentSkinColor = initSkinColor != null ? initSkinColor : defaultMaterials[skinMaterialIndex].color;
			}

			SetCharacterModel(initModel);
			SetShirt(initShirt);
		}

		Renderer[] cullRenderers;
		ShadowCastingMode[] cullRendererShadowCastingModes;
		bool culled = false;

		public void SetCullState(bool state)
		{
			if (state != culled)
			{
				culled = state;

				switch (state)
				{
					case true:
						for (int i = 0; i < cullRenderers.Length; i++)
						{
							cullRendererShadowCastingModes[i] = cullRenderers[i].shadowCastingMode;
							cullRenderers[i].shadowCastingMode = ShadowCastingMode.ShadowsOnly;
						}

						break;
					case false:
						for (int i = 0; i < cullRenderers.Length; i++)
						{
							cullRenderers[i].shadowCastingMode = cullRendererShadowCastingModes[i];
						}

						break;
				}
			}
		}

		public void HandleCameraPreRender(Camera camera)
		{
			if (camera == currentCamera)
			{
				SetCullState(!PlayerController.InThirdPerson);
			}
			else
			{
				SetCullState(false);
			}
		}

		public void HandleCameraPostRender(Camera camera)
		{
			if (camera == currentCamera)
			{
				SetCullState(false);
			}
		}

		public void InitialStateDone()
		{
			bool selfCulled = currentModel != null && currentModel is ModCharacterModel && (currentModel as ModCharacterModel).selfCulling;

			if (currentCamera)
			{
				CharacterSelfCuller[] selfCullers = currentV2Model ? currentV2Model.GetComponentsInChildren<CharacterSelfCuller>() : new CharacterSelfCuller[0];

				{
					List<Renderer> rendererList = new List<Renderer>();

					foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
					{
						if (renderer != body || renderer == body && selfCulled)
						{
							rendererList.Add(renderer);
						}
					}

					cullRenderers = rendererList.ToArray();
				}

				cullRendererShadowCastingModes = new ShadowCastingMode[cullRenderers.Length];
				culled = false;

				Camera.onPreRender += HandleCameraPreRender;
				Camera.onPostRender += HandleCameraPostRender;
			}
		}

		public void OnDestroy()
		{
			Camera.onPreRender -= HandleCameraPreRender;
			Camera.onPostRender -= HandleCameraPostRender;
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
				if (baseModel.name == modelName && baseModel.isDad == (PlayerController.teamSelector == "D"))
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

							if (PlayerController.teamSelector == "B")
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

						if (PlayerController.teamSelector == "D")
						{
							Traverse animatorFields = Traverse.Create(GetComponentInChildren<DadAnimator>());
							Traverse<Animator> anim = animatorFields.Field<Animator>("anim");

							anim.Value = currentV2Model.GetComponentInChildren<Animator>();
						}
						else if (PlayerController.teamSelector == "B")
						{
							Traverse animatorFields = Traverse.Create(GetComponentInChildren<BabyAnimator>());
							Traverse<Animator> anim = animatorFields.Field<Animator>("anim");

							anim.Value = currentV2Model.GetComponentInChildren<Animator>();
						}

						CharacterHands hands = currentV2Model.GetComponentInChildren<CharacterHands>();

						if (hands != null)
						{
							if (PlayerController.teamSelector == "D")
							{
								Transform originalLeftHand = body.transform.parent.Find("Armature").FindDeep("LeftDadHoldPos");
								Transform originalRightHand = body.transform.parent.Find("Armature").FindDeep("DadHoldPos");

								hands.leftHand.name = "LeftDadHoldPos";
								hands.rightHand.name = "DadHoldPos";

								originalLeftHand.name = "oldLeftHand";
								originalRightHand.name = "oldRightHand";
							}
							else if (PlayerController.teamSelector == "B")
							{
								Transform originalLeftHand = body.transform.parent.Find("Armature").FindDeep("LeftBabyHoldPos");
								Transform originalRightHand = body.transform.parent.Find("Armature").FindDeep("BabyHoldPos");

								hands.leftHand.name = "LeftBabyHoldPos";
								hands.rightHand.name = "BabyHoldPos";

								originalLeftHand.name = "oldLeftHand";
								originalRightHand.name = "oldRightHand";
							}
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

							networkedSound.UnregisterSounds();

							networkedSound.RegisterSound(controllerFields.Field<AudioClip>("m_JumpSound").Value);
							networkedSound.RegisterSound(controllerFields.Field<AudioClip>("m_LandSound").Value);
							networkedSound.RegisterSounds(controllerFields.Field<AudioClip[]>("m_FootstepSounds").Value);

							CharacterHeadBone headBone = currentV2Model.GetComponentInChildren<CharacterHeadBone>();

							if (headBone)
							{
								ParRotation headBoneRotation = headBone.headBone.gameObject.AddComponent<ParRotation>();
								headBoneRotation.target = controller.myCam.transform;
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
				skinMaterialIndex = initSkinMaterialIndex;
				shirtMaterialIndex = initShirtMaterialIndex;
			}

			if (PlayerController.teamSelector == "B")
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

			if (PlayerController.teamSelector == "D")
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
				var material = body.materials[shirtMaterialIndex];
				material.color = shirtColor;
			}
		}

		public void SetSkinColor(Color skinColor)
		{
			currentSkinColor = skinColor;

			if (skinMaterialIndex >= 0 && skinMaterialIndex <= body.materials.Length)
			{
				var material = body.materials[skinMaterialIndex];
				material.color = skinColor;

				if (PlayerController.teamSelector == "B")
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
