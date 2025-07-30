using System;
using HarmonyLib;
using HexaMod.API.Util.Patching;

namespace HexaMod.Patches.Debugging
{
	[OptionalPatch("logPhotonRPCEvents", "Log PhotonNetwork RPC Events", "Debugging", false)]
	[HarmonyPatch(typeof(PhotonNetwork))]
	internal class RPCLog
	{
		[HarmonyPatch("RPC", new Type[] { typeof(PhotonView), typeof(string), typeof(PhotonTargets), typeof(bool), typeof(object[]) })]
		[HarmonyPrefix]
		static void RPCPhotonTargetsLogPatch(PhotonView view, string methodName, PhotonTargets target, params object[] parameters)
		{
			// trace {Environment.StackTrace}
			Mod.Print($"RPC from \"{view.name}\" sent to PhotonTargets \"{target}\" invoking \"{methodName}\" with params: {parameters}");
		}

		[HarmonyPatch("RPC", new Type[] { typeof(PhotonView), typeof(string), typeof(PhotonPlayer), typeof(bool), typeof(object[]) })]
		[HarmonyPrefix]
		static void RPCPhotonPlayerLogPatch(PhotonView view, string methodName, PhotonPlayer targetPlayer, params object[] parameters)
		{
			Mod.Print($"RPC from \"{view.name}\" sent to PhotonPlayer \"{targetPlayer}\" invoking \"{methodName}\" with params: {parameters}");
		}
	}
}
