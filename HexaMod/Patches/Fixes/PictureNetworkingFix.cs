using HarmonyLib;
using HexaMapAssemblies;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(PictureJointBreak))]
	internal class PictureNetworkingFix
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref PictureJointBreak __instance)
		{
			if (__instance.GetComponent<PhotonView>() == null)
			{
				GlobalPhotonFactory.Register(__instance.gameObject, true);
			}

			if (__instance.GetComponent<NetworkMovementRB>() == null)
			{
				__instance.gameObject.AddComponent<NetworkMovementRB>();
			}
		}
	}
}
