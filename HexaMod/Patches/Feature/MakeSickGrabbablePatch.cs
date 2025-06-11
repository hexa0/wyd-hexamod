using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(MakeSick))]
	internal class MakeSickGrabbablePatch
	{

		[HarmonyPatch("RPCInteract")]
		[HarmonyPrefix]
		static bool RPCInteract(string input, ref MakeSick __instance)
		{
			GameObject player = GameObject.Find(input);

			if (player.name.Substring(0, 3) == "Dad" && __instance.tag == "Eat")
			{
				return false;
			}

			return true;
		}
	}
}
