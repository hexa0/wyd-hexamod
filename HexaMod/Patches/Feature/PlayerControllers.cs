using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal static class PlayerControllers
	{
		public static Transform parent;
		// TODO: make a static list of different transforms and have this parent just be the main transform so that custom logic that needs to reparent players to another object is supported

		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static void Start(ref FirstPersonController __instance)
		{
			if (__instance.transform.parent == null)
			{
				__instance.transform.SetParent(parent, true);
			}
			else
			{
				// warn that this player isn't in the root of the hierarchy with the full path
				Mod.Warn("huh ", __instance.transform.parent.name, " ", __instance.transform.name);
			}
		}

		public static FirstPersonController[] GetPlayers()
		{
			int children = parent.childCount;

			FirstPersonController[] players = new FirstPersonController[children];

			for (int i = 0; i < children; i++)
			{
				players[i] = parent.GetChild(i).GetComponent<FirstPersonController>();
			}

			return players;
		}

		public static FirstPersonController LocalPlayer => HexaGlobal.networkManager.playerObj?.GetComponent<FirstPersonController>();
		public static FirstPersonController HostPlayer => GetPlayers()[0];


		public static Transform[] GetPlayerTransforms()
		{
			int children = parent.childCount;

			Transform[] playerTransforms = new Transform[children];

			for (int i = 0; i < children; i++)
			{
				playerTransforms[i] = parent.GetChild(i);
			}

			return playerTransforms;
		}

		public static FirstPersonController GetPlayer(string name)
		{
			return parent.Find(name)?.GetComponent<FirstPersonController>();
		}
	}
}
