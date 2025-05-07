using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(FpsDisplay))]
	internal class FpsDisplayFix
	{
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool Update(ref FpsDisplay __instance)
		{
			__instance.GetComponent<Text>().text = (1.0f / Time.smoothDeltaTime).ToString("f0");
			return false;
		}
	}
}
