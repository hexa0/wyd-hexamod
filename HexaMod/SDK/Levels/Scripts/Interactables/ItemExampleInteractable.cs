using HexaMod.API.Util.Migration;
using HexaMod.Scripts.Character;
using HexaMod.Scripts.Character.Controller.Character;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Interactables
{
	[UnityMigrationIdentifier("HexaMod.4fcd2a59-ca20-4ea9-ac11-f23d365ad36e")]
	[RequireComponent(typeof(Renderer), typeof(Collider), typeof(AudioSource))]
	internal class ItemExampleInteractable : CustomPlayerInteractable
	{
		public override string ReticleText(HexaPlayerController player) => $"Grab {GetName()}";
		public override Color ReticleColor(HexaPlayerController player) => CharacterInteraction.ReticleColor.Grabable;
		public override string UseReticleText(HexaPlayerController player, GameObject target) => $"{actionName} at {CharacterInteraction.GetTargetName(target)}";
		public override bool CanInteract(HexaPlayerController player) => true;

		public string actionName = "Squeak";
		public string actionNamePlural = "squeaked";

		AudioSource audioSource;

		public void Start()
		{
			audioSource = GetComponent<AudioSource>();
		}

		public override void CustomInteract(HexaPlayerController player)
		{
			if (player.Hold(gameObject, true))
			{
				player.ActionMessage($"You wield the power of the {GetName()}");
			}
		}

		public override void CustomUse(HexaPlayerController player, GameObject usedOn = null, bool usedOnIsUsable = false)
		{
			if (usedOn && usedOnIsUsable)
			{
				player.ActionMessage($"You {actionNamePlural} at {CharacterInteraction.GetTargetName(usedOn)}");

				audioSource.pitch = Random.Range(1.2f, 1.6f);
				audioSource.Play();
			}
			else
			{
				player.ActionMessage($"You {actionNamePlural}");
				audioSource.pitch = Random.Range(0.8f, 1.2f);
				audioSource.Play();
			}
		}
	}
}
