using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaMod
{
	public class PreferenceLinker : MonoBehaviour
	{
		internal class PreferenceLinkerQueueItem
		{
			internal string preference;
			internal Action action;
		}

		static internal Queue<PreferenceLinkerQueueItem> queue = new Queue<PreferenceLinkerQueueItem>();
		public static void LinkTo(string preference, Action action)
		{
			if (!HexaMod.preferenceLinker)
			{
				queue.Enqueue(new PreferenceLinkerQueueItem
				{
					preference = preference,
					action = action
				});
			}
			else
			{
				action.Invoke();

				HexaMod.preferenceLinker.preferenceUpdated += (sender, updateEvent) =>
				{
					if (updateEvent.preference == preference)
					{
						action.Invoke();
					}
				};
			}
		}

		private class PreferenceUpdateEvent : EventArgs
		{
			public string preference;
		}

		private event EventHandler<PreferenceUpdateEvent> preferenceUpdated;

		public void TriggerUpdate(string preference)
		{
			preferenceUpdated.Invoke(this, new PreferenceUpdateEvent()
			{
				preference = preference
			});
		}

		void Start()
		{
			for (int i = 0; i < queue.Count;)
			{
				PreferenceLinkerQueueItem item = queue.Dequeue();

				item.action.Invoke();

				preferenceUpdated += (sender, updateEvent) =>
				{
					if (updateEvent.preference == item.preference)
					{
						item.action.Invoke();
					}
				};
			}
		}
	}
}
