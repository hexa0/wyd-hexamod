using System;
using HarmonyLib;
using HexaMod.API.Util.Unity;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(Resources))]
	internal class CustomPrefabs
	{
		[HarmonyPatch("Load", new Type[] { typeof(string), typeof(Type) })]
		[HarmonyPrefix]
		static bool Load(string path, ref UnityEngine.Object __result)
		{
			if (PrefabExtensionUtils.customPrefabs.ContainsKey(path))
			{
				__result = PrefabExtensionUtils.customPrefabs[path];
				return false;
			}
			else
			{
				return true;
			}
		}
	}
}
