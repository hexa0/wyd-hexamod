using System.Linq;
using HexaMod.API.Util.Serialization;

namespace HexaMod.Scripts.Multiplayer.Lobby
{
	// [XmlRoot("MatchStartObject", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class MatchStartObject
	{
		public static MatchStartObjectSerializer serializer = new MatchStartObjectSerializer();
	}

	public class MatchStartObjectSerializer
	{
		public byte[] Serialize(MatchStartObject state)
		{
			SerializationHelper writer = new SerializationHelper();

			return writer.data.ToArray();
		}

		public MatchStartObject Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			MatchStartObject state = new MatchStartObject();

			return state;
		}

		public MatchStartObject MakeUnique(MatchStartObject toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
