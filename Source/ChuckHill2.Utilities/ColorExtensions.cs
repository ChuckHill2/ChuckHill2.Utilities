using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ChuckHill2.Utilities
{
    public static class ColorExtensions
    {
        const short StateKnownColorValid = 1;
        const short StateARGBValueValid = 2;
        const short StateNameValid = 8;
        private static readonly ConstructorInfo ciColor = typeof(Color).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(long), typeof(short), typeof(string), typeof(KnownColor) }, null);
        public static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(uint)((int)red << 16 | (int)green << 8 | (int)blue | (int)alpha << 24) & (long)uint.MaxValue;
        private static Color MakeNamed(Color c, string name) => (Color)ciColor.Invoke(new object[] { MakeArgb(c.A, c.R, c.G, c.B), (short)(StateARGBValueValid | StateNameValid), name, 0 });

        /// <summary>
        /// Find the named color that is nearest to this one, independent of transparency.  However the transparency is copied to the result.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color FindNearestNamed(this Color c)
        {
            byte alpha = c.A;
            if (!c.IsNamedColor) c = c.NearestKnownColor();
            if (alpha == 255) return c;
            return MakeNamed(Color.FromArgb(alpha, c), $"({alpha},{c.Name})");
        }

        /// <summary>
        /// Find name color that has the same RGB value, independent of transparency.  However the transparency is copied to the result.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color FindNamedExact(this Color color)
        {
            if (color.IsEmpty) return color;
            if (color.A == 0) return Color.Transparent;
            var result = KnownColors.FirstOrDefault(c => c.R == color.R && c.G == color.G && c.B == color.B);
            if (result.IsEmpty || color.A == 255) return result;
            return MakeNamed(result, $"({result.A},{result.Name})");
        }

        /// <summary>
        ///  Retrieve or create a name for the specified color.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetName(this Color color)
        {
            if (color.IsEmpty) return "Empty";
            if (color.A == 0) return Color.Transparent.Name;
            if (color.IsNamedColor) return color.Name;
            var result = KnownColors.FirstOrDefault(c => c.R == color.R && c.G == color.G && c.B == color.B);
            return result.IsEmpty ? "Empty" : color.A!=255 ? $"({color.A},{color.R},{color.G},{color.B})" : $"({color.R},{color.G},{color.B})";
        }

        private class ColorItem
        {
            //Pre-computed values for NearestKnownColor()
            public readonly Color Color;
            public readonly float Hue;
            public readonly float Saturation;
            public readonly float Luminosity;
            public ColorItem(string colorName)
            {
                Color = Color.FromName(colorName);
                Hue = Color.GetHue();
                Saturation = Color.GetSaturation();
                Luminosity = Color.GetBrightness();
            }
        }

        private static List<ColorItem> __knownColorItems;
        private static List<ColorItem> KnownColorItems
        {
            get
            {
                if (__knownColorItems == null) __knownColorItems = Enum.GetNames(typeof(KnownColor))
                 .Select(s => new ColorItem(s))
                 .OrderBy(ci => ci.Color.IsSystemColor)
                 .ThenBy(ci => ci.Color.IsSystemColor ? ci.Color.Name : "0")
                 .ThenBy(ci => ci.Hue)
                 .ThenBy(ci => ci.Saturation)
                 .ThenBy(ci => ci.Luminosity)
                 .ToList();

                return __knownColorItems;
            }
        }

        private static Color[] __knownColors = null;
        /// <summary>
        /// List of all web and system colors. Web colors are ordered by color similarity and then system colors are ordered alphabetically.
        /// </summary>
        public static Color[] KnownColors
        {
            get
            {
                if (__knownColors == null) __knownColors = KnownColorItems.Select(ci => ci.Color).ToArray();
                return __knownColors;
            }
        }

        public static Color NearestKnownColor(this Color color)
        {
            // adjust these values to place more or less importance on
            // the differences between HSL components of the colors
            const double weightHue = 0.8;
            const double weightSaturation = 0.1;
            const double weightLuminosity = 0.1;

            var hue = color.GetHue();
            var saturation = color.GetSaturation();
            var luminosity = color.GetBrightness();

            double distance = double.MaxValue;
            int index = 0;
            for (int i = 0; i < KnownColorItems.Count; i++)
            {
                var item = KnownColorItems[i];
                var dH = Math.Abs(item.Hue - hue);
                var dS = Math.Abs(item.Saturation - saturation);
                var dL = Math.Abs(item.Luminosity - luminosity);
                var r = Math.Pow(weightHue * Math.Pow(dH, 2) + weightSaturation * Math.Pow(dS, 2) + weightLuminosity + Math.Pow(dL, 2), 0.5);
                if (r < distance)
                {
                    distance = r;
                    index = i;
                }
            }

            return KnownColorItems[index].Color;
        }
    }
}
