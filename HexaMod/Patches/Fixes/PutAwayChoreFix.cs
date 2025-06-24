using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(DishwareCount))]
	internal class PutAwayChoreFix
	{
		[HarmonyPatch("AddDish")]
		[HarmonyPrefix]
		static void AddDish(ref DishwareCount __instance)
		{
			Traverse fields = Traverse.Create(__instance);

			if (fields.Field<bool>("choreStarted").Value && !__instance.isDone)
			{
				DadPowerUps powerups = HexaGlobal.networkManager.playerObj.GetComponent<DadPowerUps>();
				Traverse powerupFields = Traverse.Create(powerups);
				ActionText bigAction = powerupFields.Field<GameObject>("bigAction").Value.GetComponent<ActionText>();
				bigAction.ActionDone("Chore Completed");
				var audio = bigAction.GetComponent<AudioSource>();
				audio.clip = HexaGlobal.coreBundle.LoadAsset<AudioClip>($"Assets/ModResources/Core/Audio/Chore/ChoreCompletion{Mathf.Clamp(__instance.curDishCount + 1 + (16 - __instance.totalDishes), 0, 16)}.wav");
				audio.Play();
			}
		}

		[HarmonyPatch("SubtractDish")]
		[HarmonyPrefix]
		static void SubtractDish(ref DishwareCount __instance)
		{
			Traverse fields = Traverse.Create(__instance);

			if (fields.Field<bool>("choreStarted").Value && !__instance.isDone)
			{
				DadPowerUps powerups = HexaGlobal.networkManager.playerObj.GetComponent<DadPowerUps>();
				Traverse powerupFields = Traverse.Create(powerups);
				ActionText bigAction = powerupFields.Field<GameObject>("bigAction").Value.GetComponent<ActionText>();
				bigAction.ActionDone("Chore Completed");
				var audio = bigAction.GetComponent<AudioSource>();
				audio.clip = HexaGlobal.coreBundle.LoadAsset<AudioClip>($"Assets/ModResources/Core/Audio/Chore/ChoreCompletion{Mathf.Clamp(__instance.curDishCount - 1 + (16 - __instance.totalDishes), 0, 16)}.wav");
				audio.Play();
			}
		}

		[HarmonyPatch("CheckIfDone")]
		[HarmonyPrefix]
		static bool CheckIfDone(ref DishwareCount __instance)
		{
			// HexaMod.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/SmokeDetector.wav")
			if (__instance.curDishCount >= __instance.totalDishes && !__instance.isDone)
			{
				Mod.Print($"Chore \"{__instance.smallActionMessage}\" has finished");
				if (HexaGlobal.networkManager.isDad)
				{
					ChallengeManager ChallengeManager = null;

					if (GameObject.Find("ChallengeManager"))
					{
						ChallengeManager = GameObject.Find("ChallengeManager").GetComponent<ChallengeManager>();
					}

					if (ChallengeManager == null)
					{
						Mod.Print($"Trigger Random PowerUp!");
						DadPowerUps powerups = HexaGlobal.networkManager.playerObj.GetComponent<DadPowerUps>();
						Traverse fields = Traverse.Create(powerups);
						ActionText bigAction = fields.Field<GameObject>("bigAction").Value.GetComponent<ActionText>();
						bigAction.ActionDone("Chore Completed");
						powerups.SendMessage("RandomPowerUp");
					}
					else
					{
						Mod.Print($"Trigger ChallengeManager.FinishChallenge with ID {__instance.challengeId}");
						ChallengeManager.FinishChallenge(__instance.challengeId);
					}
				}


				__instance.isDone = true;
				Object.Destroy(__instance.GetComponent<DishTrigger>());
			}

			return false;
		}
	}
}
