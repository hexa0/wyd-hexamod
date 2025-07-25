using HexaMod.SDK.Levels.Scripts.Interactables;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Scripts.Character
{
	public class CharacterItemInteraction : Photon.MonoBehaviour
	{
		HexaPlayerController player;
		public ItemTargeting itemTargetting;
		public DadItemTargeting dadItemTargetting;

		internal static int defaultLayer = LayerMask.NameToLayer("Default"); // 0
		internal static int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast"); // 2
		internal static int waterLayer = LayerMask.NameToLayer("Water"); // 4
		internal static int useableLayer = LayerMask.NameToLayer("Useable"); // 8
		internal static int grabableLayer = LayerMask.NameToLayer("Grabable"); // 11
		internal static int dishesLayer = LayerMask.NameToLayer("Dishes"); // 18
		internal static int toyLayer = LayerMask.NameToLayer("Toy"); // 20
		internal static int clothesLayer = LayerMask.NameToLayer("Clothes"); // 24
		internal static int babyGrabableLayer = LayerMask.NameToLayer("BabyGrabable"); // 26
		internal static int cameraLayer = LayerMask.NameToLayer("Camera"); // 26
		internal static int babyGateLayer = LayerMask.NameToLayer("BabyGate"); // 28

		internal int pickupMask = (1 << grabableLayer) | (1 << babyGrabableLayer) | (1 << toyLayer) | (1 << dishesLayer);
		internal int grabMask = (1 << grabableLayer) | (1 << babyGrabableLayer) | (1 << toyLayer) | (1 << dishesLayer);
		internal int raycastIgnoreMask = (1 << ignoreRaycastLayer) | (1 << waterLayer) | (1 << cameraLayer);

		public Image InteractReticle => GameObject.Find("Reticle").GetComponent<Image>();
		public Text InteractText => GameObject.Find("SightText").GetComponent<Text>();
		public GameObject target;
		public string targetName;
		public RaycastHit hit;

		internal Transform leftHandItem;
		internal Transform rightHandItem;

		public Transform PrimaryItemTransform
		{
			get => player.rightHanded ? rightHandItem : leftHandItem;
			set	{	if (player.rightHanded)
						rightHandItem = value;
					else
						leftHandItem = value; }
		}
		public Transform SecondaryItemTransform
		{
			get => player.rightHanded ? leftHandItem : rightHandItem;
			set	{	if (player.rightHanded)
						leftHandItem = value;
					else
						rightHandItem = value; }
		}

		public static class ReticleColor
		{
			public static readonly Color Nothing = new Color(1f, 1f, 1f, 0.25f);
			public static readonly Color Useable = new Color(0f, 1f, 0f, 0.75f);
			// a nice to look at color according to a unity developer who added a comment to the Color.yellow constructor lol
			public static readonly Color Grabable = new Color(1f, 47f / 51f, 0.0156862754f, 0.75f);
		}

		public float reach = 3.5f;

		internal bool UseButtonDown => player.input.btn[7].isDown;

		public bool Buttered {
			get => butterTimer > 0f;
		}

		public float butterTimer = 0f;
		public float butterLength = 30f;

		void Awake()
		{
			player = GetComponent<HexaPlayerController>();
			itemTargetting = GetComponent<ItemTargeting>();
			dadItemTargetting = GetComponent<DadItemTargeting>();

			if (itemTargetting)
			{
				itemTargetting.enabled = false;
			}
			else if (dadItemTargetting)
			{
				dadItemTargetting.enabled = false;
			}
		}

		public static string GetTargetName(GameObject target)
		{
			string targetName = target.name;

			if (targetName.Length > 5)
			{
				string lastChars = targetName.Substring(targetName.Length - 5);

				if (int.TryParse(lastChars, out _))
				{
					targetName = targetName.Substring(0, targetName.Length - 5);
				}
			}

			return targetName;
		}

		public PickUp heldProp;

		void Update()
		{
			// something is causing these to get re-enabled and i can't be bothered to write a patch to log the source of the setter so here we are
			if (itemTargetting)
			{
				itemTargetting.enabled = false;
			}
			else if (dadItemTargetting)
			{
				dadItemTargetting.enabled = false;
			}

			if (Buttered)
			{
				butterTimer -= Time.deltaTime;
			}

			Transform cameraTransform = player.myCam.transform;

			int oldLayer = gameObject.layer;
			gameObject.layer = ignoreRaycastLayer;
			bool didHit = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, reach, ~raycastIgnoreMask);
			gameObject.layer = oldLayer;

			InteractText.text = "";
			InteractReticle.color = ReticleColor.Nothing;
			if (didHit && !SecondaryItemTransform && !heldProp)
			{
				target = hit.transform.gameObject;
				targetName = GetTargetName(target);
				
				InteractText.text = $"{targetName}:{LayerMask.LayerToName(target.layer)}:{target.tag}";

				foreach (Behaviour behavior in target.GetComponents<Behaviour>())
				{
					if (behavior as ICustomPlayerInteractable != null)
					{
						ICustomPlayerInteractable customPlayerInteractable = behavior as ICustomPlayerInteractable;
					
						if (customPlayerInteractable.CanInteract(player))
						{
							InteractText.text = customPlayerInteractable.ReticleText(player);
							InteractReticle.color = customPlayerInteractable.ReticleColor(player);

							if (UseButtonDown)
							{
								PhotonView.Get(target).RPC("CustomInteractRPC", PhotonTargets.All);
							}
						}

						return; // logic overidden by ICustomPlayerInteractable
					}
				}

				if (target.layer == useableLayer)
				{
					if (target.tag == "Use")
					{
						if (PrimaryItemTransform)
						{
							InteractText.text = $"Use {GetTargetName(PrimaryItemTransform.gameObject)} on {targetName}";
							InteractReticle.color = ReticleColor.Useable;

							if (UseButtonDown)
							{
								player.ActionMessage($"You use {GetTargetName(PrimaryItemTransform.gameObject)} on the {targetName}");
								target.SendMessage("Interact", gameObject, SendMessageOptions.DontRequireReceiver);
							}
						}
						else
						{
							InteractText.text = $"{targetName}";
							InteractReticle.color = ReticleColor.Useable;
						}
					}
					else
					{
						InteractText.text = $"{target.tag} {targetName}";
						InteractReticle.color = ReticleColor.Useable;

						if (UseButtonDown)
						{
							player.ActionMessage($"You {target.tag} the {targetName}");
							target.SendMessage("Interact", gameObject, SendMessageOptions.DontRequireReceiver);
						}
					}
				}
				else if (((1 << target.layer) & grabMask) != 0)
				{
					if (target.tag != "Grab" && target.tag != "Food")
					{
						if (((1 << target.layer) & pickupMask) != 0)
						{
							InteractText.text = $"Pick Up {targetName}";
							InteractReticle.color = ReticleColor.Grabable;

							if (UseButtonDown)
							{
								player.ActionMessage($"You pick up the {targetName}");
								player.PickUpProp(target);
							}
						}
					}
					else
					{
						InteractText.text = $"Grab {targetName}";
						InteractReticle.color = ReticleColor.Grabable;

						if (UseButtonDown)
						{
							player.ActionMessage($"You grab the {targetName}");
							player.Hold(target);
						}
					}
				}
			}
			else
			{
				if (heldProp && UseButtonDown)
				{
					player.DropProp();
				}
			}
		}
	}
}
