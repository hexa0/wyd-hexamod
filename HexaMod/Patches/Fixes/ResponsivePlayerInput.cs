using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
	[HarmonyPatch]
	internal class ResponsivePlayerInput
	{
		[HarmonyPatch(typeof(ActionInput), "Update")]
		[HarmonyPostfix]
		static void Update(ref ActionInput __instance)
		{
			if (__instance.myPlayer)
			{
				if (!__instance.deathCam)
				{
					// this code is fucked, but this is what the game does so i have to do it this way
					// this is essentially just removing the lerp
					__instance.myPlayer.xAxis = ((!__instance.btn[2].isHeld) ? 0 : 1) + ((!__instance.btn[3].isHeld) ? 0 : (-1)) + +Mathf.Abs(__instance.btn[2].axisVal) + -Mathf.Abs(__instance.btn[3].axisVal);
					__instance.myPlayer.yAxis = ((!__instance.btn[0].isHeld) ? 0 : 1) + ((!__instance.btn[1].isHeld) ? 0 : (-1)) + Mathf.Abs(__instance.btn[0].axisVal) + -Mathf.Abs(__instance.btn[1].axisVal);
				}
			}
		}
	}

	// all gemini because i can't do IL patching for shit

	[HarmonyPatch(typeof(FirstPersonController), "GetInput")]
	public static class ResponsivePlayerInputRemoveNormalization
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codeMatcher = new CodeMatcher(instructions)
				.MatchForward(
					false,
					new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Vector2), "Normalize"))
				)
				.RemoveInstruction()
				.InsertAndAdvance(new CodeInstruction(OpCodes.Nop))
				.InsertAndAdvance(new CodeInstruction(OpCodes.Pop));

			return codeMatcher.InstructionEnumeration();
		}
	}

	// this was especially a pain to get working lol

	[HarmonyPatch(typeof(FirstPersonController), "FixedUpdate")]
	public static class ProjectOnPlanePatch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patchedInstructions = new List<CodeInstruction>(instructions);

			MethodInfo getNormalMethod = AccessTools.PropertyGetter(typeof(RaycastHit), "normal");
			MethodInfo getUpMethod = AccessTools.PropertyGetter(typeof(Vector3), "up");
			MethodInfo getNormalizedMethod = AccessTools.PropertyGetter(typeof(Vector3), "normalized");

			int getNormalIndex = -1;
			int getNormalizedIndex = -1;

			for (int i = 0; i < patchedInstructions.Count; i++)
			{
				if (patchedInstructions[i].opcode == OpCodes.Call)
				{
					if (patchedInstructions[i].OperandIs(getNormalMethod))
					{
						getNormalIndex = i;
					}
					if (patchedInstructions[i].OperandIs(getNormalizedMethod))
					{
						getNormalizedIndex = i;
					}
				}
			}

			if (getNormalIndex != -1)
			{
				if (getNormalIndex > 0)
				{
					CodeInstruction prevInstruction = patchedInstructions[getNormalIndex - 1];
					bool isLdloca2 = prevInstruction.opcode == OpCodes.Ldloca_S &&
								   (prevInstruction.operand is LocalBuilder lb && lb.LocalIndex == 2 ||
									prevInstruction.operand is byte b && b == 2 ||
									prevInstruction.operand is short s && s == 2 ||
									prevInstruction.operand is int i && i == 2);

					if (isLdloca2)
					{
						patchedInstructions.RemoveAt(getNormalIndex - 1);
						getNormalIndex--;
					}
				}

				patchedInstructions[getNormalIndex].operand = getUpMethod;
			}

			getNormalizedIndex = -1;
			for (int i = 0; i < patchedInstructions.Count; i++)
			{
				if (patchedInstructions[i].opcode == OpCodes.Call && patchedInstructions[i].OperandIs(getNormalizedMethod))
				{
					getNormalizedIndex = i;
					break;
				}
			}


			if (getNormalizedIndex != -1 && getNormalizedIndex > 0 && getNormalizedIndex + 1 < patchedInstructions.Count)
			{
				CodeInstruction prevInstruction = patchedInstructions[getNormalizedIndex - 1];
				CodeInstruction nextInstruction = patchedInstructions[getNormalizedIndex + 1];


				bool isLdloca4 = prevInstruction.opcode == OpCodes.Ldloca_S &&
							   (prevInstruction.operand is LocalBuilder lb && lb.LocalIndex == 4 ||
								prevInstruction.operand is byte b && b == 4 ||
								prevInstruction.operand is short s && s == 4 ||
								prevInstruction.operand is int i && i == 4);

				bool isStloc1 = nextInstruction.opcode == OpCodes.Stloc_1;

				if (isLdloca4 && isStloc1)
				{
					patchedInstructions.RemoveAt(getNormalizedIndex - 1);
					patchedInstructions.RemoveAt(getNormalizedIndex - 1);
					CodeInstruction loadVectorInstruction = new CodeInstruction(OpCodes.Ldloc_S, (byte)4);

					patchedInstructions.Insert(getNormalizedIndex - 1, loadVectorInstruction);
				}
			}


			// Uncomment to verify the patched IL:

			// string[] indexedInstructions = new string[patchedInstructions.Count];
			// for (int i = 0; i < patchedInstructions.Count; i++)
			// {
			//     indexedInstructions[i] = $"[{i}] {patchedInstructions[i].ToString()}";
			// }
			// Mod.Print("Final Patched IL with Indices:\n", string.Join("\n", indexedInstructions));


			return patchedInstructions;
		}
	}
}
