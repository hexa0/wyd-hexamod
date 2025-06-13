using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	public static class PhotonMultipleMethodsSilencer
	{
		static readonly string internalClassName = "NetworkingPeer";
		static readonly string targetMethodName = "ExecuteRpc";
		static readonly object opperandToFind = ". Should be just one?";

		public static MethodBase TargetMethod()
		{

			Type internalType = AccessTools.TypeByName(internalClassName);

			if (internalType == null)
			{
				Mod.Error($"Error: Internal class '{internalClassName}' not found.");
				return null;
			}

			return AccessTools.Method(internalType, targetMethodName);
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patchedInstructions = new List<CodeInstruction>(instructions);

			int baseAddress = patchedInstructions.FindIndex(instruction => instruction.operand == opperandToFind) + 3;

			patchedInstructions[baseAddress] = new CodeInstruction(OpCodes.Pop);

			// Uncomment to verify the patched IL:

			//string[] indexedInstructions = new string[patchedInstructions.Count];
			//for (int i = 0; i < patchedInstructions.Count; i++)
			//{
			//	indexedInstructions[i] = $"[{i}] {patchedInstructions[i].ToString()}";
			//}
			//Mod.Print("Final Patched IL with Indices:\n", string.Join("\n", indexedInstructions));


			return patchedInstructions;
		}
	}
}
