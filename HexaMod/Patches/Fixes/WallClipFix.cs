using System;
using HarmonyLib;
using HexaMod.Patches.Feature;
using HexaMod.Settings;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	// this patch basically completely rewrites the crouching system to fix wall clipping issues
	// this also adds options for smoother crouching
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
				character.localRotation = Quaternion.Slerp(character.localRotation, m_CharacterTargetRot.Value, __instance.smoothTime * Time.smoothDeltaTime);
				camera.localRotation = Quaternion.Slerp(camera.localRotation, m_CameraTargetRot.Value, __instance.smoothTime * Time.smoothDeltaTime);
			}
			else
			{
				// fix offset the issues due to the collider offset

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

		[HarmonyPatch(typeof(ActionInput), "Start")]
		[HarmonyPostfix]
		static void ActionInputStart()
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

		readonly static float characterHeight = 2.8f;
		readonly static int ceilingRaycastMask = ~671088640;

		[HarmonyPatch(typeof(Crouch), "Start")]
		[HarmonyPrefix]
		static void CrouchStart(ref Crouch __instance)
		{
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
			float delta = Time.smoothDeltaTime;

			// anything less then ~32 fps causes wall clips to be possible

			if (delta >= 1 / 35f)
			{
				delta = 1f / 35f; // prevent wall clips from low framerates
			}

			WallClipFixBehavior self = __instance.GetComponent<WallClipFixBehavior>();
			CameraController cameraController = __instance.GetComponent<CameraController>();

			if (!cameraController.cameraOffsets.ContainsKey("crouch"))
			{
				cameraController.cameraOffsets.Add("crouch", Vector3.zero);
			}

			CharacterController controller = __instance.charCont;

			__instance.GetControls();

			Vector3 floorPosition = __instance.transform.position + __instance.transform.rotation * new Vector3(controller.center.x, controller.center.y - (controller.height / 2f), controller.center.z);
			bool ceilingDetected = Physics.SphereCast(floorPosition, controller.radius, Vector3.up, out RaycastHit ceilingRaycast, characterHeight, ceilingRaycastMask);

			bool blocked = ceilingDetected && ceilingRaycast.distance + controller.radius <= characterHeight;

			self.crouching = __instance.btnDown || (blocked && self.crouching);
			self.proning = __instance.btn2Down || (blocked && self.proning);

			float targetHeight = self.proning ? characterHeight * self.proneMult : (self.crouching ? characterHeight * self.crouchMult : characterHeight);
			Vector3 targetCenter = self.proning ? new Vector3(0f, -0.8f, 0.9f) : (self.crouching ? new Vector3(0f, -0.6f, -0.28f) : new Vector3(0f, 0.1f, -0.28f));
			Vector3 targetCameraOffset = self.proning ? new Vector3(0f, __instance.proneHeight, __instance.proneDis) : (self.crouching ? new Vector3(0f, __instance.crouchHeight, 0f) : Vector3.zero);

			controller.center = Vector3.Lerp(controller.center, targetCenter, Mathf.Min(delta * 15f, 1f));
			controller.height = Mathf.Lerp(controller.height, targetHeight, Mathf.Min(delta * 15f, 1f));
			
			if (HexaModPreferences.smoothCrouching.Value)
			{
				self.crouchHeight = Mathf.Lerp(self.crouchHeight, self.crouching ? __instance.crouchHeight : 0f, Mathf.Min(delta * 15f, 1f));
				cameraController.cameraOffsets["crouch"] = Vector3.Lerp(cameraController.cameraOffsets["crouch"], targetCameraOffset, Mathf.Min(delta * 15f, 1f));
			}
			else
			{
				self.crouchHeight = self.crouching ? __instance.crouchHeight : 0f;
				cameraController.cameraOffsets["crouch"] = targetCameraOffset;
			}

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

	}
}
