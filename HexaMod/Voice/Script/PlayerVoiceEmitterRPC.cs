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
            yield return new WaitForSeconds(0.5f);

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

            AudioSource audioSource = model.AddComponent<AudioSource>();
            audioSource.playOnAwake = true;
            audioSource.volume = 1f;
            audioSource.maxDistance = 25f;
            audioSource.minDistance = 1.5f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.spatialBlend = 1f;
            audioSource.dopplerLevel = 0f;
            audioSource.spatialize = true;
            audioSource.spread = 0f;
            audioSource.bypassEffects = true;
            audioSource.loop = true;

            VoiceEmitter voiceEmitter = model.AddComponent<VoiceEmitter>();
            voiceEmitter.clientId = BitConverter.ToUInt64(id, 0);
        }
    }
}
