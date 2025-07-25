using HarmonyLib;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch]
	internal class UseNewItemTargetting
	{
		[HarmonyPatch(typeof(ItemTargeting), "DropItem2")]
		[HarmonyPrefix]
		static bool DropItem2(ref ItemTargeting __instance)
		{
			__instance.SendMessage("DropProp");
			return false;
		}

		[HarmonyPatch(typeof(DadItemTargeting), "DropItem2")]
		[HarmonyPrefix]
		static bool DropItem2(ref DadItemTargeting __instance)
		{
			__instance.SendMessage("DropProp");
			return false;
		}
	}
}
