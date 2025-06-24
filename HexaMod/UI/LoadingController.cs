using System.Collections.Generic;
using HexaMod.UI.Menus;
using UnityEngine;
using static HexaMod.UI.Util.Menu;
using static HexaMod.UI.Util.Menu.WYDMenus;

namespace HexaMod.UI
{
	public class LoadingController : MonoBehaviour
	{
		private Dictionary<string, bool> tasks = new Dictionary<string, bool>();

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

				HexaMenus.loadingOverlay.fader.fadeState = loadingShown;
				if (title != null)
				{
					title.root.Find("Version").gameObject.SetActive(!loadingShown);
				}
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
