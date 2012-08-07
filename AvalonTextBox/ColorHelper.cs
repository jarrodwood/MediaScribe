using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Reflection;

namespace AvalonTextBox
{
    static class ColorHelper
    {
        //Value: #FF3F3F3F
        public static Color ApplicationDefaultTextColour = Color.FromRgb(63, 63, 63);
        const string DefaultString = "Default";

        public static string GetColorName(this Color color)
        {
            if (color == ApplicationDefaultTextColour)
            {
                return "MediaScribe default";
            }
            string match = _knownColors
                .Where(kvp => kvp.Value.Equals(color))
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(match))
            {
                //JDW NOTE: calling color.ToString() returns #ARGB, not #RGB -- since the user can't configure the alpha in my case, there's
                //no point returning it.
                StringBuilder builder = new StringBuilder();
                builder.Append("#");
                builder.Append(color.R.ToString("X").PadLeft(2, '0'));
                builder.Append(color.G.ToString("X").PadLeft(2, '0'));
                builder.Append(color.B.ToString("X").PadLeft(2, '0'));
                return builder.ToString();
            }
            else
            {
                return match;
            }
        }

        public static Color FromString(string input)
        {
            if (input == DefaultString)
            {
                return ColorHelper.ApplicationDefaultTextColour;
            }

            return (Color)ColorConverter.ConvertFromString(input);

            #region sigh, there's always an easier way... ^^
            //if (null == rgbOrArgbString)
            //    return new Color();

            //string hex = rgbOrArgbString[0] == '#' ? rgbOrArgbString.Substring(1) : rgbOrArgbString;
            //byte[] values = Enumerable.Range(0, hex.Length / 2).Select(x => Byte.Parse(hex.Substring(2 * x, 2), NumberStyles.HexNumber)).ToArray();

            //Color result;
            //if (values.Length == 3)
            //{
            //    result = new Color()
            //    {
            //        A = byte.MaxValue,
            //        R = values[0],
            //        G = values[1],
            //        B = values[2]
            //    };
            //}
            //else if (values.Length == 4)
            //{
            //    result = new Color()
            //    {
            //        A = values[0],
            //        R = values[1],
            //        G = values[2],
            //        B = values[3]
            //    };
            //}
            //else
            //{
            //    throw new Exception("Error - invalid number of hex values in color string");
            //}

            //return result;
            #endregion
        }

        static readonly Dictionary<string, Color> _knownColors = GetKnownColors();

        static Dictionary<string, Color> GetKnownColors()
        {
            var colorProperties = typeof(Colors).GetProperties(BindingFlags.Static | BindingFlags.Public);
            return colorProperties
                .ToDictionary(
                    p => p.Name,
                    p => (Color)p.GetValue(null, null));
        }
    }
}
