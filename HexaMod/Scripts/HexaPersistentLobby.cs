﻿using System;
using System.Collections.Generic;
using HexaMod.SerializableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace HexaMod.Scripts
{
	public class HexaPersistentLobby : MonoBehaviour
	{
		public static HexaPersistentLobby instance;
		void Awake()
		{
			instance = this;
		}

		public void ResetRound()
		{
			lobbySettings.roundNumber = 0;
			CommitChanges();
		}

		public void Reset()
		{
			dads.Clear();
			lobbySettings.roundNumber = 0;
			CommitChanges();
		}

		private LobbySettings lobbySettingsBackup;
		public LobbySettings lobbySettings;
		public Dictionary<int, bool> dads = new Dictionary<int, bool>();
		private LobbySettings oldLobbySettings;
		public UnityEvent lobbySettingsChanged;
		public LobbySettingsChangedEvent currentLobbySettingsEvent = new LobbySettingsChangedEvent();
		public class LobbySettingsChangedEvent
		{
			public LobbySettings oldSettings;
			public LobbySettings newSettings;
		};
		public bool lobbySettingsFailed = false;
		public void Init()
		{
			lobbySettings = new LobbySettings();
			oldLobbySettings = LobbySettings.serializer.MakeUnique(lobbySettings);
			Load();
		}

		private bool inOtherLobby = false;

		public void SetInOtherLobby(bool inOtherLobby)
		{
			this.inOtherLobby = inOtherLobby;

			if (!inOtherLobby)
			{
				lobbySettings = lobbySettingsBackup;
				CommitChanges();
			}
		}

		public void Save()
		{
			byte[] serializedSettings = LobbySettings.serializer.Serialize(lobbySettings);
			PlayerPrefs.SetString("HMV2_LobbySettings", Convert.ToBase64String(serializedSettings));
		}

		public void Load()
		{
			Mod.Print("Loading LobbySettings");

			try
			{
				string data = PlayerPrefs.GetString("HMV2_LobbySettings", "none");

				if (data != "none")
				{
					LobbySettings deserializedSettings = LobbySettings.serializer.Deserialize(Convert.FromBase64String(data));
					deserializedSettings.roundNumber = 0;
					oldLobbySettings = lobbySettings;
					lobbySettings = deserializedSettings;
				}
			}
			catch (Exception e)
			{
				Mod.Error($"LobbySettings failed to load:\n{e}");

				lobbySettingsFailed = true;
			}
		}

		public void CommitChanges()
		{
			currentLobbySettingsEvent.oldSettings = oldLobbySettings;
			currentLobbySettingsEvent.newSettings = lobbySettings;
			instance.lobbySettingsChanged.Invoke();
			HexaGlobal.hexaLobby.SetLobbySettings(lobbySettings);
			oldLobbySettings = LobbySettings.serializer.MakeUnique(lobbySettings);
			if (!inOtherLobby)
			{
				lobbySettingsBackup = lobbySettings;
				Save();
			}
		}
	}
}
