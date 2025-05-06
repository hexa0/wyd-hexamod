using System.Collections;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Utility;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(LerpControlledBob))]
	internal class SmootherBob
	{
		[HarmonyPatch("DoBobCycle")]
		[HarmonyPostfix]
		public static IEnumerator DoBobCycle(IEnumerator result, LerpControlledBob __instance)
		{
			Traverse fields = Traverse.Create(__instance);
			Traverse<float> m_Offset = fields.Field<float>("m_Offset");
			float t = 0f;
			while (t < __instance.BobDuration)
			{
				m_Offset.Value = Mathf.Lerp(0f, __instance.BobAmount, t / __instance.BobDuration);
				t += Time.deltaTime;
				yield return 0;
			}
			t = 0f;
			while (t < __instance.BobDuration)
			{
				m_Offset.Value = Mathf.Lerp(__instance.BobAmount, 0f, t / __instance.BobDuration);
				t += Time.deltaTime;
				yield return 0;
			}
			m_Offset.Value = 0f;
			yield break;
		}
	}
}
