using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
        public UInt16 roundNumber = 0;

        public static byte[] Serialize(LobbySettings lobby)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.Serialize(memoryStream, lobby);

                return memoryStream.ToArray();
            }
        }

        public static LobbySettings Deserialize(byte[] dataStream)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter serializer = new BinaryFormatter();

                memoryStream.Write(dataStream, 0, dataStream.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);

                return (LobbySettings)serializer.Deserialize(memoryStream);
            }
        }

        public static LobbySettings Copy(LobbySettings lobby)
        {
            return Deserialize(Serialize(lobby));
        }
    }
}
