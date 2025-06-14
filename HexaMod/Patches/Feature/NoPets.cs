﻿using HarmonyLib;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(PetSpawner))]
	internal class NoPets
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool NoPetsPatch()
		{
			return !HexaMod.persistentLobby.lobbySettings.disablePets;
		}
	}
}
