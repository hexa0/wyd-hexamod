using System.Collections.Generic;
using HarmonyLib;
using HexaMod.API.Util.Unity.Settings;
using UnityEngine;

namespace HexaMod.Scripts.Character
{
	public class CameraController : MonoBehaviour
	{
		internal HexaPlayerController PlayerController => GetComponent<HexaPlayerController>();
		internal CharacterController CharacterController => PlayerController.characterController;

		internal float bobCycleX = 0f;
		internal float bobCycleY = 0f;

		internal Traverse<float> bobBaseInterval;

		public Dictionary<string, Vector3> cameraOffsets = new Dictionary<string, Vector3>();
		public Vector3 CameraOffset
		{
			get
			{
				Vector3 finalOffset = Vector3.zero;

				foreach (var offset in cameraOffsets)
				{
					finalOffset += offset.Value;
				}

				return finalOffset;
			}
		}
		public bool ViewBobbingAllowed = true;

		void Start()
		{
			Traverse headBobFields = Traverse.Create(PlayerController.HeadBob);
			bobBaseInterval = headBobFields.Field<float>("m_BobBaseInterval");
		}

		public void UpdateCamera()
		{
			if (HexaModPreferences.viewBobbing.Value && ViewBobbingAllowed)
			{
				if (CharacterController.velocity.magnitude > 0f && CharacterController.isGrounded)
				{
					float bobSpeed = CharacterController.velocity.magnitude + PlayerController.currentSpeed * (!PlayerController.IsWalking ? PlayerController.RunstepLenghten : 1f);
					float cycleTime = PlayerController.HeadBob.Bobcurve[PlayerController.HeadBob.Bobcurve.length - 1].time;

					bobCycleX += bobSpeed * Time.smoothDeltaTime / bobBaseInterval.Value;
					bobCycleY += bobSpeed * Time.smoothDeltaTime / bobBaseInterval.Value * PlayerController.HeadBob.VerticaltoHorizontalRatio;

					bobCycleX %= cycleTime;
					bobCycleY %= cycleTime;
				}

				float bobX = PlayerController.HeadBob.Bobcurve.Evaluate(bobCycleX) * PlayerController.HeadBob.HorizontalBobRange;
				float bobY = PlayerController.HeadBob.Bobcurve.Evaluate(bobCycleY) * PlayerController.HeadBob.VerticalBobRange;

				cameraOffsets["headBob"] = new Vector3(bobX, bobY, 0f);
				cameraOffsets["jumpBob"] = new Vector3(0f, -PlayerController.JumpBob.Offset(), 0f);
			}
			else
			{
				cameraOffsets["headBob"] = Vector3.zero;
				cameraOffsets["jumpBob"] = Vector3.zero;
			}

			Transform camera = PlayerController.myCam.transform;
			camera.localPosition = CameraOffset;

			switch (PlayerController.CameraPerspectiveEnum)
			{
				case HexaPlayerController.CameraPerspective.Behind:
					camera.position += camera.forward * -2f;
					break;
				case HexaPlayerController.CameraPerspective.InFront:
					camera.rotation *= Quaternion.Euler(0f, -180f, 0f);
					camera.position += camera.forward * -2f;
					break;
			}
		}
	}
}
