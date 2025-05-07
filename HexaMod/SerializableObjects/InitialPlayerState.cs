using System;
using HexaMod.Util;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("InitialPlayerState", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	[Serializable]
	public class InitialPlayerState
	{
		public string username;
		public ulong clientId;
		public SerializableColor shirtColor;
		public SerializableColor skinColor;
		public string characterModel;

		public static ClassSerializer<InitialPlayerState> serializer = new ClassSerializer<InitialPlayerState>();
	}
}
