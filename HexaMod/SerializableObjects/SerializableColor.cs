using System;
using System.Linq;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("SerializableColor", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	public class SerializableColor
	{
		public byte r;
		public byte g;
		public byte b;

		public SerializableColor(Color color)
		{
			r = (byte)(color.r * byte.MaxValue);
			g = (byte)(color.g * byte.MaxValue);
			b = (byte)(color.b * byte.MaxValue);
		}

		public SerializableColor()
		{

		}

		public Color toColor()
		{
			return new Color(
				r / (float)byte.MaxValue,
				g / (float)byte.MaxValue,
				b / (float)byte.MaxValue
			);
		}

		public static SerializableColorSerializer serializer = new SerializableColorSerializer();
	}

	public class SerializableColorSerializer
	{
		public byte[] Serialize(SerializableColor color)
		{
			SerializationHelper writer = new SerializationHelper();

			writer.Write(color.r);
			writer.Write(color.g);
			writer.Write(color.b);

			return writer.data.ToArray();
		}

		public SerializableColor Deserialize(byte[] serializedBytes)
		{
			SerializationHelper reader = new SerializationHelper()
			{
				data = serializedBytes.ToList()
			};

			SerializableColor color = new SerializableColor();

			color.r = reader.Read();
			color.g = reader.Read();
			color.b = reader.Read();

			return color;
		}

		public SerializableColor MakeUnique(SerializableColor toCopy)
		{
			return Deserialize(Serialize(toCopy));
		}
	}
}
