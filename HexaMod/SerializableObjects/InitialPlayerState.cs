using System;
using System.Linq;
using HexaMod.Util;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("InitialPlayerState", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class InitialPlayerState
	{
		public string username;
		public ulong clientId;
		public SerializableColor shirtColor;
		public SerializableColor skinColor;
		public string characterModel;

		public static InitialPlayerStateSerializer serializer = new InitialPlayerStateSerializer();
	}

	public class InitialPlayerStateSerializer
	{
		public byte[] Serialize(InitialPlayerState state)
		{
			SerializationHelper writer = new SerializationHelper();

			writer.Write(state.username);
			writer.Write(state.clientId);
			writer.WriteSizedObject(SerializableColor.serializer.Serialize(state.shirtColor));
			writer.WriteSizedObject(SerializableColor.serializer.Serialize(state.skinColor));
			writer.Write(state.characterModel);

			return writer.data.ToArray();
		}

		public InitialPlayerState Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			InitialPlayerState state = new InitialPlayerState();

			state.username = reader.ReadString();
			state.clientId = reader.ReadUlong();
			state.shirtColor = SerializableColor.serializer.Deserialize(reader.ReadSizedObject());
			state.skinColor = SerializableColor.serializer.Deserialize(reader.ReadSizedObject());
			state.characterModel = reader.ReadString();

			return state;
		}

		public InitialPlayerState MakeUnique(InitialPlayerState toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
