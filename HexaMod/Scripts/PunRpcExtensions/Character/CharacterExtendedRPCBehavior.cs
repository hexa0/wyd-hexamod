using System.Collections;
using HexaMod.Patches;
using HexaMod.UI;
using HexaMod.Util;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace HexaMod
{
	public class CharacterExtendedRPCBehavior : MonoBehaviour
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
				SetShirtColorForOthers(HexToColor.GetColorFromHex(MainUI.GetCurrentShirtColorHex()));
				SetSkinColorForOthers(HexToColor.GetColorFromHex(MainUI.GetCurrentSkinColorHex()));
				SetCharacterModelForOthers(PlayerPrefs.GetString("HMV2_DadCharacterModel", "default"));
				FixUsernameForOthers(PlayerPrefs.GetString("LobbyName", "Player"));
			}
		}

		public void RPC(string method, PhotonTargets target, params object[] param)
		{
			netView.RPC(method, target, param);
		}

		public void SetShirtColorForOthers(Color newColor)
		{
			RPC("SetShirtColor", PhotonTargets.All, new object[] { newColor.r, newColor.g, newColor.b });
		}

		public void SetSkinColorForOthers(Color newColor)
		{
			RPC("SetSkinColor", PhotonTargets.All, new object[] { newColor.r, newColor.g, newColor.b });
		}

		public void SetCharacterModelForOthers(string modelName)
		{
			RPC("SetCharacterModel", PhotonTargets.All, new object[] { modelName });
		}

		public void FixUsernameForOthers(string username)
		{
			RPC("FixUsername", PhotonTargets.All, new object[] { username });
		}

		[PunRPC]
		public void FixUsername(string username)
		{
			GetComponent<FirstPersonController>().playerName = username;
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
