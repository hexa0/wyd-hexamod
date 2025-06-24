using HarmonyLib;
using HexaMapAssemblies;
using UnityEngine;

namespace HexaMod.Patches.Feature
{
	[HarmonyPatch(typeof(Door))]
	internal class ReturnDoorSound
	{
		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void AddRpcBehaviorPatch(ref Door __instance)
		{
			__instance.gameObject.AddComponent<DoorSoundRPC>();
		}

		[HarmonyPatch("Interact")]
		[HarmonyPrefix]
		static void DoorSound(ref Door __instance)
		{
			if (HexaPersistentLobby.instance.lobbySettings.doorSounds && ! __instance.locked)
			{
				GameObject sound = __instance.openSound;

				switch (__instance.tag)
				{
					case "Open":
						sound = __instance.openSound;
						break;
					case "Close":
						sound = __instance.closeSound;
						break;
				}

				if (sound != null)
				{
					__instance.SendMessage("MakeSound", __instance.tag);
				}
			}
		}
	}
}
