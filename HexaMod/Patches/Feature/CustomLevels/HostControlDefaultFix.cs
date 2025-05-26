using HarmonyLib;

namespace HexaMod.Patches.Feature.CustomLevels
{
	[HarmonyPatch(typeof(HostControl))]
	internal class HostControlDefaultFix
	{

		[HarmonyPatch("RPCClicked")]
		[HarmonyPostfix]
		static void RPCClicked(ref HostControl __instance)
		{
			__instance.defaultOn = __instance.tog.isOn;
		}
	}
}
