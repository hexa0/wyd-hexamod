using System;
using HarmonyLib;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(PhotonNetwork))]
	internal class RPCLog
	{
		[HarmonyPatch("RPC", new Type[] { typeof(PhotonView), typeof(string), typeof(PhotonTargets), typeof(bool), typeof(object[]) })]
		[HarmonyPrefix]
		static void RPCPhotonTargetsLogPatch(PhotonView view, string methodName, PhotonTargets target, bool encrypt, params object[] parameters)
		{
			// trace {Environment.StackTrace}
			Mod.PrintDebug($"RPC from \"{view.name}\" sent to PhotonTargets \"{target}\" invoking \"{methodName}\" with params: {parameters}");
		}

		[HarmonyPatch("RPC", new Type[] { typeof(PhotonView), typeof(string), typeof(PhotonPlayer), typeof(bool), typeof(object[]) })]
		[HarmonyPrefix]
		static void RPCPhotonPlayerLogPatch(PhotonView view, string methodName, PhotonPlayer targetPlayer, bool encrpyt, params object[] parameters)
		{
			Mod.PrintDebug($"RPC from \"{view.name}\" sent to PhotonPlayer \"{targetPlayer}\" invoking \"{methodName}\" with params: {parameters}");
		}
	}
}
