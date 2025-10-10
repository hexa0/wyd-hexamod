using HarmonyLib;
using HexaMod.API.Util.Patching;

namespace HexaMod.Patches.Hooks
{
	[ModdedPatch]
	[HarmonyPatch(typeof(ParRotation))]
	public class HandleLookRotationExternally
	{
		public static bool runningExternally = false;

		[HarmonyPatch("LateUpdate")]
		[HarmonyPrefix]
		static bool LateUpdate()
		{
			return runningExternally;
		}
	}
}
