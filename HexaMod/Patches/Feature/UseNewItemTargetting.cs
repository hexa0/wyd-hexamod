using HarmonyLib;
using HexaMod.API.Util.Patching;

#pragma warning disable IDE0060
namespace HexaMod.Patches.Feature
{
	[ModdedPatch]
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

		[HarmonyPatch(typeof(ItemTargeting), "Start")]
		[HarmonyPrefix]
		static bool Start(ref ItemTargeting __instance)
		{
			return false;
		}

		[HarmonyPatch(typeof(ItemTargeting), "Update")]
		[HarmonyPrefix]
		static bool Update(ref ItemTargeting __instance)
		{
			return false;
		}

		[HarmonyPatch(typeof(DadItemTargeting), "DropItem2")]
		[HarmonyPrefix]
		static bool DropItem2(ref DadItemTargeting __instance)
		{
			__instance.SendMessage("DropProp");
			return false;
		}

		[HarmonyPatch(typeof(DadItemTargeting), "Start")]
		[HarmonyPrefix]
		static bool Start(ref DadItemTargeting __instance)
		{
			return false;
		}

		[HarmonyPatch(typeof(DadItemTargeting), "Update")]
		[HarmonyPrefix]
		static bool Update(ref DadItemTargeting __instance)
		{
			return false;
		}
	}
}
