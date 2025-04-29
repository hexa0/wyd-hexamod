using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HexaMod.Voice
{
    [HarmonyPatch]
    internal class LobbyVoiceEmitterHook
    {
        static GameObject[] emitters;
        static GameObject[] dadIndicators;
        static GameObject[] babyIndicators;

        [HarmonyPatch(typeof(PlayerNames), "Start")]
        [HarmonyPrefix]
        static void Start(ref PlayerNames __instance)
        {
            emitters = null;

            // local x 20
            // remove button
            // remove image
            // set text to *

/*            void onNameItem(GameObject gameObject)
            {
                var speakingIndicator = Object.Instantiate(gameObject.transform.Find("kickPlayer"), gameObject.transform);
                speakingIndicator.name = "Speaking";

                Object.Destroy(speakingIndicator.GetComponent<Button>());
                Object.Destroy(speakingIndicator.GetComponent<Image>());

                speakingIndicator.localPosition = new Vector2(20f, speakingIndicator.localPosition.y);
                speakingIndicator.GetComponent<Text>().text = "*";
            }

            foreach (var item in __instance.daddyNames)
            {
                onNameItem(item.gameObject);
            }

            foreach (var item in __instance.babyNames)
            {
                onNameItem(item.gameObject);
            }*/
        }

        [HarmonyPatch(typeof(PlayerNames), "RefreshNameList")]
        [HarmonyPrefix]
        static void RefreshNameList(ref PlayerNames __instance)
        {
            if (emitters != null)
            {
                foreach (var emitter in emitters)
                {
                    Object.Destroy(emitter);
                }

                emitters = null;
            }

            if (dadIndicators != null)
            {
                foreach (var indicator in dadIndicators)
                {
                    Object.Destroy(indicator);
                }

                dadIndicators = null;
            }

            if (babyIndicators != null)
            {
                foreach (var indicator in babyIndicators)
                {
                    Object.Destroy(indicator);
                }

                babyIndicators = null;
            }

            int players = __instance.daddyPlayerIds.Count + __instance.babyPlayerIds.Count - 1;
            emitters = new GameObject[players];

            Mod.Print($"Making {players} VoiceEmitter object(s).");

            for (int i = 0; i < players; i++)
            {
                emitters[i] = new GameObject($"voice {i}");

                AudioSource voiceSource = emitters[i].AddComponent<AudioSource>();
                voiceSource.spatialBlend = 0f;
                voiceSource.spatialize = false;
                voiceSource.spread = 1f;
                voiceSource.bypassEffects = true;
                voiceSource.loop = true;
                voiceSource.volume = 1f;

                VoiceEmitter voiceEmitter = emitters[i].AddComponent<VoiceEmitter>();

                voiceEmitter.enabled = false;
                voiceSource.enabled = false;

                voiceSource.enabled = true;
                voiceEmitter.enabled = true;
            }

            int playerIndex = 0;

            dadIndicators = new GameObject[__instance.daddyPlayerIds.Count];
            babyIndicators = new GameObject[__instance.babyPlayerIds.Count];

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
