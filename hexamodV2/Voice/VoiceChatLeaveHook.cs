using HarmonyLib;

namespace HexaMod.Voice
{
    [HarmonyPatch]
    internal class VoiceChatLeaveHook
    {
        [HarmonyPatch(typeof(PhotonNetwork), "LeaveRoom")]
        [HarmonyPrefix]
        static void LeaveRoom()
        {
            VoiceChat.LeaveVoiceRoom();
        }
    }
}
