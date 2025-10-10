using HarmonyLib;
using HexaMod.API.UI;
using HexaMod.API.Util.WhosYourDaddy;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(PoolTrigger))]
	internal class PoolChoreFix
	{
		[HarmonyPatch("OnTriggerEnter")]
		[HarmonyPrefix]
		static bool OnTriggerEnter(ref PoolTrigger __instance, Collider other)
		{
			PoolTrigger self = __instance;
			PoolChore poolChore = self.triggerHolder.GetComponent<PoolChore>();

			FirstPersonController localPlayer;

			try
			{
				localPlayer = PlayerControllers.LocalPlayer;
			}
			catch
			{
				localPlayer = null;
			}

			if (localPlayer != null && self.poolObjs.Contains(other.gameObject))
			{
				poolChore.AddDish();

				PickUp otherPickup = other.GetComponent<PickUp>();

				if (otherPickup && otherPickup.lastHolder != string.Empty)
				{
					FirstPersonController playerHolding = PlayerControllers.GetPlayer(otherPickup.lastHolder);

					DadPowerUps powerUps = playerHolding.GetComponent<DadPowerUps>();
					powerUps.ChangeScore(-1);
				}
			}

			return false;
		}

		[HarmonyPatch("OnTriggerExit")]
		[HarmonyPrefix]
		static bool OnTriggerExit(ref PoolTrigger __instance, Collider other)
		{
			PoolTrigger self = __instance;
			PoolChore poolChore = self.triggerHolder.GetComponent<PoolChore>();

			FirstPersonController localPlayer;

			try
			{
				localPlayer = PlayerControllers.LocalPlayer;
			}
			catch
			{
				localPlayer = null;
			}

			if (localPlayer != null && self.poolObjs.Contains(other.gameObject))
			{
				PickUp otherPickup = other.GetComponent<PickUp>();

				if (otherPickup)
				{
					if (otherPickup.lastHolder == string.Empty)
					{
						GameObject poolSkimmer = GameObject.Find("PoolSkimmerHead").transform.parent.gameObject;

						// removed with pool skimmer
						if (poolSkimmer.transform.IsChildOf(localPlayer.transform))
						{
							DadPowerUps powerUps = localPlayer.GetComponent<DadPowerUps>();

							if (!SplitscreenUtil.IsInSplitscreen())
							{
								powerUps.ChangeScore(1);
							}

							if (HexaGlobal.networkManager.isDad)
							{
								Traverse powerupFields = Traverse.Create(powerUps);

								ActionText bigAction = powerupFields.Field<GameObject>("bigAction").Value.GetComponent<ActionText>();
								var audio = bigAction.GetComponent<AudioSource>();
								audio.clip = HexaGlobal.coreBundle.LoadAsset<AudioClip>($"Assets/ModResources/Core/Audio/Chore/ChoreCompletion{Mathf.Clamp(-poolChore.curPoolObjCount + poolChore.totalObjsForComplete - 1 + (16 - poolChore.totalObjsForComplete), 0, 16)}.wav");
								audio.Play();
							}

							string doer = HexaGlobal.networkManager.playerObj.name;

							otherPickup.SetChoreDoer(doer);
							poolChore.SubtractDish(doer);
						}
						else // removed by no-one, assign it to the host player?
						{
							FirstPersonController hostPlayer = PlayerControllers.HostPlayer;
							DadPowerUps powerUps = hostPlayer.GetComponent<DadPowerUps>();

							if (!SplitscreenUtil.IsInSplitscreen())
							{
								powerUps.ChangeScore(1);
							}

							string doer = hostPlayer.name;

							otherPickup.SetChoreDoer(doer);
							poolChore.SubtractDish(doer);
						}
					}
					else // removed by hand
					{
						string doer = otherPickup.lastHolder;

						DadPowerUps powerUps = PlayerControllers.GetPlayer(doer).GetComponent<DadPowerUps>();

						if (HexaGlobal.networkManager.isDad)
						{
							Traverse powerupFields = Traverse.Create(powerUps);

							ActionText bigAction = powerupFields.Field<GameObject>("bigAction").Value.GetComponent<ActionText>();
							var audio = bigAction.GetComponent<AudioSource>();
							audio.clip = HexaGlobal.coreBundle.LoadAsset<AudioClip>($"Assets/ModResources/Core/Audio/Chore/ChoreCompletion{Mathf.Clamp(-poolChore.curPoolObjCount + poolChore.totalObjsForComplete - 1 + (16 - poolChore.totalObjsForComplete), 0, 16)}.wav");
							audio.Play();
						}

						otherPickup.SetChoreDoer(doer);
						poolChore.SubtractDish(doer);
					}
				}
			}

			return false;
		}
	}
}
