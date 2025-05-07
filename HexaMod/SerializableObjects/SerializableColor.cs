using System;
using HexaMod.Util;
using UnityEngine;

namespace HexaMod.SerializableObjects
{
	// [XmlRoot("SerializableColor", Namespace = "https://hexa.blueberry.coffee/hexa-mod/")]
	[Serializable]
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

		public Color toColor()
		{
			return new Color(
				r / (float)byte.MaxValue,
				g / (float)byte.MaxValue,
				b / (float)byte.MaxValue
			);
		}

		public static ClassSerializer<SerializableColor> serializer = new ClassSerializer<SerializableColor>();
	}
}
