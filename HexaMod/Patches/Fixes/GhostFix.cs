using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class GhostFix
	{
		[HarmonyPatch("OnPhotonPlayerDisconnected")]
		[HarmonyPrefix]
		static bool OnPhotonPlayerDisconnected()
		{
			return false;
		}
	}
}
