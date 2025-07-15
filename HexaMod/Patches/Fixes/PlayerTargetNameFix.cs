using HarmonyLib;
using HexaMod.UI;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class PlayerTargetNameFix
	{
		[HarmonyPatch(typeof(ItemTargeting), "Update")]
		[HarmonyPostfix]
		static void Update(ref ItemTargeting __instance)
		{
			if (SplitscreenUtil.IsInSplitscreen()) { return; }

			var privateFields = Traverse.Create(__instance);

			var curTarget = privateFields.Field<GameObject>("curTarget");

			if (curTarget.Value)
			{
				var playerController = curTarget.Value.GetComponentInChildren<FirstPersonController>();
				if (playerController)
				{
					__instance.textComp.text = playerController.playerName;
				}
			}
		}

		[HarmonyPatch(typeof(DadItemTargeting), "Update")]
		[HarmonyPostfix]
		static void Update(ref DadItemTargeting __instance)
		{
			if (SplitscreenUtil.IsInSplitscreen()) { return; }

			var privateFields = Traverse.Create(__instance);

			var curTarget = privateFields.Field<GameObject>("curTarget");

			if (curTarget.Value)
			{
				var playerController = curTarget.Value.GetComponentInChildren<FirstPersonController>();
				if (playerController)
				{
					__instance.textComp.text = playerController.playerName;
				}
			}
		}
	}
}
