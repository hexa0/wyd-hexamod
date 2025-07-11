using System.Collections.Generic;
using HarmonyLib;
using HexaMod.Settings;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;
namespace HexaMod.Patches.Feature
{
	internal class CameraController : MonoBehaviour
	{
		internal FirstPersonController firstPersonController;
		internal CharacterController characterController;

		internal CurveControlledBob headBob;
		internal LerpControlledBob jumpBob;

		internal float bobCycleX = 0f;
		internal float bobCycleY = 0f;

		internal float speed;
		internal Traverse<float> runStepLength;
		internal Traverse<float> bobBaseInterval;
		internal Traverse<bool> isWalking;

		internal Camera camera;

		public Dictionary<string, Vector3> cameraOffsets = new Dictionary<string, Vector3>();
		public Vector3 CameraOffset {
			get {
				Vector3 finalOffset = Vector3.zero;

				foreach (var offset in cameraOffsets)
				{
					finalOffset += offset.Value;
				}

				return finalOffset;
			}
		}
		public bool ViewBobbingAllowed = true;

		void Update()
		{
			if (HexaModPreferences.viewBobbing.Value && ViewBobbingAllowed)
			{
				if (characterController.velocity.magnitude > 0f && characterController.isGrounded)
				{
					float bobSpeed = characterController.velocity.magnitude + speed * (!isWalking.Value ? runStepLength.Value : 1f);
					float cycleTime = headBob.Bobcurve[headBob.Bobcurve.length - 1].time;

					bobCycleX += bobSpeed * Time.smoothDeltaTime / bobBaseInterval.Value;
					bobCycleY += bobSpeed * Time.smoothDeltaTime / bobBaseInterval.Value * headBob.VerticaltoHorizontalRatio;

					bobCycleX %= cycleTime;
					bobCycleY %= cycleTime;
				}

				float bobX = headBob.Bobcurve.Evaluate(bobCycleX) * headBob.HorizontalBobRange;
				float bobY = headBob.Bobcurve.Evaluate(bobCycleY) * headBob.VerticalBobRange;

				cameraOffsets["headBob"] = new Vector3(bobX, bobY, 0f);
				cameraOffsets["jumpBob"] = new Vector3(0f, -jumpBob.Offset(), 0f);
			}
			else
			{
				cameraOffsets["headBob"] = Vector3.zero;
				cameraOffsets["jumpBob"] = Vector3.zero;
			}

			camera.transform.localPosition = CameraOffset;
		}
	}

	[HarmonyPatch]
	internal static class CameraControllerPatch
	{
		[HarmonyPatch(typeof(FirstPersonController), "Start")]
		[HarmonyPostfix]
		static void Start(ref FirstPersonController __instance)
		{
			CameraController cameraController = __instance.gameObject.AddComponent<CameraController>();

			cameraController.firstPersonController = __instance;

			Traverse wydControllerFields = Traverse.Create(__instance);

			cameraController.jumpBob = wydControllerFields.Field<LerpControlledBob>("m_JumpBob").Value;
			cameraController.headBob = wydControllerFields.Field<CurveControlledBob>("m_HeadBob").Value;
			cameraController.camera = wydControllerFields.Field<Camera>("m_Camera").Value;
			cameraController.characterController = __instance.GetComponent<CharacterController>();

			Traverse headBobFields = Traverse.Create(cameraController.headBob);

			cameraController.runStepLength = wydControllerFields.Field<float>("m_RunstepLenghten");
			cameraController.isWalking = wydControllerFields.Field<bool>("m_IsWalking");
			cameraController.bobBaseInterval = headBobFields.Field<float>("m_BobBaseInterval");

			if (__instance.name.StartsWith("dad"))
			{
				cameraController.cameraOffsets.Add("dadOffset", new Vector3(
					cameraController.camera.transform.localPosition.x,
					cameraController.camera.transform.localPosition.y,
					cameraController.camera.transform.localPosition.x
					// ^ this code bug causes a behavior in the game that makes the camera offset itself forwards
					// it seems the code was unintentionally made to do that by setting a variable wrong but was never patched because it actually prevents you from seeing yourself
				));
			}

			cameraController.cameraOffsets.Add("headBob", Vector3.zero);
			cameraController.cameraOffsets.Add("jumpBob", Vector3.zero);
		}

		[HarmonyPatch(typeof(FirstPersonController), "UpdateCameraPosition")]
		[HarmonyPrefix]
		static bool UpdateCameraPosition(ref FirstPersonController __instance, float speed)
		{
			__instance.gameObject.GetComponent<CameraController>().speed = speed;

			return false;
		}
	}
}
