using System.Linq;
using HexaMod.Util;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("InitialPlayerState", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class InitialPlayerState
	{
		public SerializableColor shirtColor;
		public SerializableColor skinColor;
		public string characterModel;
		public string shirtMaterial;

		public static InitialPlayerStateSerializer serializer = new InitialPlayerStateSerializer();
	}

	public class InitialPlayerStateSerializer
	{
		public byte[] Serialize(InitialPlayerState state)
		{
			SerializationHelper writer = new SerializationHelper();

			writer.WriteSizedObject(SerializableColor.serializer.Serialize(state.shirtColor));
			writer.WriteSizedObject(SerializableColor.serializer.Serialize(state.skinColor));
			writer.Write(state.characterModel);
			writer.Write(state.shirtMaterial);

			return writer.data.ToArray();
		}

		public InitialPlayerState Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			InitialPlayerState state = new InitialPlayerState();

			state.shirtColor = SerializableColor.serializer.Deserialize(reader.ReadSizedObject());
			state.skinColor = SerializableColor.serializer.Deserialize(reader.ReadSizedObject());
			state.characterModel = reader.ReadString();
			state.shirtMaterial = reader.ReadString();

			return state;
		}

		public InitialPlayerState MakeUnique(InitialPlayerState toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
