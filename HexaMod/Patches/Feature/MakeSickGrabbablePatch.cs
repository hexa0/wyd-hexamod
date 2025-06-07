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

			// this works but throws errors that don't matter
			// ideally we should patch everything to use a different name to prevent the name collision throwing errors
			// also the ""error"" isn't even an error it's actually a warning but they log it as an error despite nothing actually breaking

			if (player.name.Substring(0, 3) == "Dad" && __instance.tag == "Eat")
			{
				return false;
			}

			return true;
		}
	}
}
