using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
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

		private static Dictionary<string, string> taskNames = new Dictionary<string, string>()
		{
			{ "PhotonConnect", "Connecting to Photon" },
			{ "LobbyJoin", "Fetching lobby list" },
			{ "RoomLookForOrCreateTag", "Searching for lobby" },
			{ "RoomCreate", "Creating lobby" },
			{ "RoomJoin", "Joining lobby" },
			{ "LevelLoad", "Loading Game" },
			{ "MatchLoad", "Waiting for others to load" }
		}
;

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

			List<string> activeTasks = new List<string>();
			foreach (var task in tasks)
			{
				if (task.Value)
				{
					if (taskNames.ContainsKey(task.Key))
					{
						activeTasks.Add(taskNames[task.Key]);
					}
					else
					{
						Mod.Warn($"Unknown task name: {task.Key}");
						activeTasks.Add(task.Key);
					}
				}
			}

			if (activeTasks.Count > 0)
			{
				HexaMenus.loadingOverlay.cornerLoadingAnimation.loadingText.SetText(string.Join(", ", activeTasks.ToArray()));
			}
		}
	}
}
