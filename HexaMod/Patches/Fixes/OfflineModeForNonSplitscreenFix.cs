using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HexaMod.UI;

namespace HexaMod.Patches.Fixes
{
	// the game incorrectly checks PhotonNetwork.offlineMode when switching between splitscreen and non-splitscreen logic, this patches all of those to use a correct check
	[HarmonyPatch]
	internal static class OfflineModeForNonSplitscreenFix
	{
		static readonly MethodInfo originalMethod = AccessTools.PropertyGetter(typeof(PhotonNetwork), nameof(PhotonNetwork.offlineMode));
		static readonly MethodInfo replacementMethod = AccessTools.Method(typeof(SplitscreenUtil), nameof(SplitscreenUtil.IsInSplitscreen));

		static IEnumerable<MethodBase> TargetMethods()
		{
			var targetTypes = new List<System.Type>
				{
					typeof(Door),
					typeof(Door1),
					typeof(Door2),
					typeof(Cabinet),
					typeof(Cabinet1),
					typeof(Cabinet2),
					typeof(Cabinet3),
					typeof(Cabinet4),
					typeof(Blaster),
					typeof(GameStateController),
					typeof(MenuOffHelper),
					typeof(OptionsController),
					typeof(RematchHelper),
					typeof(SplitOptionsController),
					typeof(Turrent),
					typeof(VoteCounter),
					typeof(DadItemTargeting),
					typeof(InGameMenuHelper),
					typeof(NetworkHelper),
				};

			return targetTypes.SelectMany(type =>
				type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
			).Cast<MethodBase>();
		}

		static IEnumerable<CodeInstruction> PatchOfflineModeGetterCalls(List<CodeInstruction> instructions)
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

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return PatchOfflineModeGetterCalls(new List<CodeInstruction>(instructions));
		}
	}
}
