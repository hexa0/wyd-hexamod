using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace HexaMod.API.Util.Patching
{
	public class ClassReferenceUpdater
	{
		public static IEnumerable<CodeInstruction> PatchClassReferences(List<CodeInstruction> instructions, Type originalClass, Type replacementClass)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.operand is MethodInfo method && method.DeclaringType == originalClass)
				{
					instruction.operand = replacementClass;
				}
			}

			return instructions;
		}
	}
}
