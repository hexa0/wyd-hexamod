using HexaMod.API.Util.Migration;
using HexaMod.Scripts.Character;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Interactables
{
	[UnityMigrationIdentifier("HexaMod.4fcd2a59-ca20-4ea9-ac11-f23d365ad36e")]
	[RequireComponent(typeof(Renderer), typeof(Collider), typeof(AudioSource))]
	internal class ItemExampleInteractable : CustomPlayerInteractable
	{
		public override Color ReticleColor(HexaPlayerController player) => CharacterItemInteraction.ReticleColor.Grabable;
		public override string ReticleText(HexaPlayerController player) => $"Grab {GetName()}";
		public override bool CanInteract(HexaPlayerController player) => true;

		public AudioSource audioSource;

		public void Start()
		{
			audioSource = GetComponent<AudioSource>();
		}

		public override void CustomInteract(HexaPlayerController player)
		{
			player.ActionMessage($"You grab the {GetName()}");
			player.Hold(gameObject, true);
		}

		public override void CustomUse(HexaPlayerController player)
		{
			audioSource.pitch = Random.Range(0.8f, 1.2f);
			audioSource.Play();
		}
	}
}
