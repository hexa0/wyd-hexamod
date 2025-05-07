using System.Collections.Generic;
using UnityEngine;
using static HexaMod.UI.Util.Menu;
using static HexaMod.UI.Util.Menu.Menus;

namespace HexaMod.UI
{
	public class LoadingController : MonoBehaviour
	{
		private static GameObject loadingAnimation = HexaMod.startupBundle.LoadAsset<GameObject>("Assets/ModResources/Init/LoadingUI/HexaLoadingAnimation1080.prefab");
		private GameObject currentLoadingAnimation;
		private Dictionary<string, bool> tasks = new Dictionary<string, bool>();

		public LoadingController Init()
		{
			currentLoadingAnimation = Instantiate(loadingAnimation);
			currentLoadingAnimation.transform.SetParent(menuCanvas.transform, false);
			currentLoadingAnimation.SetActive(currentlyShown);

			return this;
		}

		public void ResetTasks()
		{
			tasks.Clear();
		}

		public void SetTaskState(string taskName, bool working)
		{
			tasks[taskName] = working;
		}

		public bool GetTaskState(string taskName)
		{
			return tasks.ContainsKey(taskName) ? tasks[taskName] : false;
		}

		private bool currentlyShown = false;
		private void ShowLoading(bool loadingShown)
		{
			if (loadingShown != currentlyShown)
			{
				currentlyShown = loadingShown;

				currentLoadingAnimation.SetActive(loadingShown);
				title.root.Find("Version").gameObject.SetActive(!loadingShown);
			}
		}

		private float lastWasWorking = 0f;
		private void Update()
		{
			var isCurrentlyWorking = tasks.ContainsValue(true);

			if (isCurrentlyWorking)
			{
				lastWasWorking = Time.time;
				ShowLoading(true);
			}
			else
			{
				ShowLoading((Time.time - lastWasWorking) < 0.1f);
			}
		}
	}
}
