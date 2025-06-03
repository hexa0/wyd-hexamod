using System.Linq;
using HexaMod.UI.Class;
using HexaMod.Util;

namespace HexaMod.SerializableObjects
{
	public enum ShufflePlayersMode : byte
	{
		Off,
		Alternate,
		Shuffle,
	}

	public enum SpawnLocationMode : byte
	{
		Vanilla,
		Ideal,
		All,
	}

	// [XmlRoot("LobbySettings", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class LobbySettings
	{
		public bool allMustDie = true; // TODO: make this a percentage for more customization, needs a UI component first though
		public bool allowSpectating = true;
		public ShufflePlayersMode shufflePlayers = ShufflePlayersMode.Alternate;
		public SpawnLocationMode spawnMode = SpawnLocationMode.Ideal;
		public bool disablePets = true;
		public bool doorSounds = true;
		public bool ventSounds = true;
		public bool modernGrabbing = true;
		public bool cheats = true;
		public byte GameMode = 0; // TODO: switching game modes in the lobby & quit to lobby instead of the menu button
		public string mapName = Assets.defaultLevelName;
		public string relay = "127.0.0.1";
		public ushort roundNumber = 0;

		public static WYDSwitchOption<ShufflePlayersMode>[] shuffleOptions = new WYDSwitchOption<ShufflePlayersMode>[]
		{
			new WYDSwitchOption<ShufflePlayersMode>()
			{
				name = "Keep Teams",
				value = ShufflePlayersMode.Off
			},

			new WYDSwitchOption<ShufflePlayersMode>()
			{
				name = "Alternate Teams",
				value = ShufflePlayersMode.Alternate
			},

			new WYDSwitchOption<ShufflePlayersMode>()
			{
				name = "Shuffle Teams (Unfinished DO NOT USE)",
				value = ShufflePlayersMode.Shuffle
			}
		};

		public static WYDSwitchOption<SpawnLocationMode>[] spawnOptions = new WYDSwitchOption<SpawnLocationMode>[]
{
			new WYDSwitchOption<SpawnLocationMode>()
			{
				name = "Vanilla Spawns",
				value = SpawnLocationMode.Vanilla
			},

			new WYDSwitchOption<SpawnLocationMode>()
			{
				name = "Ideal Spawns",
				value = SpawnLocationMode.Ideal
			},

			new WYDSwitchOption<SpawnLocationMode>()
			{
				name = "All Spawns",
				value = SpawnLocationMode.All
			}
};

		public static LobbySettingsSerializer serializer = new LobbySettingsSerializer();
	}

	public class LobbySettingsSerializer
	{
		public byte[] Serialize(LobbySettings lobby)
		{
			SerializationHelper writer = new SerializationHelper();

			writer.WriteBooleanBlock(new bool[]
			{
				lobby.allMustDie,
				lobby.allowSpectating,
				lobby.disablePets,
				lobby.doorSounds,
				lobby.ventSounds,
				lobby.modernGrabbing,
				lobby.cheats
			});

			writer.Write((byte)lobby.shufflePlayers);
			writer.Write(lobby.GameMode);
			writer.Write(lobby.mapName);
			writer.Write(lobby.relay);
			writer.Write(lobby.roundNumber);
			writer.Write((byte)lobby.spawnMode);

			return writer.data.ToArray();
		}

		public LobbySettings Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			LobbySettings lobby = new LobbySettings();

			bool[] booleanBlock = reader.ReadBooleanBlock();

			lobby.allMustDie = booleanBlock[0];
			lobby.allowSpectating = booleanBlock[1];
			lobby.disablePets = booleanBlock[2];
			lobby.doorSounds = booleanBlock[3];
			lobby.ventSounds = booleanBlock[4];
			lobby.modernGrabbing = booleanBlock[5];
			lobby.cheats = booleanBlock[6];

			lobby.shufflePlayers = (ShufflePlayersMode)reader.Read();
			lobby.GameMode = reader.Read();
			lobby.mapName = reader.ReadString();
			lobby.relay = reader.ReadString();
			lobby.roundNumber = reader.ReadUshort();
			lobby.spawnMode = (SpawnLocationMode)reader.Read();

			return lobby;
		}

		public LobbySettings MakeUnique(LobbySettings toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
