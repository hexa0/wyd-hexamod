using System.Reflection;
using HarmonyLib;
using HexaMod.API.Util.WhosYourDaddy;
using HexaMod.API.Voice.Script;
using HexaMod.Patches.Feature;
using HexaMod.Patches.Fixes;
using HexaMod.Scripts.Multiplayer.Lobby;
using HexaMod.Scripts.Multiplayer.SerializableObjects;
using HexaMod.Scripts.Persistent;
using HexaMod.Scripts.Util;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

namespace HexaMod.Scripts.Character
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
			public Traverse<float> JumpSpeed { get; set; }
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
			public Traverse<AudioSource> AudioSource { get; set; }
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
				JumpSpeed = traverse.Field<float>("m_JumpSpeed");
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
				AudioSource = traverse.Field<AudioSource>("m_AudioSource");
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
		public float JumpSpeed
		{
			get => fields.JumpSpeed.Value;
			set => fields.JumpSpeed.Value = value;
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
		public AudioSource AudioSource
		{
			get => fields.AudioSource.Value;
			set => fields.AudioSource.Value = value;
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
		internal MethodInfo baseMovementUpdate = typeof(FirstPersonController).GetMethod("FixedUpdate", BindingFlags.NonPublic | BindingFlags.Instance);
		internal MethodInfo baseUpdateCameraPosition = typeof(FirstPersonController).GetMethod("UpdateCameraPosition", BindingFlags.NonPublic | BindingFlags.Instance);

		public NetworkedSoundBehavior networkedSound;
		public CameraController cameraController;
		public CharacterController characterController;
		public CharacterModelSwapper characterModelSwapper;
		public CharacterItemInteraction characterItemInteraction;
		public Crouch crouch;
		public DadAnimator dadAnimator;
		public BabyAnimator babyAnimator;
		public DeathCam deathCam;
		public AudioListener audioListener;
		public ActionInput input;
		public PlayerVoiceEmitterRPC voiceEmitter;

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

		public bool noclip = false;
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
			networkedSound = gameObject.AddComponent<NetworkedSoundBehavior>();
			cameraController = GetComponent<CameraController>();
			characterModelSwapper = GetComponent<CharacterModelSwapper>();
			characterController = GetComponent<CharacterController>();
			characterItemInteraction = GetComponent<CharacterItemInteraction>();
			crouch = GetComponent<Crouch>();
			dadAnimator = GetComponentInChildren<DadAnimator>();
			babyAnimator = GetComponentInChildren<BabyAnimator>();
			deathCam = GetComponentInChildren<DeathCam>();
			audioListener = GetComponentInChildren<AudioListener>();
			voiceEmitter = GetComponent<PlayerVoiceEmitterRPC>();

			if (View && View.isMine)
			{
				HexaGlobal.networkManager.playerObj = gameObject;
				input = HexaGlobal.networkManager.player1Input;
				input.igmh = GetComponentInChildren<InGameMenuHelper>();
				input.myPlayer = this;
				input.dadItem = characterItemInteraction.dadItemTargetting;
				input.babyItem = characterItemInteraction.itemTargetting;
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
			StartCoroutine(ActionText.ActionDone(text));
		}

		/// <summary>
		/// holds the specified gameObject as an item
		/// </summary>
		/// <param name="toHold">the gameObject to grab up.</param>
		/// <param name="useHandPrimary">whether to pickup with the primary hand or not.</param>
		public void Hold(GameObject toHold, bool useHandPrimary = true)
		{
			if (characterItemInteraction.Buttered)
			{
				ActionMessage($"The {CharacterItemInteraction.GetTargetName(toHold)} slips through your buttery fingers.");
				return;
			}
			else
			{
				if (useHandPrimary)
				{
					if (!characterItemInteraction.PrimaryItemTransform)
					{
						toHold.SendMessage("Grab", SendMessageOptions.DontRequireReceiver);
						toHold.transform.SetParent(PrimaryHoldTransform, true);
						toHold.transform.localPosition = Vector3.zero;
						toHold.transform.localRotation = Quaternion.identity;
						toHold.layer = CharacterItemInteraction.ignoreRaycastLayer;
						characterItemInteraction.PrimaryItemTransform = toHold.transform;
					}
				}
				else
				{
					if (!characterItemInteraction.SecondaryItemTransform)
					{
						toHold.SendMessage("Grab", SendMessageOptions.DontRequireReceiver);
						toHold.transform.SetParent(SecondaryHoldTransform, true);
						toHold.transform.localPosition = Vector3.zero;
						toHold.transform.localRotation = Quaternion.identity;
						toHold.layer = CharacterItemInteraction.ignoreRaycastLayer;
						characterItemInteraction.SecondaryItemTransform = toHold.transform;
					}
				}
			}
		}

		public void DropPrimaryItem()
		{
			if (characterItemInteraction.PrimaryItemTransform)
			{
				ActionMessage($"You drop the {CharacterItemInteraction.GetTargetName(characterItemInteraction.PrimaryItemTransform.gameObject)}");
				characterItemInteraction.PrimaryItemTransform.SetParent(ItemSpawnerParent.parent, true);
				characterItemInteraction.PrimaryItemTransform = null;
			}
		}

		public void DropSecondaryItem()
		{
			if (characterItemInteraction.SecondaryItemTransform)
			{
				ActionMessage($"You drop the {CharacterItemInteraction.GetTargetName(characterItemInteraction.SecondaryItemTransform.gameObject)}");
				characterItemInteraction.SecondaryItemTransform.SetParent(ItemSpawnerParent.parent, true);
				characterItemInteraction.SecondaryItemTransform = null;
			}
		}

		/// <summary>
		/// picks up the specified gameObject as a prop
		/// </summary>
		/// <param name="toPickUp">the gameObject to pick up.</param>
		public void PickUpProp(GameObject toPickUp)
		{
			if (!characterItemInteraction.heldProp)
			{
				characterItemInteraction.heldProp = toPickUp.GetComponent<PickUp>();
				toPickUp.SendMessage("PickUp", transform, SendMessageOptions.DontRequireReceiver);
			}
		}

		public void DropProp()
		{
			if (characterItemInteraction.heldProp)
			{
				ActionMessage($"You drop the {CharacterItemInteraction.GetTargetName(characterItemInteraction.heldProp.gameObject)}");
				characterItemInteraction.heldProp.SendMessage("PutDown");
				characterItemInteraction.heldProp = null;
			}
		}

		public virtual void UpdateCameraPosition(float speed)
		{
			cameraController.speed = speed;
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
					baseMovementUpdate.Invoke(this, null);
				}
				else
				{
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
			}
		}

		public virtual void FixedUpdate()
		{
			// not used currently but is required to disable the original FixedUpdate method from being called
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

			string shirtColor = PlayerPrefs.GetString($"HMV2_{teamSelector}ShirtColor", defaultShirtColor);
			string skinColor = PlayerPrefs.GetString($"HMV2_{teamSelector}SkinColor", defaultSkinColor);

			initialState = new InitialPlayerState()
			{
				shirtColor = new SerializableColor(new Color().FromHex(shirtColor)),
				skinColor = new SerializableColor(new Color().FromHex(skinColor)),
				characterModel = PlayerPrefs.GetString($"HMV2_{teamSelector}CharacterModel", "default"),
				shirtMaterial = PlayerPrefs.GetString($"HMV2_{teamSelector}ShirtMaterial", "default")
			};

			View.RPC("SetInitialState", PhotonTargets.AllBuffered, new object[] { InitialPlayerState.serializer.Serialize(initialState) });
		}

		[PunRPC]
		public void SetInitialState(byte[] data)
		{
			initialState = InitialPlayerState.serializer.Deserialize(data);
			ProcessInitialState();
		}

		[PunRPC]
		public void FixNan(Vector3 characterPosition, Quaternion characterRotation, Vector3 cameraPosition, Quaternion cameraRotation)
		{
			NaNFixBehavior nanFixBehavior = gameObject.AddComponent<NaNFixBehavior>();
			nanFixBehavior.firstPersonController = GetComponent<FirstPersonController>();
			nanFixBehavior.characterPosition = characterPosition;
			nanFixBehavior.characterRotation = characterRotation;
			nanFixBehavior.cameraPosition = cameraPosition;
			nanFixBehavior.cameraRotation = cameraRotation;
		}
	}
}
