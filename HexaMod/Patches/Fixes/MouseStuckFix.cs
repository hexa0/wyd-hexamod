using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using HexaMod.UI.Util;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class MouseStuckFix
	{
		[HarmonyPatch(typeof(InGameMenuHelper), "Update")]
		public static class RemoveOriginalCursorLockLogic
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var patchedInstructions = new List<CodeInstruction>(instructions);

				CodeInstruction pop = new CodeInstruction(OpCodes.Pop);

				patchedInstructions[62] = pop;
				patchedInstructions[64] = pop;
				patchedInstructions[70] = pop;
				patchedInstructions[72] = pop; 

				return patchedInstructions;
			}
		}

		[HarmonyPatch(typeof(InGameMenuHelper), "Update")]
		[HarmonyPostfix]
		static void Update(ref InGameMenuHelper __instance)
		{
			bool mouseLocked = !Menu.WYDMenus.AnyMenuOpen();

			Cursor.visible = !mouseLocked;
			Cursor.lockState = mouseLocked ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
