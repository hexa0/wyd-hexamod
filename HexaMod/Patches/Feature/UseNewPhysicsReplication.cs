using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HexaMod.API.Util.Patching;
using HexaMod.Patches.Fixes;

namespace HexaMod.Patches.Feature
{
	[ModdedPatch]
	[HarmonyPatch]
	internal static class UseNewPhysicsReplication
	{
		static readonly Type originalClass = typeof(NetworkMovementRB);
		static readonly Type replacementClass = typeof(RigidBodyReplication);

		static IEnumerable<MethodBase> TargetMethods()
		{
			var targetTypes = new List<Type>
				{
					typeof(Fork),
					typeof(GivePills),
					typeof(LeftHand)
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
