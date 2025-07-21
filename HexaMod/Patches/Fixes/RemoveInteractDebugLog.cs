using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal static class RemoveInteractDebugLog
	{
		[HarmonyPatch(typeof(GivePills), "RPCUseInteract")]
		static class RemoveInteractDebugLogGivePillsPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var patchedInstructions = new List<CodeInstruction>(instructions);

				patchedInstructions.RemoveRange(0, 4);

				return patchedInstructions.ToArray();
			}
		}

		[HarmonyPatch]
		static class RemoveInteractDebugLogForkPatch
		{
			public static MethodBase TargetMethod()
			{
				Type declaringType = typeof(Fork);
				Type innerEnumerator = AccessTools.FirstInner(declaringType, _ => true);
				Type deeperInnerEnumerator = AccessTools.FirstInner(innerEnumerator, _ => true); // why tf is it doubled???
				MethodInfo moveNextMethod = AccessTools.Method(deeperInnerEnumerator, "MoveNext");

				return moveNextMethod;
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var patchedInstructions = new List<CodeInstruction>(instructions);
				// ILDebugger.SetOriginalInstructionStrings(patchedInstructions);

				patchedInstructions[5] = new CodeInstruction(OpCodes.Pop);

				// ILDebugger.LogILDiff(patchedInstructions);
				return patchedInstructions.ToArray();
			}
		}
	}
}
