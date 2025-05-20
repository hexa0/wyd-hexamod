using HarmonyLib;
using HexaMod.Scripts;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
	[HarmonyPatch(typeof(FirstPersonController))]
	internal class CharacterSoundOverlapFix
	{
		[HarmonyPatch("PlayLandingSound")]
		[HarmonyPrefix]
		static bool PlayLandingSound(ref FirstPersonController __instance)
		{
			Traverse<AudioClip> m_LandSound = Traverse.Create(__instance).Field<AudioClip>("m_LandSound");
			Traverse<float> m_NextStep = Traverse.Create(__instance).Field<float>("m_NextStep");
			Traverse<float> m_StepCycle = Traverse.Create(__instance).Field<float>("m_StepCycle");

			NetworkedSoundBehavior netSound = __instance.GetComponent<NetworkedSoundBehavior>();

			netSound.Play(m_LandSound.Value);
			m_NextStep.Value = m_StepCycle.Value + 0.5f;
			return false;
		}

		[HarmonyPatch("PlayJumpSound")]
		[HarmonyPrefix]
		static bool PlayJumpSound(ref FirstPersonController __instance)
		{
			Traverse fields = Traverse.Create(__instance);
			Traverse<AudioClip> m_JumpSound = fields.Field<AudioClip>("m_JumpSound");

			NetworkedSoundBehavior netSound = __instance.GetComponent<NetworkedSoundBehavior>();

			netSound.Play(m_JumpSound.Value);
			return false;
		}

		[HarmonyPatch("PlayFootStepAudio")]
		[HarmonyPrefix]
		static bool PlayFootStepAudio(ref FirstPersonController __instance)
		{
			Traverse fields = Traverse.Create(__instance);
			Traverse<CharacterController> m_CharacterController = fields.Field<CharacterController>("m_CharacterController");

			if (m_CharacterController.Value.isGrounded)
			{
				Traverse<AudioClip[]> m_FootstepSounds = fields.Field<AudioClip[]>("m_FootstepSounds");

				int randomSound = Random.Range(1, m_FootstepSounds.Value.Length);

				AudioClip sound = m_FootstepSounds.Value[randomSound];

				NetworkedSoundBehavior netSound = __instance.GetComponent<NetworkedSoundBehavior>();
				float baseSpeed = HexaMod.networkManager.isDad ? 4f : 1f;
				netSound.Play(sound, Mathf.Clamp(m_CharacterController.Value.velocity.magnitude / baseSpeed, 0f, 1f));

				m_FootstepSounds.Value[randomSound] = m_FootstepSounds.Value[0];
				m_FootstepSounds.Value[0] = sound;

				Traverse<Vector3> m_MoveDir = fields.Field<Vector3>("m_MoveDir");
			}

			return false;
		}
	}
}
