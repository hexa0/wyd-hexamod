using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HexaMod.API.Util.Data;
using HexaMod.API.Util.WhosYourDaddy;
using HexaMod.API.Voice.Script;
using HexaMod.Patches.Feature;
using HexaMod.Scripts.Character.Controller;
using HexaMod.Scripts.Multiplayer.Lobby;
using HexaMod.Scripts.Multiplayer.SerializableObjects;
using HexaMod.Scripts.Persistent;
using HexaMod.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

namespace HexaMod.Scripts.Character.Controller.Character
{
	public class HexaPlayerController : FirstPersonController
	{
		public PhotonView View => PhotonView.Get(this);

		internal Traverse traverse;
		internal Fields fields;

		public HexaPlayerController()
		{
			traverse = Traverse.Create(this);
			fields = new Fields(traverse);
		}

		#region Private FirstPersonController Properties (Setup)

		public class Fields
		{
			// all private fields of FirstPersonController
			public Traverse<bool> IsWalking { get; set; }
			public Traverse<float> WalkSpeed { get; set; }
			public Traverse<float> RunSpeed { get; set; }
			public Traverse<float> RunstepLenghten { get; set; }
			public Traverse<float> JumpForce { get; set; }
			public Traverse<float> StickToGroundForce { get; set; }
			public Traverse<float> GravityMultiplier { get; set; }
			public Traverse<bool> UseFovKick { get; set; }
			public Traverse<FOVKick> FovKick { get; set; }
			public Traverse<bool> UseHeadBob { get; set; }
			public Traverse<CurveControlledBob> HeadBob { get; set; }
			public Traverse<LerpControlledBob> JumpBob { get; set; }
			public Traverse<float> StepInterval { get; set; }
			public Traverse<AudioClip[]> FootstepSounds { get; set; }
			public Traverse<AudioClip> JumpSound { get; set; }
			public Traverse<AudioClip> LandSound { get; set; }
			public Traverse<bool> Jump { get; set; }
			public Traverse<float> YRotation { get; set; }
			public Traverse<Vector2> Input { get; set; }
			public Traverse<Vector3> MoveDir { get; set; }
			public Traverse<CollisionFlags> CollisionFlags { get; set; }
			public Traverse<bool> PreviouslyGrounded { get; set; }
			public Traverse<Vector3> OriginalCameraPosition { get; set; }
			public Traverse<float> StepCycle { get; set; }
			public Traverse<float> NextStep { get; set; }
			public Traverse<bool> Jumping { get; set; }
			public Traverse<bool> UnlimitedRun { get; set; }
			public Traverse<bool> TripleJump { get; set; }
			public Traverse<bool> SpeedBoosted { get; set; }
			public Traverse<bool> BulkBaby { get; set; }

			public Fields(Traverse traverse)
			{
				IsWalking = traverse.Field<bool>("m_IsWalking");
				WalkSpeed = traverse.Field<float>("m_WalkSpeed");
				RunSpeed = traverse.Field<float>("m_RunSpeed");
				RunstepLenghten = traverse.Field<float>("m_RunstepLenghten");
				JumpForce = traverse.Field<float>("m_JumpSpeed");
				StickToGroundForce = traverse.Field<float>("m_StickToGroundForce");
				GravityMultiplier = traverse.Field<float>("m_GravityMultiplier");
				UseFovKick = traverse.Field<bool>("m_UseFovKick");
				FovKick = traverse.Field<FOVKick>("m_FovKick");
				UseHeadBob = traverse.Field<bool>("m_UseHeadBob");
				HeadBob = traverse.Field<CurveControlledBob>("m_HeadBob");
				JumpBob = traverse.Field<LerpControlledBob>("m_JumpBob");
				StepInterval = traverse.Field<float>("m_StepInterval");
				FootstepSounds = traverse.Field<AudioClip[]>("m_FootstepSounds");
				JumpSound = traverse.Field<AudioClip>("m_JumpSound");
				LandSound = traverse.Field<AudioClip>("m_LandSound");
				Jump = traverse.Field<bool>("m_Jump");
				YRotation = traverse.Field<float>("m_YRotation");
				Input = traverse.Field<Vector2>("m_Input");
				MoveDir = traverse.Field<Vector3>("m_MoveDir");
				CollisionFlags = traverse.Field<CollisionFlags>("m_CollisionFlags");
				PreviouslyGrounded = traverse.Field<bool>("m_PreviouslyGrounded");
				OriginalCameraPosition = traverse.Field<Vector3>("m_OriginalCameraPosition");
				StepCycle = traverse.Field<float>("m_StepCycle");
				NextStep = traverse.Field<float>("m_NextStep");
				Jumping = traverse.Field<bool>("m_Jumping");
				UnlimitedRun = traverse.Field<bool>("unlimitedRun");
				TripleJump = traverse.Field<bool>("tripleJump");
				SpeedBoosted = traverse.Field<bool>("speedBoosted");
				BulkBaby = traverse.Field<bool>("bulkBaby");
			}
		}

		#endregion

		#region Private FirstPersonController Properties (Setters and Setters)


		public bool IsWalking
		{
			get => fields.IsWalking.Value;
			set => fields.IsWalking.Value = value;
		}
		public float WalkSpeed
		{
			get => fields.WalkSpeed.Value;
			set => fields.WalkSpeed.Value = value;
		}
		public float RunSpeed
		{
			get => fields.RunSpeed.Value;
			set => fields.RunSpeed.Value = value;
		}
		public float RunstepLenghten
		{
			get => fields.RunstepLenghten.Value;
			set => fields.RunstepLenghten.Value = value;
		}
		public float JumpForce
		{
			get => fields.JumpForce.Value;
			set => fields.JumpForce.Value = value;
		}
		public float StickToGroundForce
		{
			get => fields.StickToGroundForce.Value;
			set => fields.StickToGroundForce.Value = value;
		}
		public float GravityMultiplier
		{
			get => fields.GravityMultiplier.Value;
			set => fields.GravityMultiplier.Value = value;
		}
		public bool UseFovKick
		{
			get => fields.UseFovKick.Value;
			set => fields.UseFovKick.Value = value;
		}
		public FOVKick FovKick
		{
			get => fields.FovKick.Value;
			set => fields.FovKick.Value = value;
		}
		public bool UseHeadBob
		{
			get => fields.UseHeadBob.Value;
			set => fields.UseHeadBob.Value = value;
		}
		public CurveControlledBob HeadBob
		{
			get => fields.HeadBob.Value;
			set => fields.HeadBob.Value = value;
		}
		public LerpControlledBob JumpBob
		{
			get => fields.JumpBob.Value;
			set => fields.JumpBob.Value = value;
		}
		public float StepInterval
		{
			get => fields.StepInterval.Value;
			set => fields.StepInterval.Value = value;
		}
		public AudioClip[] FootstepSounds
		{
			get => fields.FootstepSounds.Value;
			set => fields.FootstepSounds.Value = value;
		}
		public AudioClip JumpSound
		{
			get => fields.JumpSound.Value;
			set => fields.JumpSound.Value = value;
		}
		public AudioClip LandSound
		{
			get => fields.LandSound.Value;
			set => fields.LandSound.Value = value;
		}
		public bool Jump
		{
			get => fields.Jump.Value;
			set => fields.Jump.Value = value;
		}
		public float YRotation
		{
			get => fields.YRotation.Value;
			set => fields.YRotation.Value = value;
		}
		public Vector2 MoveInput
		{
			get => fields.Input.Value;
			set => fields.Input.Value = value;
		}
		public Vector3 MoveDir
		{
			get => fields.MoveDir.Value;
			set => fields.MoveDir.Value = value;
		}
		public CollisionFlags CollisionFlags
		{
			get => fields.CollisionFlags.Value;
			set => fields.CollisionFlags.Value = value;
		}
		public bool PreviouslyGrounded
		{
			get => fields.PreviouslyGrounded.Value;
			set => fields.PreviouslyGrounded.Value = value;
		}
		public Vector3 OriginalCameraPosition
		{
			get => fields.OriginalCameraPosition.Value;
			set => fields.OriginalCameraPosition.Value = value;
		}
		public float StepCycle
		{
			get => fields.StepCycle.Value;
			set => fields.StepCycle.Value = value;
		}
		public float NextStep
		{
			get => fields.NextStep.Value;
			set => fields.NextStep.Value = value;
		}
		public bool Jumping
		{
			get => fields.Jumping.Value;
			set => fields.Jumping.Value = value;
		}
		public bool UnlimitedRun
		{
			get => fields.UnlimitedRun.Value;
			set => fields.UnlimitedRun.Value = value;
		}
		public bool TripleJump
		{
			get => fields.TripleJump.Value;
			set => fields.TripleJump.Value = value;
		}
		public bool SpeedBoosted
		{
			get => fields.SpeedBoosted.Value;
			set => fields.SpeedBoosted.Value = value;
		}
		public bool BulkBaby
		{
			get => fields.BulkBaby.Value;
			set => fields.BulkBaby.Value = value;
		}

		#endregion

		internal MethodInfo baseStart = typeof(FirstPersonController).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
		internal MethodInfo baseUpdate = typeof(FirstPersonController).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
		internal MethodInfo baseUpdateCameraPosition = typeof(FirstPersonController).GetMethod("UpdateCameraPosition", BindingFlags.NonPublic | BindingFlags.Instance);

		public NetworkedSoundBehavior networkedSound;
		public CameraController cameraController;
		public UnityEngine.CharacterController characterController;
		public CharacterModelSwapper characterModelSwapper;
		public CharacterInteraction characterInteraction;
		public Crouch crouch;
		public DadAnimator dadAnimator;
		public BabyAnimator babyAnimator;
		public DeathCam deathCam;
		public AudioListener audioListener;
		public ActionInput input;
		public PlayerVoiceEmitterRPC voiceEmitter;
		public AudioSource audioSource;

		[System.Flags]
		public enum PlayerFlags
		{
			None = 0,
			UseClassicProjectOnPlane = 1,
			UseClassicFixedUpdate = 2,
			UseClassicGroundedCheckForJumps = 3,
		}


		PlayerFlags _playerFlags = PlayerFlags.None;
		public FlagHelper<PlayerFlags> playerFlags;

		public ActionText ActionText => GameObject.Find("ActionText").GetComponent<ActionText>();
		public AchievementManager AchievementManager => GameObject.Find("AchievementManager").GetComponent<AchievementManager>();

		InitialPlayerState initialState;

		public string teamSelector = "A";
		public string defaultShirtColor = "#FF00FF";
		public string defaultSkinColor = "#FF00FF";

		internal Transform leftHand;
		internal Transform rightHand;
		public bool rightHanded = true;

		public Transform PrimaryHoldTransform {
			get => rightHanded ? rightHand : leftHand;
			set {	if (rightHanded)
						rightHand = value;
					else
						leftHand = value; }
		}
		public Transform SecondaryHoldTransform
		{
			get => rightHanded ? leftHand : rightHand;
			set {	if (rightHanded)
						leftHand = value;
					else
						rightHand = value; }
		}

		private static readonly int NAN_FIX_MEMORY_SIZE = 120;
		private readonly List<Vector3> positionMemory = new List<Vector3>();
		private readonly List<Quaternion> rotationMemory = new List<Quaternion>();
		private readonly List<Quaternion> cameraRotationMemory = new List<Quaternion>();

		public bool noclip = false;
		public bool fixingNaN = false;
		public bool InThirdPerson => cameraPerspective != 0;
		public int cameraPerspective = 0;
		public CameraPerspective CameraPerspectiveEnum => (CameraPerspective)cameraPerspective;

		public enum CameraPerspective : int
		{
			FirstPerson = 0,
			Behind = 1,
			InFront = 2
		}

		public virtual void Awake()
		{
			playerFlags = new FlagHelper<PlayerFlags>(() => { return ref _playerFlags; } );
			networkedSound = gameObject.GetComponent<NetworkedSoundBehavior>();
			cameraController = GetComponent<CameraController>();
			characterModelSwapper = GetComponent<CharacterModelSwapper>();
			characterController = GetComponent<UnityEngine.CharacterController>();
			characterInteraction = GetComponent<CharacterInteraction>();
			crouch = GetComponent<Crouch>();
			dadAnimator = GetComponentInChildren<DadAnimator>();
			babyAnimator = GetComponentInChildren<BabyAnimator>();
			deathCam = GetComponentInChildren<DeathCam>();
			audioListener = GetComponentInChildren<AudioListener>();
			voiceEmitter = GetComponent<PlayerVoiceEmitterRPC>();
			audioSource = GetComponent<AudioSource>();

			if (View && View.isMine)
			{
				HexaGlobal.networkManager.playerObj = gameObject;
				input = HexaGlobal.networkManager.player1Input;
				input.igmh = GetComponentInChildren<InGameMenuHelper>();
				input.myPlayer = this;
				input.dadItem = characterInteraction.dadItemTargetting;
				input.babyItem = characterInteraction.itemTargetting;
				input.dadCrouch = crouch;
				input.dadAnim = dadAnimator;
				input.babyAnim = babyAnimator;
				input.deathCam = deathCam;
				audioListener.enabled = true;
			}
		}

		public AudioClip[] GetSounds()
		{
			AudioClip[] networkedSounds = new AudioClip[2 + FootstepSounds.Length];

			networkedSounds[0] = JumpSound;
			networkedSounds[1] = LandSound;
			for (int i = 0; i < FootstepSounds.Length; i++)
			{
				networkedSounds[i + 2] = FootstepSounds[i];
			}

			return networkedSounds;
		}

		public void ActionMessage(string text)
		{
			if (View & View.isMine)
			{
				StartCoroutine(ActionText.ActionDone(text));
			}
		}

		/// <summary>
		/// holds the specified gameObject as an item
		/// </summary>
		/// <param name="toHold">the gameObject to grab up.</param>
		/// <param name="useHandPrimary">whether to pickup with the primary hand or not.</param>
		/// <returns>Whether the object was succesfully held or not</returns>
		public bool Hold(GameObject toHold, bool useHandPrimary = true)
		{
			if (characterInteraction.Buttered)
			{
				ActionMessage($"The {CharacterInteraction.GetTargetName(toHold)} slips through your buttery fingers.");
				return false;
			}
			else
			{
				if (useHandPrimary)
				{
					if (characterInteraction.PrimaryItemTransform)
					{
						Transform item = characterInteraction.PrimaryItemTransform;
						DropPrimaryItem();
						item.SetPositionAndRotation(toHold.transform.position, toHold.transform.rotation);
					}
				}
				else
				{
					if (characterInteraction.SecondaryItemTransform)
					{
						Transform item = characterInteraction.SecondaryItemTransform;
						DropSecondaryItem();
						item.SetPositionAndRotation(toHold.transform.position, toHold.transform.rotation);
					}
				}

				View.RPC("HoldRPC", PhotonTargets.All, toHold.GetPhotonView().viewID, useHandPrimary);
				toHold.SendMessage("Grab", SendMessageOptions.DontRequireReceiver);

				return true;
			}
		}

		[PunRPC]
		public void HoldRPC(int toHoldId, bool useHandPrimary = true)
		{
			GameObject toHold = PhotonView.Find(toHoldId).gameObject;

			if (useHandPrimary)
			{
				toHold.transform.SetParent(PrimaryHoldTransform, true);
				characterInteraction.PrimaryItemTransform = toHold.transform;
			}
			else
			{
				toHold.transform.SetParent(SecondaryHoldTransform, true);
				characterInteraction.SecondaryItemTransform = toHold.transform;
			}

			Fork fork = toHold.GetComponent<Fork>();
			LeftHand leftHand = toHold.GetComponent<LeftHand>();

			if (fork != null)
			{
				fork.curHoldPos = toHold.transform.parent;
				fork.held = true;
			}

			if (leftHand != null)
			{
				leftHand.curHoldPos = toHold.transform.parent;
				leftHand.held = true;
			}

			Rigidbody rigidbody = toHold.GetComponent<Rigidbody>();

			if (rigidbody)
			{
				rigidbody.isKinematic = true;
				rigidbody.interpolation = RigidbodyInterpolation.None;
			}

			toHold.transform.localPosition = Vector3.zero;
			toHold.transform.localRotation = Quaternion.identity;
			toHold.layer = CharacterInteraction.ignoreRaycastLayer;
		}

		public void DropItem(Transform item)
		{
			if (item)
			{
				ActionMessage($"You drop the {CharacterInteraction.GetTargetName(item.gameObject)}");

				View.RPC("DropItemRPC", PhotonTargets.All, item.gameObject.GetPhotonView().viewID);
				item.SendMessage("Drop", Vector3.zero, SendMessageOptions.DontRequireReceiver);
			}
		}

		[PunRPC]
		public void DropItemRPC(int itemId)
		{
			Transform item = PhotonView.Find(itemId).transform;

			if (characterInteraction.leftHandItem == item)
			{
				characterInteraction.leftHandItem = null;
			}

			if (characterInteraction.rightHandItem == item)
			{
				characterInteraction.rightHandItem = null;
			}

			Fork fork = item.GetComponent<Fork>();
			LeftHand leftHand = item.GetComponent<LeftHand>();

			if (fork != null)
			{
				fork.curHoldPos = null;
				fork.held = false;
			}

			if (leftHand != null)
			{
				leftHand.curHoldPos = null;
				leftHand.held = false;
			}

			Rigidbody rigidbody = item.GetComponent<Rigidbody>();

			if (rigidbody)
			{
				rigidbody.isKinematic = false;
				rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			}

			item.gameObject.layer = CharacterInteraction.grabableLayer;

			item.SetParent(ItemSpawnerParent.parent, true);
		}

		public void DropPrimaryItem() => DropItem(characterInteraction.PrimaryItemTransform);

		public void DropSecondaryItem() => DropItem(characterInteraction.SecondaryItemTransform);

		/// <summary>
		/// picks up the specified gameObject as a prop
		/// </summary>
		/// <param name="toPickUp">the gameObject to pick up.</param>
		public void PickUpProp(GameObject toPickUp)
		{
			if (!characterInteraction.heldProp)
			{
				characterInteraction.heldProp = toPickUp.GetComponent<PickUp>();
				toPickUp.SendMessage("PickUp", transform, SendMessageOptions.DontRequireReceiver);
			}
		}

		public void DropProp()
		{
			if (characterInteraction.heldProp)
			{
				ActionMessage($"You drop the {CharacterInteraction.GetTargetName(characterInteraction.heldProp.gameObject)}");
				characterInteraction.heldProp.SendMessage("PutDown");
				characterInteraction.heldProp = null;
			}
		}

		public void MouseLookUseCurrentTransform()
		{
			m_MouseLook.Init(transform, myCam.transform);
		}

		public virtual void Update()
		{
			baseUpdate.Invoke(this, null);

			if (View && View.isMine)
			{
				if (Input.GetKeyDown("x"))
				{
					cameraPerspective++;

					if (cameraPerspective > 2)
					{
						cameraPerspective = 0;
					}

					ActionMessage($"Perspective Changed to {CameraPerspectiveEnum}");
				}

				if (HexaPersistentLobby.instance.lobbySettings.cheats && Input.GetKeyDown("v"))
				{
					noclip = !noclip;
					characterController.enabled = !noclip;

					ActionMessage($"Noclip {(noclip ? "Enabled" : "Disabled")}");
				}

				if (!noclip)
				{
					if (!fixingNaN)
					{
						ProcessMovement();

						model.tased = blasted < 0f;

						if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
						{
							fixingNaN = true;
							StartCoroutine(Unstuck());
						}
						else
						{
							positionMemory.Add(transform.position);
							rotationMemory.Add(transform.rotation);
							cameraRotationMemory.Add(myCam.transform.rotation);

							if (positionMemory.Count > NAN_FIX_MEMORY_SIZE)
							{
								positionMemory.RemoveAt(0);
								rotationMemory.RemoveAt(0);
								cameraRotationMemory.RemoveAt(0);
							}
						}
					}
				}
				else
				{
					fixingNaN = false;

					Vector3 moveVector = new Vector3(
						xAxis,
						0f +
						(Input.GetKey("e") ? 1f : 0f) +
						(Input.GetKey("q") ? -1f : 0f),
						yAxis
					);

					transform.Translate(moveVector * (Time.deltaTime * (runButton ? 20f : 10f)), myCam.transform);
				}

				cameraController.UpdateCamera();
				characterInteraction.UpdateItemInteraction();
			}
		}

		public float currentSpeed = 0f;
		public void UpdatePlayerSpeed()
		{
			blasted += Time.deltaTime * 0.5f;

			if (blasted > 1f)
			{
				blasted = 1f;
			}
			if (hasCuffs)
			{
				blasted = 0.5f;
			}

			currentSpeed = (!IsWalking ? RunSpeed * stabbed * blasted : WalkSpeed * stabbed * blasted) * Mathf.Clamp(energy, 0.15f, 1f);

			if (dash)
			{
				currentSpeed = Mathf.Clamp(currentSpeed, WalkSpeed, RunSpeed);
				currentSpeed *= blasted;

				if (!IsWalking)
				{
					if (!UnlimitedRun)
					{
						energy -= Time.deltaTime / 2f;
					}
				}
				else
				{
					energy += Time.deltaTime / 24f;
					if (restrainedHeld)
					{
						energy += Time.deltaTime / 12f;
					}
				}

				cooldownImg.GetComponent<Image>().fillAmount = energy;

				energy = Mathf.Clamp01(energy);

				if (energy == 1f && restrainedHeld)
				{
					restrainer.SendMessage("DropItem");
					restrainedHeld = false;
				}
			}

			if (blasted < 0f)
			{
				currentSpeed = 0f;
			}

			if (SpeedBoosted)
			{
				currentSpeed *= 2f;
			}
		}

		public Vector2 GetRawInput()
		{
			if (haltInput)
			{
				return Vector2.zero;
			}
			else
			{
				return new Vector2(
					xAxis,
					yAxis
				);
			}
		}

		public void ProcessInputs()
		{
			Vector2 rawInput = GetRawInput();

			bool wasWalking = IsWalking;
			IsWalking = !runButton;

			MoveInput = rawInput;
			if (MoveInput.sqrMagnitude > 1f)
			{
				MoveInput.Normalize();
			}

			if (IsWalking != wasWalking && UseFovKick && characterController.velocity.sqrMagnitude > 0f)
			{
				StopCoroutine("FOVKickDown");
				StopCoroutine("FOVKickUp");
				StartCoroutine(IsWalking ? FovKick.FOVKickDown() : FovKick.FOVKickUp());
			}
		}

		public virtual void ProcessMovement() {
			UpdatePlayerSpeed();
			ProcessInputs();

			Vector3 moveInputInWorldSpace = transform.forward * MoveInput.y + transform.right * MoveInput.x;
			Vector3 position = transform.position;
			position.y -= rayHeight;

			if (playerFlags.IsSet(PlayerFlags.UseClassicProjectOnPlane))
			{
				Physics.SphereCast(position, characterController.radius, Vector3.down, out RaycastHit floorRaycast, characterController.height / 2f);
				moveInputInWorldSpace = Vector3.ProjectOnPlane(moveInputInWorldSpace, floorRaycast.normal).normalized;
			}
			else
			{
				moveInputInWorldSpace = moveInputInWorldSpace.normalized;
			}

			Vector3 moveDir = MoveDir;

			moveDir.x = moveInputInWorldSpace.x * currentSpeed;
			moveDir.z = moveInputInWorldSpace.z * currentSpeed;

			if (characterController.isGrounded)
			{
				moveDir.y = -StickToGroundForce;

				bool grounded = playerFlags.IsSet(PlayerFlags.UseClassicGroundedCheckForJumps) ? ClassicCharIsGrounded() : characterController.isGrounded;

				if (Jump && grounded)
				{
					moveDir.y = TripleJump ? JumpForce * 2f : JumpForce;
					PlayJumpSound();
					Jump = false;
					Jumping = true;
				}
			}

			moveDir += Physics.gravity * GravityMultiplier * Time.smoothDeltaTime;
			CollisionFlags = characterController.Move(moveDir * Time.smoothDeltaTime);
			MoveDir = moveDir;
			ProgressStepCycle();
		}

		public bool ClassicCharIsGrounded()
		{

			if (!Physics.SphereCast(transform.position, 0.2f, Vector3.down, out RaycastHit raycastHit, GetComponent<UnityEngine.CharacterController>().height + 0.3f + transform.localScale.x - 0.75f) && !Physics.Raycast(transform.position, Vector3.down, out raycastHit, GetComponent<UnityEngine.CharacterController>().height + 0.2f))
			{
				Jump = false;
				Jumping = false;

				return false;
			}

			Rigidbody rigidbody = raycastHit.rigidbody;

			if (!rigidbody)
			{
				return true;
			}

			if (Mathf.Abs(rigidbody.velocity.y) > 0.25f)
			{
				Jump = false;
				Jumping = false;

				return false;
			}

			return true;
		}

		public virtual void ProgressStepCycle()
		{
			if (characterController.velocity.sqrMagnitude > 0f && MoveInput.magnitude > 0f)
			{
				StepCycle += (characterController.velocity.magnitude + currentSpeed * (!IsWalking ? RunstepLenghten : 1f)) * Time.smoothDeltaTime;
			}

			if (StepCycle <= NextStep)
			{
				return;
			}

			NextStep = StepCycle + StepInterval;

			if (characterController.isGrounded)
			{
				PlayFootStepAudio();
			}
		}

		// not used currently but is required to disable the original FixedUpdate method from being called
		public virtual void FixedUpdate() { }

		void PlayFootStepAudio()
		{
			int randomAudio = Random.Range(1, FootstepSounds.Length);
			AudioClip randomClip = FootstepSounds[randomAudio];
			networkedSound.Play(randomClip, Mathf.Clamp01(characterController.velocity.magnitude / currentSpeed));
			FootstepSounds[randomAudio] = FootstepSounds[0];
			FootstepSounds[0] = randomClip;
		}

		void PlayJumpSound()
		{
			networkedSound.Play(JumpSound);
		}

		void PlayLandSound()
		{
			networkedSound.Play(LandSound);
		}

		void ProcessInitialState()
		{
			playerName = HexaLobby.GetPlayerName(View.owner);
			characterModelSwapper.Init();
			characterModelSwapper.SetShirtColor(initialState.shirtColor.toColor());
			characterModelSwapper.SetSkinColor(initialState.skinColor.toColor());
			characterModelSwapper.SetCharacterModel(initialState.characterModel);
			characterModelSwapper.SetShirt(initialState.shirtMaterial);
			characterModelSwapper.InitialStateDone();
			voiceEmitter.SetVoicePlayer(View.owner);
		}

		public virtual void Start()
		{
			transform.SetParent(PlayerControllers.parent, true);

			cameraController.cameraOffsets.Add("baseCharacterOffset", new Vector3(
				myCam.transform.localPosition.x,
				myCam.transform.localPosition.y,
				myCam.transform.localPosition.x
				// ^ this code bug causes a behavior in the game that makes the camera offset itself forwards
				// it seems the code was unintentionally made to do that by setting a variable wrong but was never patched because it actually prevents you from seeing yourself
			));

			baseStart.Invoke(this, null);

			networkedSound.RegisterSounds(GetSounds());

			if (View & View.isMine)
			{
				audioSource.volume = 0.35f;
				audioSource.spatialize = false;
				audioSource.spatialBlend = 0f;
				audioSource.bypassEffects = true;
				audioSource.panStereo = 0f;

				string shirtColor = PlayerPrefs.GetString($"HMV2_{teamSelector}ShirtColor", defaultShirtColor);
				string skinColor = PlayerPrefs.GetString($"HMV2_{teamSelector}SkinColor", defaultSkinColor);

				initialState = new InitialPlayerState()
				{
					shirtColor = new SerializableColor(new Color().FromHex(shirtColor)),
					skinColor = new SerializableColor(new Color().FromHex(skinColor)),
					characterModel = PlayerPrefs.GetString($"HMV2_{teamSelector}CharacterModel", "default"),
					shirtMaterial = PlayerPrefs.GetString($"HMV2_{teamSelector}ShirtMaterial", "default")
				};

				View.RPC("SetInitialState", PhotonTargets.AllBuffered, InitialPlayerState.serializer.Serialize(initialState));
			}
			else {
				if (audioSource)
				{
					audioSource.spatialize = true;
					audioSource.spatialBlend = 1f;
					audioSource.bypassEffects = true;
					audioSource.panStereo = 0f;
				}
			}
		}

		[PunRPC]
		public void SetInitialState(byte[] data)
		{
			initialState = InitialPlayerState.serializer.Deserialize(data);
			ProcessInitialState();
		}

		public IEnumerator Unstuck()
		{
			int count = positionMemory.Count;

			if (count <= 5)
			{
				transform.position = Vector3.zero;
				transform.rotation = Quaternion.identity;
				myCam.transform.rotation = Quaternion.identity;
				MouseLookUseCurrentTransform();
				characterController.center = transform.position;
			}
			else
			{
				while (count > 0)
				{
					int index = count - 1;
					transform.position = positionMemory[index];
					transform.rotation = rotationMemory[index];
					myCam.transform.rotation = cameraRotationMemory[index];
					MouseLookUseCurrentTransform();
					characterController.center = characterController.center;

					positionMemory.RemoveAt(index);
					rotationMemory.RemoveAt(index);
					cameraRotationMemory.RemoveAt(index);

					yield return new WaitForEndOfFrame();

					count = positionMemory.Count;
				}
			}

			fixingNaN = false;
		}
	}
}
