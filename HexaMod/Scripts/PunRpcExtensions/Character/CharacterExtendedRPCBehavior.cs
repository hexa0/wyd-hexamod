using System.Collections;
using HexaMod.Patches.Fixes;
using HexaMod.SerializableObjects;
using HexaMod.UI;
using HexaMod.Util;
using HexaMod.Voice;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod
{
	public class CharacterExtendedRPCBehavior : MonoBehaviour
	{
		PhotonView netView;
		InitialPlayerState initialState;
		bool processedInitialState = false;
		void Start()
		{
			netView = GetComponent<PhotonView>();
			initialState = new InitialPlayerState()
			{
				shirtColor = new SerializableColor(HexToColor.GetColorFromHex(MainUI.GetCurrentShirtColorHex())),
				skinColor = new SerializableColor(HexToColor.GetColorFromHex(MainUI.GetCurrentSkinColorHex())),
				characterModel = PlayerPrefs.GetString("HMV2_DadCharacterModel", "default"),
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
			GetComponent<FirstPersonController>().playerName = netView.owner.NickName;
			GetComponent<CharacterModelSwapper>().SetShirtColor(initialState.shirtColor.toColor());
			GetComponent<CharacterModelSwapper>().SetSkinColor(initialState.skinColor.toColor());
			GetComponent<CharacterModelSwapper>().SetCharacterModel(initialState.characterModel);
			GetComponent<CharacterModelSwapper>().SetShirt(initialState.shirtMaterial);
			GetComponent<PlayerVoiceEmitterRPC>().SetVoiceId((ulong)netView.owner.ID);
		}

		IEnumerator SendInitalState()
		{
			if (gameObject == HexaMod.networkManager.playerObj)
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
