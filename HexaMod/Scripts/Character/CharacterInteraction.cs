using System.Linq;
using HexaMod.Scripts.Character.Controller.Character;
using HexaMod.SDK.Levels.Scripts.Interactables;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Scripts.Character
{
	public class CharacterInteraction : Photon.MonoBehaviour
	{
		HexaPlayerController player;
		public ItemTargeting itemTargetting;
		public DadItemTargeting dadItemTargetting;

		internal static readonly bool ENABLE_DEBUG_TEXT = false;

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
		internal static string[] consumables = new string[] {
			"Eat",
			"Drink",
			"Empty",
			"Douse Body In"
		};

		internal int pickupMask = (1 << grabableLayer) | (1 << babyGrabableLayer) | (1 << toyLayer) | (1 << dishesLayer);
		internal int grabMask = (1 << grabableLayer) | (1 << babyGrabableLayer) | (1 << toyLayer) | (1 << dishesLayer);
		internal int raycastIgnoreMask = (1 << ignoreRaycastLayer) | (1 << waterLayer) | (1 << cameraLayer);
		internal bool canConsume = false;

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
			HexaPlayerController targetPlayer = target.GetComponent<HexaPlayerController>();

			if (targetPlayer != null)
			{
				return targetPlayer.playerName;
			}

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

		public ICustomPlayerInteractable GetCustomInteractable(GameObject gameObject)
		{
			foreach (Behaviour behavior in gameObject.GetComponents<Behaviour>())
			{
				if (behavior as ICustomPlayerInteractable != null)
				{
					return behavior as ICustomPlayerInteractable;
				}
			}

			return null;
		}

		int _oldLayer;
		public void PreRaycastSetup()
		{
			_oldLayer = gameObject.layer;
			gameObject.layer = ignoreRaycastLayer;
		}
		public void PostRaycastSetup()
		{
			gameObject.layer = _oldLayer;
		}

		bool UpdateCustomItemInteraction()
		{
			bool customItemsOverideLogic = false;
			customItemsOverideLogic |= CheckItem(leftHandItem);
			customItemsOverideLogic |= CheckItem(rightHandItem);

			return customItemsOverideLogic;
		}

		bool CheckItem(Transform item)
		{
			ICustomPlayerInteractable customPlayerInteractable = null;

			if (item)
			{
				customPlayerInteractable = GetCustomInteractable(item.gameObject);
			}

			if (customPlayerInteractable != null)
			{
				PreRaycastSetup();
				Physics.Raycast(player.myCam.transform.position, player.myCam.transform.forward, out hit, reach, ~raycastIgnoreMask);
				PostRaycastSetup();

				GameObject target = hit.transform?.gameObject;

				bool isTargetUsable = false;
				if (target && target.layer == useableLayer && target.tag == "Use")
				{
					InteractText.text = customPlayerInteractable.UseReticleText(player, target);
					InteractReticle.color = customPlayerInteractable.UseReticleColor(player, target);
					isTargetUsable = true;
				}

				if (UseButtonDown)
				{
					int targetId = -1;

					if (target)
					{
						PhotonView targetView = target.GetPhotonView();
						if (targetView)
						{
							targetId = targetView.viewID;
						}
					}

					PhotonView.Get(item).RPC("CustomUseRPC", PhotonTargets.All, targetId, isTargetUsable);
				}

				return isTargetUsable;
			}

			return false;
		}

		public void UpdateItemInteraction()
		{
			if (UpdateCustomItemInteraction())
			{
				return;
			}

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

			PreRaycastSetup();
			bool didHit = Physics.Raycast(player.myCam.transform.position, player.myCam.transform.forward, out hit, reach, ~raycastIgnoreMask);
			PostRaycastSetup();

			InteractText.text = ENABLE_DEBUG_TEXT ? $"{targetName}:{LayerMask.LayerToName(target.layer)}:{target.tag}" : "";
			InteractReticle.color = ReticleColor.Nothing;
			if (didHit && !heldProp)
			{
				target = hit.transform.gameObject;
				targetName = GetTargetName(target);

				ICustomPlayerInteractable customPlayerInteractable = GetCustomInteractable(target);

				if (customPlayerInteractable != null)
				{
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
								target.SendMessage("UseInteract", new string[] { PrimaryItemTransform.name, gameObject.name }, SendMessageOptions.DontRequireReceiver);
							}
						}
						else if (SecondaryItemTransform)
						{
							InteractText.text = $"Use {GetTargetName(SecondaryItemTransform.gameObject)} on {targetName}";
							InteractReticle.color = ReticleColor.Useable;

							if (UseButtonDown)
							{
								player.ActionMessage($"You use {GetTargetName(SecondaryItemTransform.gameObject)} on the {targetName}");
								target.SendMessage("UseInteract", new string[] { SecondaryItemTransform.name, gameObject.name }, SendMessageOptions.DontRequireReceiver);
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
				else if (canConsume && consumables.Contains(target.tag))
				{
					InteractText.text = $"{target.tag} {targetName}";
					InteractReticle.color = ReticleColor.Useable;

					if (UseButtonDown && target.tag != "Empty")
					{
						player.ActionMessage($"You {target.tag} the {targetName}");
						target.SendMessage("Interact", gameObject.name, SendMessageOptions.DontRequireReceiver);
					}
				}
				else if (((1 << target.layer) & grabMask) != 0)
				{
					if (target.tag != "Grab" && target.tag != "LeftGrab" && target.tag != "Food")
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
							if (player.Hold(target, target.GetComponent<LeftHand>() == null))
							{
								player.ActionMessage($"You grab the {targetName}");
							}
						}
					}
				}
				else
				{
					if (PrimaryItemTransform && UseButtonDown)
					{
						player.ActionMessage($"You drop the {GetTargetName(PrimaryItemTransform.gameObject)}");
						Transform item = PrimaryItemTransform;
						player.DropItem(item);
						item.position = hit.point + new Vector3(0f, 0.05f, 0f);
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
