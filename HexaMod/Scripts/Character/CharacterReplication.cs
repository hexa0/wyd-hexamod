using System;
using HexaMod.Scripts.Character.Controller.Character;
using HexaMod.Scripts.Multiplayer.Lobby;
using UnityEngine;

namespace HexaMod.Scripts.Character
{
	[Serializable]
	internal class CharacterReplication : Photon.MonoBehaviour, IPunObservable
	{
		HexaPlayerController PlayerController => GetComponent<HexaPlayerController>();
		Vector3 lastPosition;
		Quaternion lastRotation;

		Vector3 nextPosition;
		Quaternion nextRotation;

		float lastTime = Time.time;
		float timeScale = PhotonNetwork.sendRateOnSerialize;

		public bool turnedOff = false;

		void Start()
		{
			lastPosition = transform.position;
			lastRotation = transform.rotation;

			photonView.ObservedComponents.Add(this);
			photonView.synchronization = ViewSynchronization.Unreliable;
		}

		void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (HexaLobby.HexaLobbyState.handledPlayersLoaded)
			{
				if (stream.isWriting)
				{
					Vector3 position = transform.position;
					Quaternion rotation = transform.rotation;

					if (float.IsNaN(position.y) || float.IsNaN(rotation.y))
					{
						position = lastPosition;
						rotation = lastRotation;
					}
					else
					{
						lastPosition = position;
						lastRotation = rotation;
					}

					stream.Serialize(ref position);
					stream.Serialize(ref rotation);

					if (PlayerController)
					{
						stream.Serialize(ref PlayerController.energy);
					}
				}
				else if (stream.isReading)
				{
					lastPosition = nextPosition;
					lastRotation = nextRotation;

					// check if the stream has enough data to read
					if (stream.Count < (PlayerController ? 7 : 5))
					{
						Mod.Warn("CharacterReplication: Not enough data in stream to read position and rotation.");
						return;
					}

					try
					{
						nextPosition = (Vector3)stream.ReceiveNext();
						nextRotation = (Quaternion)stream.ReceiveNext();

						if (PlayerController)
						{
							PlayerController.energy = (float)stream.ReceiveNext();
						}

						if (lastTime != Time.time)
						{
							timeScale = PhotonNetwork.sendRateOnSerialize;
							lastTime = Time.time;
						}
					}
					catch
					{
						// despite the extensive size check this still can fail with an out of bounds array exception
						// it makes no sense but this catches that instead of throwing a massive error in the console (despite it not actually mattering)
						nextPosition = Vector3.zero;
						nextRotation = Quaternion.identity;

						if (PlayerController)
						{
							PlayerController.energy = 0f;
						}

						lastTime = Time.time;
						timeScale = PhotonNetwork.sendRateOnSerialize;
					}
				}
			}
		}

		public virtual void TurnOff()
		{
			turnedOff = true;
		}

		public virtual void TurnOn()
		{
			turnedOff = false;
		}

		void Update()
		{
			if (!photonView.isMine)
			{
				if (!turnedOff)
				{
					float scaledTime = (Time.time - lastTime) * timeScale;
					Vector3 targetPosition = Vector3.LerpUnclamped(lastPosition, nextPosition, Mathf.Clamp(scaledTime, 0f, 1.1f) + 0.75f);
					Quaternion targetRotation = Quaternion.LerpUnclamped(lastRotation, nextRotation, Mathf.Clamp(scaledTime, 0f, 1.1f) + 0.75f);
					transform.position = Vector3.Lerp(transform.position, targetPosition, Time.smoothDeltaTime * 35f);
					transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.smoothDeltaTime * 35f);
					if ((lastPosition - targetPosition).magnitude > 2f)
					{
						transform.position = nextPosition;
						transform.rotation = nextRotation;
					}
				}
			}
		}
	}
}
