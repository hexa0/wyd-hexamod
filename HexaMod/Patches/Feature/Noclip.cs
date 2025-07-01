using HarmonyLib;
using HexaMod.Patches.Fixes;
using HexaMod.Scripts;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class Noclip
	{
		public static class NoclipGlobal
		{
			public static bool isNoclipping = false;
		}

		[HarmonyPatch("FixedUpdate")]
		[HarmonyPrefix]
		static bool FixedUpdate(ref FirstPersonController __instance)
		{
			if (SmoothCharacterControllerPatch.SmoothCharacterControllerPatchGlobal.isRunningFromPatch)
			{
				if (HexaPersistentLobby.instance.lobbySettings.cheats)
				{
					if (Input.GetKeyDown("v"))
					{
						NoclipGlobal.isNoclipping = !NoclipGlobal.isNoclipping;

						__instance.gameObject.GetComponentInChildren<CharacterController>().enabled = !NoclipGlobal.isNoclipping;
						GameObject.Find("ActionText").SendMessage("ActionDone", "Noclip " + (NoclipGlobal.isNoclipping ? "Enabled" : "Disabled"));
					}
				}
				else
				{
					NoclipGlobal.isNoclipping = false;
				}
			}
			if (SmoothCharacterControllerPatch.SmoothCharacterControllerPatchGlobal.isRunningFromPatch && NoclipGlobal.isNoclipping)
			{
				Vector3 moveVector = new Vector3(
					__instance.xAxis,
					0f +
					(Input.GetKey("e") ? 1f : 0f) +
					(Input.GetKey("q") ? -1f : 0f),
					__instance.yAxis
				);
				__instance.transform.Translate(moveVector * (Time.deltaTime * (__instance.runButton ? 20f : 10f)), __instance.myCam.transform);
				return false;
			}

			return true;
		}
	}
}
