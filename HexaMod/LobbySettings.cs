using System;
using HexaMod.Util;

namespace HexaMod
{

    // [XmlRoot("LobbySettings", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
    [Serializable]
    public class LobbySettings
    {
        public bool allMustDie = true;
        public bool shufflePlayers = false;
        public bool disablePets = true;
        public bool doorSounds = true;
        public bool modernGrabbing = true;
        public bool cheats = true;
        public byte GameMode = 0; // Todo: switching game modes in the lobby & quit to lobby instead of the menu button
        public string mapName = "Default";
        public string relay = "127.0.0.1";
        public ushort roundNumber = 0;

        public static ClassSerializer<LobbySettings> serializer = new ClassSerializer<LobbySettings>();
    }
}
