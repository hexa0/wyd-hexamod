using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaMod
{
	public class PreferenceLinker : MonoBehaviour
	{
		public static PreferenceLinker instance;

		internal class PreferenceLinkerQueueItem
		{
			internal string preference;
			internal Action action;
		}

		static internal Queue<PreferenceLinkerQueueItem> queue = new Queue<PreferenceLinkerQueueItem>();
		public static void LinkTo(string preference, Action action)
		{
			if (!instance)
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

				instance.preferenceUpdated += (sender, updateEvent) =>
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

		public static void TriggerUpdate(string preference)
		{
			if (instance)
			{
				instance.preferenceUpdated.Invoke(instance, new PreferenceUpdateEvent()
				{
					preference = preference
				});
			}
		}

		void Awake()
		{
			instance = this;
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
