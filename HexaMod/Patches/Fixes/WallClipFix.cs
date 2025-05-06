using System;
using System.Windows.Forms;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

namespace HexaMod.Patches.Fixes
{
	[HarmonyPatch]
	internal class WallClipFix
	{
		private static Quaternion ClampRotationAroundXAxis(Quaternion q, float MinimumX, float MaximumX)
		{
			q.x /= q.w;
			q.y /= q.w;
			q.z /= q.w;
			q.w = 1f;
			float value = 114.59156f * Mathf.Atan(q.x);
			value = Mathf.Clamp(value, MinimumX, MaximumX);
			q.x = Mathf.Tan((float)Math.PI / 360f * value);
			return q;
		}

		// fix offset the issues due to the collider offset

		[HarmonyPatch(typeof(MouseLook), "LookRotation")]
		[HarmonyPrefix]
		static bool LookRotation(ref MouseLook __instance, ref Transform character, ref Transform camera)
		{
			Traverse fields = Traverse.Create(__instance);

			Traverse<Quaternion> m_CharacterTargetRot = fields.Field<Quaternion>("m_CharacterTargetRot");
			Traverse<Quaternion> m_CameraTargetRot = fields.Field<Quaternion>("m_CameraTargetRot");

			// the baby camera is configured wrong and has either the Y or Z value ever so slightly off from 0f which causes it to worsen over time
			// this fixes that
			m_CameraTargetRot.Value = new Quaternion(m_CameraTargetRot.Value.x, 0f, 0f, m_CameraTargetRot.Value.w);

			float y = __instance.xAxis * __instance.XSensitivity * __instance.sensitivity;
			float num = __instance.yAxis * __instance.YSensitivity * __instance.invert * __instance.sensitivity;
			m_CharacterTargetRot.Value *= Quaternion.Euler(0f, y, 0f);
			m_CameraTargetRot.Value *= Quaternion.Euler(0f - num, 0f, 0f);
			if (__instance.clampVerticalRotation)
			{
				m_CameraTargetRot.Value = ClampRotationAroundXAxis(m_CameraTargetRot.Value, __instance.MinimumX, __instance.MaximumX);
			}

			if (__instance.smooth) // this is likely only used while spectating so we leave it unmodified
			{
				character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot.Value, __instance.smoothTime * Time.deltaTime);
				camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot.Value, __instance.smoothTime * Time.deltaTime);
			}
			else
			{
				CharacterController controller = character.GetComponent<CharacterController>();
				if (controller != null)
				{
					character.position += character.rotation * controller.center;
				}

				character.localRotation = m_CharacterTargetRot.Value;
				camera.localRotation = m_CameraTargetRot.Value;

				if (controller != null)
				{
					character.position += character.rotation * -controller.center;
				}
			}

			return false;
		}

		// fix camera issues with proning/crouching

		static bool crouchingTarget = false;
		static bool proningTarget = false;

		static private float camStartX = 0f;
		static private float camStartY = 0f;
		static private float camStartZ = 0f;


		[HarmonyPatch(typeof(ActionInput), "Start")]
		[HarmonyPostfix]
		static void ActionInputStart(ref ActionInput __instance)
		{
			crouchingTarget = false;
			proningTarget = false;
		}

		[HarmonyPatch(typeof(ActionInput), "Update")]
		[HarmonyPostfix]
		static void ActionInputUpdate(ref ActionInput __instance)
		{
			if (__instance.dadCrouch != null)
			{
				if (__instance.btn[6].isDown)
				{
					crouchingTarget = !crouchingTarget;
				}

				if (__instance.btn[16].isDown)
				{
					proningTarget = !proningTarget;
				}

				__instance.dadCrouch.crouchButton = crouchingTarget;
				__instance.dadCrouch.proneButton = proningTarget;
			}
		}

		static float characterHeight = 2.8f;
		static int ceilingRaycastMask = ~671088640;

		[HarmonyPatch(typeof(Crouch), "Start")]
		[HarmonyPrefix]
		static void CrouchStart(ref Crouch __instance)
		{
			camStartX = __instance.cam.localPosition.x;
			camStartY = __instance.cam.localPosition.y;
			camStartZ = __instance.cam.localPosition.x; // this bug replicates a behavior in the game that makes the camera offset itself forwards
			// it seems the code was unintentionally made to do that by setting a variable wrong but was never patched because it prevents you from seeing yourself
			__instance.gameObject.AddComponent<WallClipFixBehavior>();
		}

		[HarmonyPatch(typeof(DadAnimator), "GetControls")]
		[HarmonyPrefix]
		static void DadAnimatorGetControls(ref DadAnimator __instance)
		{
			PhotonView netView = __instance.GetComponent<PhotonView>();
			if (netView.isMine)
			{
				WallClipFixBehavior self = __instance.transform.parent.GetComponent<WallClipFixBehavior>();
				__instance.proneButton = self.proning;
			}
		}


		[HarmonyPatch(typeof(Crouch), "Update")]
		[HarmonyPrefix]
		static bool CrouchUpdate(ref Crouch __instance)
		{
			//LineRenderer line = __instance.gameObject.GetComponent<LineRenderer>();
			//if (line == null)
			//{
			//	line = __instance.gameObject.AddComponent<LineRenderer>();
			//	line.widthMultiplier = 0.1f;
			//}

			WallClipFixBehavior self = __instance.GetComponent<WallClipFixBehavior>();

			CharacterController controller = __instance.charCont;
			FirstPersonController wydController = __instance.charContScript;

			Traverse wydControllerFields = Traverse.Create(wydController);

			Traverse<LerpControlledBob> m_JumpBob = wydControllerFields.Field<LerpControlledBob>("m_JumpBob");
			Traverse<CurveControlledBob> m_HeadBob = wydControllerFields.Field<CurveControlledBob>("m_HeadBob");

			Traverse headBobFields = Traverse.Create(m_HeadBob.Value);

			Traverse<float> m_CyclePositionX = headBobFields.Field<float>("m_CyclePositionX");
			Traverse<float> m_CyclePositionY = headBobFields.Field<float>("m_CyclePositionY");

			float bobX = m_HeadBob.Value.Bobcurve.Evaluate(m_CyclePositionX.Value) * m_HeadBob.Value.HorizontalBobRange;
			float bobY = m_HeadBob.Value.Bobcurve.Evaluate(m_CyclePositionY.Value) * m_HeadBob.Value.VerticalBobRange;

			__instance.GetControls();

			RaycastHit ceilingRaycast;
			Vector3 floorPosition = __instance.transform.position + __instance.transform.rotation * new Vector3(controller.center.x, controller.center.y - (controller.height / 2f), controller.center.z);
			bool ceilingDetected = Physics.SphereCast(floorPosition, controller.radius, Vector3.up, out ceilingRaycast, characterHeight, ceilingRaycastMask);

			bool blocked = ceilingDetected ? ceilingRaycast.distance + controller.radius <= characterHeight : false;

			self.crouching = __instance.btnDown ? true : (blocked ? self.crouching : false);
			self.proning = __instance.btn2Down ? true : (blocked ? self.proning : false);

			float targetHeight = self.proning ? characterHeight * self.proneMult : (self.crouching ? characterHeight * self.crouchMult : characterHeight);
			Vector3 targetCenter = self.proning ? new Vector3(0f, -0.8f, 0.9f) : (self.crouching ? new Vector3(0f, -0.6f, -0.28f) : new Vector3(0f, 0.1f, -0.28f));
			Vector3 targetCameraOffset = self.proning ? new Vector3(0f, __instance.proneHeight, __instance.proneDis) : (self.crouching ? new Vector3(0f, __instance.crouchHeight, 0f) : Vector3.zero);

			controller.center = Vector3.Lerp(controller.center, targetCenter, Mathf.Min(Time.deltaTime * 15f, 1f));
			controller.height = Mathf.Lerp(controller.height, targetHeight, Mathf.Min(Time.deltaTime * 15f, 1f));
			self.crouchHeight = Mathf.Lerp(self.crouchHeight, self.crouching ? __instance.crouchHeight : 0f, Mathf.Min(Time.deltaTime * 15f, 1f));
			self.cameraOffset = Vector3.Lerp(self.cameraOffset, targetCameraOffset, Mathf.Min(Time.deltaTime * 15f, 1f));

			Vector3 localCameraPosition = __instance.cam.localPosition;
			localCameraPosition = new Vector3(
				camStartX + self.cameraOffset.x,
				camStartY + self.cameraOffset.y,
				camStartZ + self.cameraOffset.z
			) + new Vector3(0f, -m_JumpBob.Value.Offset(), 0f) + new Vector3(bobX, bobY, 0f);
			__instance.cam.localPosition = localCameraPosition;

			return false;
		}

		[HarmonyPatch(typeof(Crouch), "LateUpdate")]
		[HarmonyPrefix]
		static bool LateUpdate(ref Crouch __instance)
		{
			WallClipFixBehavior extendedValues = __instance.GetComponent<WallClipFixBehavior>();

			if (Mathf.Abs(extendedValues.crouchHeight) > 0.01f)
			{
				float z = extendedValues.crouchHeight;
				Vector3 localPosition = __instance.crouchBone.localPosition;
				localPosition.z = z;
				__instance.crouchBone.localPosition = localPosition;
			}

			return false;
		}

			//[HarmonyPatch(typeof(Crouch), "Update")]
			//[HarmonyPrefix]
			//static void CrouchUpdatePre(ref Crouch __instance)
			//{
			//	if (!proning && __instance.btn2Down && __instance.charCont.height == 1f) // exiting a prone while not crouching
			//	{
			//		Mod.Print("exit prone");
			//	}

			//	if (!crouching && __instance.btnDown && __instance.charCont.height == 1.4f) // exiting a crouch while not proning
			//	{
			//		Mod.Print("exit crouch");
			//	}

			//	lastHeight = __instance.charCont.height;

			//	__instance.goBack = true;
			//}

			//[HarmonyPatch(typeof(Crouch), "Update")]
			//[HarmonyPostfix]
			//static void CrouchUpdatePost(ref Crouch __instance)
			//{
			//	if (!crouching && !proning)
			//	{
			//		if (__instance.charCont.height != 2.8f)
			//		{
			//			RaycastHit hitInfo;
			//			Physics.SphereCast(__instance.transform.position, 0.3f, Vector3.up, out hitInfo, 1000f, ~671088640);
			//			if (!(hitInfo.distance <= 1.4f))
			//			{
			//				Mod.Print("Fix");
			//				__instance.charCont.height = 2.8f;
			//			}
			//		}
			//	}

			//	if ((__instance.charCont.height - lastHeight) > 0f)
			//	{
			//		// __instance.transform.position += new Vector3(0f, (__instance.charCont.height - lastHeight) / -2f, 0f);
			//	}
			//}
		}
}
