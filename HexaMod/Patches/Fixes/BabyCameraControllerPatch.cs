using HarmonyLib;
using HexaMod.Settings;
using MagicalFX;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

namespace HexaMod.Patches.Fixes
{
	internal class BabyCameraController : MonoBehaviour
	{
		internal FirstPersonController firstPersonController;
		internal CharacterController characterController;

		internal LerpControlledBob jumpBob;
		internal CurveControlledBob headBob;

		internal Traverse<float> headBobCycleX;
		internal Traverse<float> headBobCycleY;
		internal float speed;
		internal Traverse<float> runStepLength;
		internal Traverse<bool> isWalking;

		internal Camera camera;

		private Vector3 currentHeadBob = Vector3.zero;

		void Update()
		{
			if (HexaModPreferences.viewBobbing.Value)
			{
				if (characterController.velocity.magnitude > 0f && characterController.isGrounded)
				{
					currentHeadBob = headBob.DoHeadBob(characterController.velocity.magnitude + speed * ((!isWalking.Value) ? runStepLength.Value : 1f));
				}

				camera.transform.localPosition = currentHeadBob + new Vector3(0f, -jumpBob.Offset(), 0f);
			}
			else
			{
				camera.transform.localPosition = Vector3.zero;
			}
		}
	}

	[HarmonyPatch]
	internal static class BabyCameraControllerPatch
	{
		[HarmonyPatch(typeof(BabyStats), "Start")]
		[HarmonyPostfix]
		static void Start(ref BabyStats __instance)
		{
			BabyCameraController babyCameraController = __instance.gameObject.AddComponent<BabyCameraController>();

			FirstPersonController firstPersonController = __instance.gameObject.GetComponent<FirstPersonController>();
			babyCameraController.firstPersonController = firstPersonController;

			Traverse wydControllerFields = Traverse.Create(firstPersonController);

			babyCameraController.jumpBob = wydControllerFields.Field<LerpControlledBob>("m_JumpBob").Value;
			babyCameraController.headBob = wydControllerFields.Field<CurveControlledBob>("m_HeadBob").Value;
			babyCameraController.camera = wydControllerFields.Field<Camera>("m_Camera").Value;
			babyCameraController.characterController = firstPersonController.GetComponent<CharacterController>();

			Traverse headBobFields = Traverse.Create(babyCameraController.headBob);

			babyCameraController.headBobCycleX = headBobFields.Field<float>("m_CyclePositionX");
			babyCameraController.headBobCycleY = headBobFields.Field<float>("m_CyclePositionY");
			babyCameraController.runStepLength = wydControllerFields.Field<float>("m_RunstepLenghten");
			babyCameraController.isWalking = wydControllerFields.Field<bool>("m_IsWalking");
		}

		[HarmonyPatch(typeof(FirstPersonController), "UpdateCameraPosition")]
		[HarmonyPrefix]
		static bool UpdateCameraPosition(ref BabyStats __instance, float speed)
		{
			__instance.gameObject.GetComponent<BabyCameraController>().speed = speed;
			return false;
		}
	}
}
