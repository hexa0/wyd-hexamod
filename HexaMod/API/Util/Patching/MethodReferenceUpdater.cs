using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace HexaMod.API.Util.Patching
{
	public class MethodReferenceUpdater
	{
		public static IEnumerable<CodeInstruction> PatchMethodReferences(List<CodeInstruction> instructions, MethodInfo originalMethod, MethodInfo replacementMethod)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.Calls(originalMethod))
				{
					instruction.operand = replacementMethod;
				}
			}

			return instructions;
		}
	}
}
