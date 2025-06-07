using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(DishwareCount))]
	internal class PutAwayChoreFix
	{
		[HarmonyPatch("CheckIfDone")]
		[HarmonyPrefix]
		static bool CheckIfDone(object choreDoer, ref DishwareCount __instance)
		{
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
						powerups.DisplayComplete();
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
