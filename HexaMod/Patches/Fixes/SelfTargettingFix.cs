using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HexaMod.API.UI;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	internal class SelfTargettingFix
	{
		internal static bool RaycastProxy(bool isDad, Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			GameObject player;

			if (SplitscreenUtil.IsInSplitscreen())
			{
				if (HexaGlobal.networkManager.player2Input.myPlayer.name.ToLower().StartsWith("dad") == isDad)
				{
					player = HexaGlobal.networkManager.player2Input.myPlayer.gameObject;
				}
				else
				{
					player = HexaGlobal.networkManager.player1Input.myPlayer.gameObject;
				}
			}
			else
			{
				player = HexaGlobal.networkManager.playerObj;
			}

			int oldLayer = player.layer;
			player.layer = 2;
			bool output = Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask);
			player.layer = oldLayer;

			return output;
		}

		internal static bool RaycastProxyDad(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			return RaycastProxy(true, origin, direction, out hitInfo, maxDistance, layerMask);
		}

		internal static bool RaycastProxyBaby(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask)
		{
			return RaycastProxy(false, origin, direction, out hitInfo, maxDistance, layerMask);
		}
	}

	[HarmonyPatch(typeof(DadItemTargeting), "Update")]
	internal class DadSelfTargettingFix
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patchedInstructions = new List<CodeInstruction>(instructions);

			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode.Name == "call" && (instruction.operand as MethodInfo).Name == "Raycast")
				{
					instruction.operand = AccessTools.Method(typeof(SelfTargettingFix), nameof(SelfTargettingFix.RaycastProxyDad));
				}
			}

			return patchedInstructions;
		}
	}

	[HarmonyPatch(typeof(ItemTargeting), "Update")]
	internal class BabySelfTargettingFix
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var patchedInstructions = new List<CodeInstruction>(instructions);

			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode.Name == "call" && (instruction.operand as MethodInfo).Name == "Raycast")
				{
					instruction.operand = AccessTools.Method(typeof(SelfTargettingFix), nameof(SelfTargettingFix.RaycastProxyBaby));
				}
			}

			return patchedInstructions;
		}
	}
}
