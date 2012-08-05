using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Reflection;

namespace JayDev.Notemaker.Common
{
    static class ColorExtensions
    {
        //Value: #FF3F3F3F
        public static Color ApplicationDefaultTextColour = Color.FromRgb(63, 63, 63);

        public static string GetColorName(this Color color)
        {
            if (color == ApplicationDefaultTextColour)
            {
                return "Notemaker default";
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
