using System.Globalization;
using UnityEngine;

namespace HexaMod.Util
{
    public class HexToColor
    {
        public static Color GetColorFromHex(string hex)
        {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            int r = int.Parse(hex.Substring(0, 2), NumberStyles.AllowHexSpecifier);
            int g = int.Parse(hex.Substring(2, 2), NumberStyles.AllowHexSpecifier);
            int b = int.Parse(hex.Substring(4, 2), NumberStyles.AllowHexSpecifier);

            return new Color(r / 255f, g / 255f, b / 255f);
        }
    }
}
