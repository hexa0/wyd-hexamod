using System.Globalization;
using HexaMod;
using UnityEngine;

public static class ColorExtensions
{
	public static Color FromHex(this Color color, string hex)
	{
		if (hex.StartsWith("#"))
		{
			hex = hex.Substring(1);
		}

		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);

		color.r = r / 255f;
		color.g = g / 255f;
		color.b = b / 255f;
		color.a = 1f;

		return color;
	}
}