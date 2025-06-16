using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(DadAnimator))]
	internal class KeepDefaultAnimator
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool Start(ref DadAnimator __instance)
		{
			Traverse fields = Traverse.Create(__instance);
			Traverse<Animator> animator = fields.Field<Animator>("anim");

			if (animator.Value == null)
			{
				animator.Value = __instance.GetComponent<Animator>();
			}

			return false;
		}
	}
}
