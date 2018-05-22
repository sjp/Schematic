using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using System.Data;
using Dapper;
using System.Drawing;
using System.Globalization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SchemaSpy
{

    internal static class ColorTranslator
    {
        public static Color FromHex(string hex)
        {
            if (hex.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(hex));

            FromHex(hex, out var a, out var r, out var g, out var b);

            return Color.FromArgb(a, r, g, b);
        }

        public static string ToRgbHex(this Color color)
        {
            var r = color.R.ToString("X2");
            var g = color.G.ToString("X2");
            var b = color.B.ToString("X2");

            return string.Concat("#", r, g, b);
        }

        public static string ToArgbHex(this Color color)
        {
            var a = color.A.ToString("X2");
            var r = color.R.ToString("X2");
            var g = color.G.ToString("X2");
            var b = color.B.ToString("X2");

            return string.Concat("#", a, r, g, b);
        }

        private static void FromHex(string hex, out byte a, out byte r, out byte g, out byte b)
        {
            hex = ToRgbaHex(hex);
            if (hex == null || !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var packedValue))
                throw new ArgumentException("Hexadecimal string is not in the correct format.", nameof(hex));

            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
            a = Convert.ToByte(hex.Substring(6, 2), 16);
        }

        private static string ToRgbaHex(string hex)
        {
            hex = hex.StartsWith("#") ? hex.Substring(1) : hex;

            if (hex.Length == 8)
                return hex;

            if (hex.Length == 6)
                return hex + "FF";

            if (hex.Length < 3 || hex.Length > 4)
                return null;

            //Handle values like #3B2
            var red = hex[0].ToString();
            var green = hex[1].ToString();
            var blue = hex[2].ToString();
            var alpha = hex.Length == 3 ? "F" : hex[3].ToString();

            return string.Concat(red, red, green, green, blue, blue, alpha, alpha);
        }
    }
}
