using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches
{
    [HarmonyPatch(typeof(FirstPersonController))]
    internal class LandSoundSpam
    {

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void LandSoundSpamPatch(ref FirstPersonController __instance)
        {
            var privateFields = Traverse.Create(__instance);

            if (__instance.isOwner)
            {
                var m_PreviouslyGrounded = privateFields.Field<bool>("m_PreviouslyGrounded");
                var m_CharacterController = privateFields.Field<CharacterController>("m_CharacterController");
                var m_MoveDir = privateFields.Field<Vector3>("m_MoveDir");
                var m_Jumping = privateFields.Field<bool>("m_Jumping");
                bool didCancel = false;

                // this would normally play the sound
                if (!m_PreviouslyGrounded.Value && m_CharacterController.Value.isGrounded)
                {
                    if (m_MoveDir.Value.y >= -4f)
                    {
                        var newMoveDir = m_MoveDir.Value;
                        newMoveDir.y = 0f;
                        m_MoveDir.Value = newMoveDir;
                        didCancel = true;
                    }
                }

                if (didCancel)
                {
                    // due to how we patch this out, this needs to be readded to prevent issues.
                    if (!m_CharacterController.Value.isGrounded && !m_Jumping.Value && !m_PreviouslyGrounded.Value)
                    {
                        var newMoveDir = m_MoveDir.Value;
                        newMoveDir.y = 0f;
                        m_MoveDir.Value = newMoveDir;
                    }

                    m_PreviouslyGrounded.Value = m_CharacterController.Value.isGrounded;
                }
            }
        }
    }
}
