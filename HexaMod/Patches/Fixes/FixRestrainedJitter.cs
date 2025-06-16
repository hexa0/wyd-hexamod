using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class FixRestrainedJitter
	{
		[HarmonyPatch("FixedUpdate")]
		[HarmonyPostfix]
		static void FixedUpdate(ref FirstPersonController __instance)
		{
			if (__instance.restrained || __instance.restrainedHeld)
			{
				__instance.transform.localPosition = Vector3.zero;
				__instance.transform.localRotation = Quaternion.Euler(0f, 90f, 180f);

				CharacterController controller = __instance.GetComponent<CharacterController>();

				Traverse characterFields = Traverse.Create(__instance);
				var m_MoveDir = characterFields.Field<Vector3>("m_MoveDir");

				m_MoveDir.Value = Vector3.zero;

				controller.center = controller.center;
				controller.Move(Vector3.zero);
			}
		}
	}
}
