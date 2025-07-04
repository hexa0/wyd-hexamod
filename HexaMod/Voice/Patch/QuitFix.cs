﻿using System.Diagnostics;
using HarmonyLib;
using UnityEngine;

namespace HexaMod.Voice
{
	[HarmonyPatch(typeof(Application))]
	internal class QuitFix
	{

		[HarmonyPatch("Quit")]
		[HarmonyPrefix]
		static bool Quit()
		{
			Mod.Print("Saving PlayerPrefs to disk.");
			PlayerPrefs.Save();
			Mod.Print("Quitting.");
			// the BepInEx console results in unity freezing upon shutdown, so we just kill the application forcefully instead.
			Process.GetCurrentProcess().Kill();

			return false;
		}
	}
}
