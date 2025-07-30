using System;
using HarmonyLib;
using HexaMod.API.Util.Patching;
using UnityEngine;

namespace HexaMod.Patches.Debugging
{
	[OptionalPatch("logPhotonInstantiateCalls", "Log PhotonNetwork.Instantiate Calls", "Debugging", false)]
	[HarmonyPatch(typeof(PhotonNetwork))]
	internal class LogInstantiations
	{
		[HarmonyPatch("Instantiate", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion), typeof(byte), typeof(object[]) })]
		[HarmonyPrefix]
		static void Instantiate(string prefabName)
		{
			//Mod.Debug($"Photon Instantation of \"{prefabName}\", trace: {Environment.StackTrace}");
			Mod.Print($"Photon Instantation of \"{prefabName}\"");
		}
	}
}
