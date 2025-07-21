using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class SmoothCharacterControllerPatch
	{
		public static class SmoothCharacterControllerPatchGlobal
		{
			public static bool isRunningFromPatch = false;
		}

		[HarmonyPatch("FixedUpdate")]
		[HarmonyPrefix]
		static bool CancelFixedUpdate()
		{
			if (!SmoothCharacterControllerPatchGlobal.isRunningFromPatch)
			{
				return false;
			}

			return true;
		}

		[HarmonyPatch]
		internal class UpdateFixedDeltaTimeTranspiler
		{
			static readonly MethodInfo originalMethod = AccessTools.PropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime));
			static readonly MethodInfo replacementMethod = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));

			static IEnumerable<MethodBase> TargetMethods()
			{
				return typeof(FirstPersonController).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var patchedInstructions = new List<CodeInstruction>(instructions);

				foreach (CodeInstruction instruction in instructions)
				{
					if (instruction.Calls(originalMethod))
					{
						instruction.operand = replacementMethod;
					}
				}

				return patchedInstructions;
			}
		}

		static readonly MethodInfo fixedUpdateMethod = AccessTools.Method(typeof(FirstPersonController), "FixedUpdate");

		[HarmonyPatch("Update")]
		[HarmonyPostfix]
		static void RunFixedUpdateOnUpdate(ref FirstPersonController __instance)
		{
			PhotonView netView = __instance.GetComponent<PhotonView>();

			if (netView & netView.isMine)
			{
				SmoothCharacterControllerPatchGlobal.isRunningFromPatch = true;
				fixedUpdateMethod.Invoke(__instance, null);
				SmoothCharacterControllerPatchGlobal.isRunningFromPatch = false;
			}
		}
	}
}
