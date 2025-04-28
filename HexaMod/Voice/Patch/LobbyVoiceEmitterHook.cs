using HarmonyLib;
using UnityEngine;

namespace HexaMod.Voice
{
    [HarmonyPatch]
    internal class LobbyVoiceEmitterHook
    {
        static GameObject[] emitters;

        [HarmonyPatch(typeof(PlayerNames), "Start")]
        [HarmonyPrefix]
        static void Start(ref PlayerNames __instance)
        {
            emitters = null;
        }

        [HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
        [HarmonyPrefix]
        static void RefreshNameList(ref PlayerNames __instance)
        {
            if (emitters != null)
            {
                foreach (var emitter in emitters)
                {
                    GameObject.Destroy(emitter);
                }

                emitters = null;
            }

            int players = __instance.daddyPlayerIds.Count + __instance.babyPlayerIds.Count - 1;
            emitters = new GameObject[players];

            Mod.Print($"Making {players} VoiceEmitter object(s).");

            for (int i = 0; i < players; i++)
            {
                emitters[i] = new GameObject($"voice {i}").AddComponent<VoiceEmitter>().gameObject;

                AudioSource voiceSource = emitters[i].AddComponent<AudioSource>();
                voiceSource.spatialBlend = 0f;
                voiceSource.spatialize = false;
                voiceSource.spread = 1f;
                voiceSource.bypassEffects = true;
                voiceSource.loop = true;
                voiceSource.volume = 1f;
            }

            int playerIndex = 0;

            __instance.daddyPlayerIds.ForEach(player => {
                if (player != PhotonNetwork.player)
                {
                    Mod.Print($"Setup VoiceEmitter {playerIndex} (dad)");
                    emitters[playerIndex].GetComponent<VoiceEmitter>().clientId = (ulong)player.ID;
                    playerIndex++;
                }
            });

            __instance.babyPlayerIds.ForEach(player => {
                if (player != PhotonNetwork.player)
                {
                    Mod.Print($"Setup VoiceEmitter {playerIndex} (baby)");
                    emitters[playerIndex].GetComponent<VoiceEmitter>().clientId = (ulong)player.ID;
                    playerIndex++;
                }
            });
        }
    }
}
