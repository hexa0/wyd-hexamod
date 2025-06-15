using System.Collections.Generic;
using HarmonyLib;

namespace HexaMod.Patches.Debugging
{
	public static class ILDebugger
	{
		static string GetInstructionString(int i, CodeInstruction instruction)
		{
			return $"[{i}] {instruction}";
		}

		static string[] originalInstructionStrings = new string[0];

		public static void SetOriginalInstructionStrings(List<CodeInstruction> original)
		{
			originalInstructionStrings = new string[original.Count];

			for (int i = 0; i < original.Count; i++)
			{
				originalInstructionStrings[i] = GetInstructionString(i, original[i]);
			}
		}

		public static void LogILDiff(List<CodeInstruction> patched)
		{
			string patchedInstructions = string.Empty;

			for (int i = 0; i < patched.Count; i++)
			{
				string instruction = GetInstructionString(i, patched[i]);
				if (originalInstructionStrings[i] != instruction)
				{
					patchedInstructions += instruction;

					if (i < patched.Count - 1)
					{
						patchedInstructions += "\n";
					}
				}
			}

			Mod.Print($"ILDebugger:\nOriginal IL:\n{string.Join("\n", originalInstructionStrings)}\nPatched IL diff:\n{patchedInstructions}");

			originalInstructionStrings = new string[0];
		}
	}
}
