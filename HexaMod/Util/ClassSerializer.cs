using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HexaMod.Util
{
	public class ClassSerializer<Type>
	{
		public byte[] Serialize(Type toSerialize)
		{
			BinaryFormatter serializer = new BinaryFormatter();
			using (MemoryStream memoryStream = new MemoryStream())
			{
				serializer.Serialize(memoryStream, toSerialize);
				byte[] serializedBytes = memoryStream.ToArray();

				return serializedBytes;
			}
		}

		public Type Deserialize(byte[] serializedBytes)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter serializer = new BinaryFormatter();

				memoryStream.Write(serializedBytes, 0, serializedBytes.Length);
				memoryStream.Seek(0, SeekOrigin.Begin);

				return (Type)serializer.Deserialize(memoryStream);
			}
		}

		public Type MakeUnique(Type toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
