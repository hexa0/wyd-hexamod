using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Debugging
{
	[HarmonyPatch(typeof(GameObject))]
	public class FindCounter
	{
		public static int findMax = 0;
		public static int find = 0;
		[HarmonyPatch("Find")]
		[HarmonyPostfix]
		static void Find()
		{
			if (findMax < find)
			{
				findMax = find;
			}

			find++;
		}
	}
}
