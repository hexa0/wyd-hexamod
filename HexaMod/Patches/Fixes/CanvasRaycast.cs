﻿using HarmonyLib;
using UnityEngine;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(anvasHelper))]
	internal class CanvasRaycast
	{
		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool CanvasRaycastPatch(ref anvasHelper __instance)
		{
			if (!HexaGlobal.networkManager.gameStarted)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
				__instance.canvGroup.blocksRaycasts = true;
			}
			else
			{
				if (!Cursor.visible)
				{
					__instance.canvGroup.blocksRaycasts = false;
				}
				else
				{
					__instance.canvGroup.blocksRaycasts = true;
				}
			}

			return false;
		}
	}
}
