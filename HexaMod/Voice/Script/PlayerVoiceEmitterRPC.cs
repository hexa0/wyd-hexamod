using System;
using System.Collections;
using HexaMod.Voice;
using UnityEngine;

namespace HexaMod
{
    public class PlayerVoiceEmitterRPC : MonoBehaviour
    {
        PhotonView netView;
        void Start()
        {
            netView = GetComponent<PhotonView>();

            StartCoroutine(Wait());
        }

        IEnumerator Wait()
        {
            yield return 0;

            if (gameObject.name == HexaMod.networkManager.playerObj.name)
            {
                SetVoiceForOthers((ulong)PhotonNetwork.player.ID);
            }
        }

        public void RPC(string method, PhotonTargets target, params object[] param)
        {
            netView.RPC(method, target, param);
        }

        public void SetVoiceForOthers(ulong id)
        {
            RPC("SetVoiceId", PhotonTargets.Others, new object[] { BitConverter.GetBytes(id) });
        }

        AudioSource voiceSource;

        [PunRPC]
        public void SetVoiceId(byte[] id)
        {
            GameObject model = gameObject;
            var dadModel = transform.Find("DadModel");
            if (dadModel)
            {
                model = dadModel.gameObject;
            }
            else
            {
                var babyModel = transform.Find("Baby001");
                if (babyModel)
                {
                    model = babyModel.gameObject;
                }
            }

            voiceSource = model.AddComponent<AudioSource>();
            voiceSource.playOnAwake = true;
            voiceSource.volume = 1f;
            voiceSource.maxDistance = 25f;
            voiceSource.minDistance = 1.5f;
            voiceSource.rolloffMode = AudioRolloffMode.Linear;
            voiceSource.spatialBlend = 1f;
            voiceSource.dopplerLevel = 0f;
            voiceSource.spatialize = true;
            voiceSource.spread = 0f;
            voiceSource.bypassEffects = true;
            voiceSource.loop = true;

            VoiceEmitter voiceEmitter = model.AddComponent<VoiceEmitter>();
            voiceEmitter.clientId = BitConverter.ToUInt64(id, 0);

            voiceEmitter.enabled = false;
            voiceSource.enabled = false;

            voiceSource.enabled = true;
            voiceEmitter.enabled = true;
        }

        public void FixedUpdate()
        {
            if (HexaMod.gameStateController != null && voiceSource != null)
            {
                if (HexaMod.gameStateController.gameOver)
                {
                    voiceSource.spatialBlend = 0f;
                }
            }
        }
    }
}
