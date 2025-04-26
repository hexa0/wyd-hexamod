using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HexaMod
{
    public class HexaPersistentLobby : MonoBehaviour
    {
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
            oldLobbySettings = lobbySettings;
            lobbySettings.mapName = PlayerPrefs.GetString("HMV2_CustomMap", "Default");
        }

        public void CommitChanges()
        {
            currentLobbySettingsEvent.oldSettings = oldLobbySettings;
            currentLobbySettingsEvent.newSettings = lobbySettings;
            HexaMod.persistentLobby.lobbySettingsChanged.Invoke();
            HexaMod.hexaLobby.SetLobbySettings(lobbySettings, oldLobbySettings);
        }
    }
}
