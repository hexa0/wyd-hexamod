using System.Linq;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("PlayerConnectedObject", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class PlayerConnectedObject
	{
		public bool isDad;

		public static PlayerConnectedObjectSerializer serializer = new PlayerConnectedObjectSerializer();
	}

	public class PlayerConnectedObjectSerializer
	{
		public byte[] Serialize(PlayerConnectedObject player)
		{
			SerializationHelper writer = new SerializationHelper();

			writer.WriteBooleanBlock(new bool[]
			{
				player.isDad
			});

			return writer.data.ToArray();
		}

		public PlayerConnectedObject Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			PlayerConnectedObject state = new PlayerConnectedObject();

			bool[] booleanBlock = reader.ReadBooleanBlock();

			state.isDad = booleanBlock[0];

			return state;
		}

		public PlayerConnectedObject MakeUnique(PlayerConnectedObject toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
