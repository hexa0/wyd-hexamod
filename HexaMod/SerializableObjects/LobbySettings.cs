using System;
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

	// [XmlRoot("LobbySettings", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	[Serializable]
	public class LobbySettings
	{
		public bool allMustDie = true; // TODO: make this a percentage for more customization, needs a UI component first though
		public ShufflePlayersMode shufflePlayers = ShufflePlayersMode.Alternate;
		public bool disablePets = true;
		public bool doorSounds = true;
		public bool modernGrabbing = true;
		public bool cheats = true;
		public byte GameMode = 0; // Todo: switching game modes in the lobby & quit to lobby instead of the menu button
		public string mapName = "Default";
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

		public static ClassSerializer<LobbySettings> serializer = new ClassSerializer<LobbySettings>();
	}
}
