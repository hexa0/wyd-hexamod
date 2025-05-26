using System;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(AchievementManager))]
	internal class OfflineAchievementFix
	{
		[HarmonyPatch("UnlockAchievement")]
		[HarmonyPrefix]
		static bool UnlockAchievement(ref AchievementManager __instance, int input)
		{
			string achievement = "ACHIEVEMENT_" + (input + 1).ToString("000");
			PlayerPrefs.SetInt(achievement, 1);
			MonoBehaviour.print(achievement);
			MonoBehaviour.print("Achievement Unlocked: " + __instance.achievements[input].name);
			UnlockAchievementSteam(achievement);

			return false;
		}

		static void UnlockAchievementSteam(string name)
		{
			try
			{
				SteamUserStats.SetAchievement(name);
				SteamUserStats.StoreStats();
			}
			catch
			{
				Mod.Warn("failed to unlock an achievement on the steam side, this error has been skipped to avoid game breaking bugs when playing offline/without launching through steam");
			}
		}
	}
}
