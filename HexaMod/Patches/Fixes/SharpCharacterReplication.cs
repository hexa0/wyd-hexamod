using System;
using Boo.Lang.Runtime;
using HarmonyLib;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Patches.Fixes
{
	[Serializable]
	internal class CharacterReplication : Photon.MonoBehaviour
	{
		FirstPersonController playerController;
		CharacterController characterController;
		NetworkMovement networkMovement;
		Vector3 lastPosition;
		Quaternion lastRotation;

		Vector3 nextPosition;
		Quaternion nextRotation;

		float lastTime = Time.time;
		float timeScale = PhotonNetwork.sendRateOnSerialize;


		void Awake()
		{
			playerController = transform.GetComponent<FirstPersonController>();
			characterController = transform.GetComponent<CharacterController>();
			networkMovement = transform.GetComponent<NetworkMovement>();

			lastPosition = transform.position;
			lastRotation = transform.rotation;

			photonView.ObservedComponents.Add(this);
		}

		void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			if (HexaLobby.HexaLobbyState.handledPlayersLoaded)
			{
				if (stream.isWriting)
				{
					Vector3 position = transform.position;
					Quaternion rotation = transform.rotation;

					if (float.IsNaN(position.y))
					{
						position = Vector3.zero;
					}

					if (float.IsNaN(rotation.y))
					{
						rotation = Quaternion.identity;
					}

					stream.Serialize(ref position);
					stream.Serialize(ref rotation);

					if (networkMovement.useFpc)
					{
						float energy = playerController.energy;
						stream.Serialize(ref energy);
					}
				}
				else
				{
					lastPosition = nextPosition;
					lastRotation = nextRotation;

					nextPosition = (Vector3)stream.ReceiveNext();
					nextRotation = (Quaternion)stream.ReceiveNext();

					if (networkMovement.useFpc)
					{
						playerController.energy = RuntimeServices.UnboxSingle(stream.ReceiveNext());
					}

					if (lastTime != Time.time)
					{
						// timeScale = 1f / (Time.time - lastTime);
						// timeScale = Mathf.Clamp(timeScale, 0f, PhotonNetwork.sendRateOnSerialize);
						timeScale = PhotonNetwork.sendRateOnSerialize;
						lastTime = Time.time;
					}
				}
			}
		}

		void Update()
		{;
			if (!photonView.isMine)
			{
				var privateFields = Traverse.Create(networkMovement);

				var turnedOff = privateFields.Field<bool>("turnedOff");

				if (!turnedOff.Value)
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

	[HarmonyPatch(typeof(NetworkMovement))]
	internal class SharpCharacterReplication
	{
		[HarmonyPatch("Start")]
		[HarmonyPrefix]
		static bool Start(ref NetworkMovement __instance)
		{
			__instance.gameObject.AddComponent<CharacterReplication>();
			return false;
		}

		[HarmonyPatch("OnPhotonSerializeView")]
		[HarmonyPrefix]
		static bool OnPhotonSerializeView()
		{
			return false;
		}

		[HarmonyPatch("Update")]
		[HarmonyPrefix]
		static bool Update()
		{
			return false;
		}

		[HarmonyPatch("SyncedMovement")]
		[HarmonyPrefix]
		static bool SyncedMovement()
		{
			return false;
		}
	}
}
