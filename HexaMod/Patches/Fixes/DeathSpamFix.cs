using HarmonyLib;
namespace HexaMod.Patches.Hooks
{
	[HarmonyPatch(typeof(BabyStats))]
	internal class DeathSpamFix
	{
		[HarmonyPatch("Dead")]
		[HarmonyPrefix]
		static bool Dead(ref BabyStats __instance)
		{
			return !__instance.GetComponent<DeathManager>().isDead;
		}
	}
}