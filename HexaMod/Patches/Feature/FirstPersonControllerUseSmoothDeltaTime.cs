using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch]
	internal class FirstPersonControllerUseSmoothDeltaTime
	{
		static readonly MethodInfo originalMethod = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
		static readonly MethodInfo originalMethod2 = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
		static readonly MethodInfo replacementMethod = AccessTools.PropertyGetter(typeof(Time), nameof(Time.smoothDeltaTime));

		static IEnumerable<MethodBase> TargetMethods()
		{
			return typeof(FirstPersonController).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patchedInstructions = new List<CodeInstruction>(instructions);

			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.Calls(originalMethod) || instruction.Calls(originalMethod2))
				{
					instruction.operand = replacementMethod;
				}
			}

			return patchedInstructions;
		}
	}
}
