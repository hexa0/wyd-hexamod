using System.Collections;
using HexaMod.API.Util.Migration;
using HexaMod.Scripts.Character.Controller.Character;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Interactables
{
	[UnityMigrationIdentifier("HexaMod.daa83d8b-6c4d-48c0-9228-702ca85f7b51")]
	[RequireComponent(typeof(Renderer), typeof(Collider), typeof(AudioSource))]
	public class BanishExampleInteractable : CustomPlayerInteractable
	{
		public override string ReticleText(HexaPlayerController player) => $"Banish the {GetName()} to the shadow realm";
		public override Color ReticleColor(HexaPlayerController player) => Color.HSVToRGB(Time.time % 1f, 1f, 1f);
		public override bool CanInteract(HexaPlayerController player)
		{
			return player.teamSelector != "B";
		}

		[PunRPC]
		public void AfterBanish()
		{
			Destroy(gameObject);
		}

		public IEnumerator Banish()
		{
			gameObject.GetComponent<Renderer>().enabled = false;
			gameObject.GetComponent<Collider>().enabled = false;
			gameObject.GetComponent<AudioSource>().Play();
			yield return new WaitForSeconds(5f);
			photonView.RPC("AfterBanish", PhotonTargets.AllBuffered);
		}

		public override void CustomInteract(HexaPlayerController player)
		{
			player.ActionMessage($"You banished the {GetName()} to the shadow realm");
			StartCoroutine(Banish());
		}
	}
}
