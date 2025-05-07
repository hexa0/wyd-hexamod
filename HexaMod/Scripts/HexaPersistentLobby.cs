using System;
using System.Collections.Generic;
using HexaMod.SerializableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace HexaMod
{
	public class HexaPersistentLobby : MonoBehaviour
	{
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

		public void Init()
		{
			lobbySettings = new LobbySettings();
			oldLobbySettings = LobbySettings.serializer.MakeUnique(lobbySettings);
			Load();
		}

		private bool inOtherLobby = false;

		public void SetInOtherLobby(bool inLobbyNew)
		{
			inOtherLobby = inLobbyNew;

			if (!inLobbyNew)
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
					Mod.Print("Setup Saved Relay");
				}
			}
			catch (Exception e)
			{
				Mod.Error($"LobbySettings failed to load:\n{e}");
			}
		}

		public void CommitChanges()
		{
			currentLobbySettingsEvent.oldSettings = oldLobbySettings;
			currentLobbySettingsEvent.newSettings = lobbySettings;
			HexaMod.persistentLobby.lobbySettingsChanged.Invoke();
			HexaMod.hexaLobby.SetLobbySettings(lobbySettings, oldLobbySettings);
			oldLobbySettings = LobbySettings.serializer.MakeUnique(lobbySettings);
			if (!inOtherLobby)
			{
				lobbySettingsBackup = lobbySettings;
				Save();
			}
		}
	}
}
