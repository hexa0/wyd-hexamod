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
            RPC("SetVoiceId", PhotonTargets.Others, new object[] { id });
        }

        [PunRPC]
        public void SetVoiceId(ulong id)
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.volume = 1f;
            audioSource.spatialBlend = 1f;
            audioSource.dopplerLevel = 0f;
            audioSource.spatialize = true;
            audioSource.spread = 0f;
            audioSource.bypassEffects = true;
            audioSource.loop = true;

            VoiceEmitter voiceEmitter = gameObject.AddComponent<VoiceEmitter>();
            voiceEmitter.clientId = id;
        }
    }
}
