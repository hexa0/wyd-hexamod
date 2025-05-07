using System;
using System.Collections;
using UnityEngine;

namespace HexaMod.Voice
{
	public class PlayerVoiceEmitterRPC : MonoBehaviour
	{
		PhotonView netView;
		void Start()
		{
			netView = GetComponent<PhotonView>();
		}

		AudioSource voiceSource;

		public void SetVoiceId(ulong id)
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
			voiceEmitter.clientId = id;

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
