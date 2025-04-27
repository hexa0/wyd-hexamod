using System.Collections;
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
        public void SetShirtColor(float R, float G, float B)
        {
            Color shirtColor = new Color(R, G, B);

            var dadModel = transform.Find("DadModel");
            if (dadModel)
            {
                var body = dadModel.Find("generic_male_01.005").GetComponent<SkinnedMeshRenderer>();
                var shirtMaterial = body.materials[4];
                shirtMaterial.color = shirtColor;
            }
            else
            {
                var babyModel = transform.Find("Baby001");
                if (babyModel)
                {

                }
            }
        }
    }
}
