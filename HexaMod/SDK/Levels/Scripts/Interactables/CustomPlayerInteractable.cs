using HexaMod.API.Util.Migration;
using HexaMod.API.Util.WhosYourDaddy;
using HexaMod.Scripts.Character;
using HexaMod.Scripts.Character.Controller.Character;
using UnityEngine;

namespace HexaMod.SDK.Levels.Scripts.Interactables
{
	[UnityMigrationIdentifier("HexaMod.5fbf637b-d698-45fb-af38-607a9d164d86")]
	public class CustomPlayerInteractable : MigratablePhotonMonoBehavior, ICustomPlayerInteractable
	{
		#region ICustomPlayerInteractable Default Implemenation
		public virtual bool CanInteract(HexaPlayerController player) => true;
		public virtual bool CanUseOn(HexaPlayerController player, GameObject target) => true;

		public virtual string ReticleText(HexaPlayerController player) => $"Interact With {name} as teamSelector {player.teamSelector}";
		public virtual Color ReticleColor(HexaPlayerController player) => CharacterInteraction.ReticleColor.Useable;
		public virtual string UseReticleText(HexaPlayerController player, GameObject target) => $"Use {CharacterInteraction.GetTargetName(gameObject)} on {CharacterInteraction.GetTargetName(target)}";
		public virtual Color UseReticleColor(HexaPlayerController player, GameObject target) => CharacterInteraction.ReticleColor.Useable;

		public virtual void CustomInteract(HexaPlayerController player) => player.ActionMessage($"Interacted with {name} as teamSelector {player.teamSelector}.");
		[PunRPC]
		public virtual void CustomInteractRPC(PhotonMessageInfo messageInfo)
		{
			foreach (HexaPlayerController player in PlayerControllers.GetPlayers())
			{
				if (player.View.owner == messageInfo.sender)
				{
					CustomInteract(player);
					return;
				}
			}
		}
		public virtual void CustomUse(HexaPlayerController player, GameObject usedOn = null, bool usedOnIsUsable = false) => player.ActionMessage($"Used {name} as teamSelector {player.teamSelector}.");
		[PunRPC]
		public virtual void CustomUseRPC(int usedOnId, bool usedOnIsUsable, PhotonMessageInfo messageInfo) {
			GameObject usedOn = null;

			if (usedOnId != -1)
			{
				usedOn = PhotonView.Find(usedOnId).gameObject;
			}

			foreach (HexaPlayerController player in PlayerControllers.GetPlayers())
			{
				if (player.View.owner == messageInfo.sender)
				{
					CustomUse(player, usedOn, usedOnIsUsable);
					return;
				}
			}
		}
		#endregion

		#region CustomPlayerInteractable Utils
		public string GetName() => CharacterInteraction.GetTargetName(gameObject);
		#endregion
	}

	public interface ICustomPlayerInteractable
	{
		bool CanInteract(HexaPlayerController player);
		bool CanUseOn(HexaPlayerController player, GameObject target);
		string ReticleText(HexaPlayerController player);
		Color ReticleColor(HexaPlayerController player);
		string UseReticleText(HexaPlayerController player, GameObject target);
		Color UseReticleColor(HexaPlayerController player, GameObject target);

		void CustomInteract(HexaPlayerController player);
		[PunRPC]
		void CustomInteractRPC(PhotonMessageInfo messageInfo);

		[PunRPC]
		void CustomUse(HexaPlayerController player, GameObject usedOn = null, bool usedOnIsUsable = false);
	}
}
