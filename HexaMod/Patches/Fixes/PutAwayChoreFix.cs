using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(DishwareCount))]
	internal class PutAwayChoreFix
	{
		[HarmonyPatch("AddDish")]
		[HarmonyPrefix]
		static void AddDish(object choreDoer, ref DishwareCount __instance)
		{
			if (__instance.curDishCount > 0)
			{
				DadPowerUps powerups = HexaMod.networkManager.playerObj.GetComponent<DadPowerUps>();
				Traverse fields = Traverse.Create(powerups);
				ActionText bigAction = fields.Field<GameObject>("bigAction").Value.GetComponent<ActionText>();
				bigAction.ActionDone("Chore Completed");
				var audio = bigAction.GetComponent<AudioSource>();
				audio.clip = HexaMod.coreBundle.LoadAsset<AudioClip>($"Assets/ModResources/Core/Audio/Chore/ChoreCompletion{(__instance.curDishCount + 1) + (16 - __instance.totalDishes)}.wav");
				audio.Play();
			}
		}

		[HarmonyPatch("CheckIfDone")]
		[HarmonyPrefix]
		static bool CheckIfDone(object choreDoer, ref DishwareCount __instance)
		{
			// HexaMod.coreBundle.LoadAsset<AudioClip>("Assets/ModResources/Core/Audio/SmokeDetector.wav")
			if (__instance.curDishCount >= __instance.totalDishes && !__instance.isDone)
			{
				Mod.Print($"Chore \"{__instance.smallActionMessage}\" has finished");
				if (HexaMod.networkManager.isDad)
				{
					ChallengeManager ChallengeManager = null;

					if (GameObject.Find("ChallengeManager"))
					{
						ChallengeManager = GameObject.Find("ChallengeManager").GetComponent<ChallengeManager>();
					}

					if (ChallengeManager == null)
					{
						Mod.Print($"Trigger Random PowerUp!");
						DadPowerUps powerups = HexaMod.networkManager.playerObj.GetComponent<DadPowerUps>();
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
