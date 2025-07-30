using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HexaMod.API.Util.Patching;
using HexaMod.Scripts.Character;

namespace HexaMod.Patches.Feature
{
	[ModdedPatch]
	[HarmonyPatch]
	internal static class UseNewCharacterReplication
	{
		static readonly Type originalClass = typeof(NetworkMovement);
		static readonly Type replacementClass = typeof(CharacterReplication);

		static IEnumerable<MethodBase> TargetMethods()
		{
			var targetTypes = new List<Type>
				{
					typeof(NetworkManager),
					typeof(PhotonNetworkManager)
				};

			return targetTypes.SelectMany(type =>
				type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
			).Cast<MethodBase>();
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return ClassReferenceUpdater.PatchClassReferences(
				instructions.ToList(),
				originalClass,
				replacementClass
			);
		}
	}
}
