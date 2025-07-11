using System;
using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Debugging
{
	[HarmonyPatch(typeof(PhotonNetwork))]
	internal class LogInstantiations
	{
		[HarmonyPatch("Instantiate", new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion), typeof(byte), typeof(object[]) })]
		[HarmonyPrefix]
		static void Instantiate(string prefabName)
		{
			//Mod.Debug($"Photon Instantation of \"{prefabName}\", trace: {Environment.StackTrace}");
			Mod.Debug($"Photon Instantation of \"{prefabName}\"");
		}
	}
}
