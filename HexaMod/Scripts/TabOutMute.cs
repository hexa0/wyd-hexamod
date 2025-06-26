using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod.Scripts
{
	public class TabOutMute : MonoBehaviour
	{
		public static TabOutMute instance;
		internal static class VolumeState
		{
			public static bool lastTabbedOut = true;
		}

		void Awake()
		{
			instance = this;
		}

		void Start()
		{
			UpdateFocusedState(IsFocused());

			SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
			{
				UpdateFocusedState(IsFocused());
				StartCoroutine(DelayedStart());
			};
		}
		public static bool currentlyFocused = true;
		public static bool tabOutMuteEnabled = true;

		public void UpdateFocusedState(bool state)
		{
			AudioListener.volume = state ? PlayerPrefs.GetFloat("MasterVolume", 0.75f) : 0f;
		}

		public static void SetEnabled(bool enabled)
		{
			tabOutMuteEnabled = enabled;
		}

		public bool IsFocused()
		{
			return !tabOutMuteEnabled || currentlyFocused;
		}

		void OnApplicationFocus(bool hasFocus)
		{
			currentlyFocused = hasFocus;
		}


		public void Update()
		{
			bool isFocused = IsFocused();

			if (isFocused != VolumeState.lastTabbedOut)
			{
				VolumeState.lastTabbedOut = isFocused;
				UpdateFocusedState(isFocused);
			}
		}

		public IEnumerator DelayedStart()
		{
			yield return new WaitForSeconds(1f);
			UpdateFocusedState(IsFocused());
		}
	}
}
