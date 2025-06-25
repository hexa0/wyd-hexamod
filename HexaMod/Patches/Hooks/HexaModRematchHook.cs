using System.Collections;
using HarmonyLib;
using HexaMod.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod.Patches.Hooks
{
	internal class HexaModRematcher : MonoBehaviour
	{
		public bool rematchInProgress = false;
		public IEnumerator HexaModRematchAsync()
		{
			if (rematchInProgress)
			{
				yield break; // Prevent multiple rematch requests
			}

			Camera currentCamera = Camera.current;

			GameObject menuCamera = GameObject.Find("BackendObjects").Find("MenuCamera");

			Camera menuCameraComponent = menuCamera.GetComponent<Camera>();
			menuCameraComponent.enabled = true;
			menuCameraComponent.fieldOfView = currentCamera.fieldOfView;
			menuCameraComponent.farClipPlane = currentCamera.farClipPlane;
			menuCameraComponent.nearClipPlane = currentCamera.nearClipPlane;
			menuCameraComponent.orthographic = currentCamera.orthographic;
			menuCamera.transform.position = currentCamera.transform.position;
			menuCamera.transform.rotation = currentCamera.transform.rotation;

			menuCamera.SetActive(true);

			if (PhotonNetwork.isMasterClient)
			{
				PhotonNetwork.DestroyAll();
			}

			HexaMenus.fadeOverlay.fader.fadeState = true;
			AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(1);
			sceneLoadOperation.allowSceneActivation = false;
			yield return new WaitForSeconds((1f / HexaMenus.fadeOverlay.fader.fadeSpeed) - 0.1f);
			HexaMenus.loadingOverlay.controller.SetTaskState("LevelLoad", true);
			yield return new WaitForSeconds(0.1f);
			sceneLoadOperation.allowSceneActivation = true;
		}
	}
	[HarmonyPatch(typeof(MenuController))]
	internal class HexaModRematchHook
	{
		[HarmonyPatch("Rematch")]
		[HarmonyPrefix]
		static bool Rematch(ref MenuController __instance)
		{
			__instance.SendMessage("HexaModRematchAsync");

			return false;
		}

		[HarmonyPatch("Start")]
		[HarmonyPostfix]
		static void Start(ref MenuController __instance)
		{
			HexaMenus.fadeOverlay.fader.fadeState = false;
			__instance.gameObject.AddComponent<HexaModRematcher>();
		}
	}
}