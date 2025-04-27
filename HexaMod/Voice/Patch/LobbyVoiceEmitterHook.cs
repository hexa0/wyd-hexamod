using HarmonyLib;
using UnityEngine;

namespace HexaMod.Voice
{
    [HarmonyPatch]
    internal class LobbyVoiceEmitterHook
    {
        static VoiceEmitter[] emitters;

        [HarmonyPatch(typeof(PlayerNames), "Start")]
        [HarmonyPrefix]
        static void Start(ref PlayerNames __instance)
        {
            emitters = null;
            AudioSource voiceSource = __instance.gameObject.AddComponent<AudioSource>();
            voiceSource.spatialBlend = 0f;
            voiceSource.spatialize = false;
            voiceSource.spread = 1f;
            voiceSource.bypassEffects = true;
            voiceSource.loop = true;
            voiceSource.volume = 1f;
        }

        [HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
        [HarmonyPrefix]
        static void RefreshNameList(ref PlayerNames __instance)
        {
            if (emitters != null)
            {
                foreach (var emitter in emitters)
                {
                    GameObject.DestroyImmediate(emitter);
                }

                emitters = null;
            }

            int players = __instance.daddyPlayerIds.Count + __instance.babyPlayerIds.Count;
            emitters = new VoiceEmitter[players - 1];

            Mod.Print($"Making {players - 1} VoiceEmitter object(s).");

            for (int i = 0; i < players - 1; i++)
            {
                emitters[i] = __instance.gameObject.AddComponent<VoiceEmitter>();
            }

            int playerIndex = 0;

            __instance.daddyPlayerIds.ForEach(player => {
                if (player != PhotonNetwork.player)
                {
                    Mod.Print($"Setup VoiceEmitter {playerIndex} (dad)");
                    emitters[playerIndex].clientId = (ulong)player.ID;
                    playerIndex++;
                }
            });

            __instance.babyPlayerIds.ForEach(player => {
                if (player != PhotonNetwork.player)
                {
                    Mod.Print($"Setup VoiceEmitter {playerIndex} (baby)");
                    emitters[playerIndex].clientId = (ulong)player.ID;
                    playerIndex++;
                }
            });
        }
    }
}
