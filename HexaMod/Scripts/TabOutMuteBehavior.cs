﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaMod
{
	public class TabOutMuteBehavior : MonoBehaviour
	{
		internal static class VolumeState
		{
			public static bool lastTabbedOut = true;
		}

		public void Start()
		{
			UpdateFocusedState(IsFocused());

			SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode loadingMode)
			{
				UpdateFocusedState(IsFocused());
				StartCoroutine(DelayedStart());
			};
		}
		public bool currentlyFocused = true;
		public bool tabOutMuteEnabled = true;

		public void UpdateFocusedState(bool state)
		{
			AudioListener.volume = state ? PlayerPrefs.GetFloat("MasterVolume", 0.75f) : 0f;
		}

		public bool IsFocused()
		{
			return tabOutMuteEnabled ? currentlyFocused : true;
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
