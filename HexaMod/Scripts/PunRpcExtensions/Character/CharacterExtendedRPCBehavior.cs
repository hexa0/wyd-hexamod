﻿using System.Collections;
using HexaMod.Patches.Fixes;
using HexaMod.Scripts.CustomCharacterModels;
using HexaMod.Scripts.PunRpcExtensions.Lobby;
using HexaMod.SerializableObjects;
using HexaMod.UI;
using HexaMod.Voice.Script;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod.Scripts.PunRpcExtensions.Character
{
	public class CharacterExtendedRPCBehavior : MonoBehaviour
	{
		PhotonView netView;
		InitialPlayerState initialState;
		bool processedInitialState = false;
		void Start()
		{
			netView = GetComponent<PhotonView>();

			bool isDad = gameObject.name.Substring(0, 3) == "Dad";

			initialState = new InitialPlayerState()
			{
				shirtColor = new SerializableColor(new Color().FromHex(MainUI.GetCurrentShirtColorHex())),
				skinColor = new SerializableColor(new Color().FromHex(isDad ? MainUI.GetCurrentSkinColorHex() : MainUI.GetCurrentBabySkinColorHex())),
				characterModel = isDad ? PlayerPrefs.GetString("HMV2_DadCharacterModel", "default") : PlayerPrefs.GetString("HMV2_BabyCharacterModel", "default"),
				shirtMaterial = PlayerPrefs.GetString("HMV2_DadShirtMaterial", "default")
			};

			StartCoroutine(SendInitalState());
		}

		void ProcessInitialState()
		{
			if (processedInitialState)
			{
				return;
			}

			processedInitialState = true;
			GetComponent<FirstPersonController>().playerName = HexaLobby.GetPlayerName(netView.owner);
			GetComponent<CharacterModelSwapper>().SetShirtColor(initialState.shirtColor.toColor());
			GetComponent<CharacterModelSwapper>().SetSkinColor(initialState.skinColor.toColor());
			GetComponent<CharacterModelSwapper>().SetCharacterModel(initialState.characterModel);
			GetComponent<CharacterModelSwapper>().SetShirt(initialState.shirtMaterial);
			GetComponent<PlayerVoiceEmitterRPC>().SetVoicePlayer(netView.owner);
		}

		IEnumerator SendInitalState()
		{
			if (gameObject == HexaGlobal.networkManager.playerObj)
			{
				yield return 0;

				// this still sometimes drops and there's no way to send a reliable RPC event

				for (int i = 0; i < 5; i++)
				{
					SetInitialStateForOthers();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}

		public void RPC(string method, PhotonTargets target, params object[] param)
		{
			netView.RPC(method, target, param);
		}

		public void SetInitialStateForOthers()
		{
			ProcessInitialState();
			RPC("SetInitialState", PhotonTargets.Others, new object[] { InitialPlayerState.serializer.Serialize(initialState) });
		}

		[PunRPC]
		public void SetInitialState(byte[] data)
		{
			initialState = InitialPlayerState.serializer.Deserialize(data);
			ProcessInitialState();
		}

		[PunRPC]
		public void FixNan(Vector3 characterPosition, Quaternion characterRotation, Vector3 cameraPosition, Quaternion cameraRotation)
		{
			NaNFixBehavior nanFixBehavior = gameObject.AddComponent<NaNFixBehavior>();
			nanFixBehavior.firstPersonController = gameObject.GetComponent<FirstPersonController>();
			nanFixBehavior.characterPosition = characterPosition;
			nanFixBehavior.characterRotation = characterRotation;
			nanFixBehavior.cameraPosition = cameraPosition;
			nanFixBehavior.cameraRotation = cameraRotation;
		}
	}
}
