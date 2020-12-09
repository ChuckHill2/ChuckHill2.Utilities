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
        private static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(uint)((int)red << 16 | (int)green << 8 | (int)blue | (int)alpha << 24) & (long)uint.MaxValue;
        private static Color MakeNamed(Color c, string name) => (Color)ciColor.Invoke(new object[] { MakeArgb(c.A, c.R, c.G, c.B), (short)(StateARGBValueValid | StateNameValid), name, 0 });

        /// <summary>
        /// Find the named color that is nearest to this one, independent of transparency.  However the transparency is copied to the result.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color FindNearestNamed(this Color c)
        {
            byte alpha = c.A;
            if (alpha == 0) return Color.Transparent;
            if (!c.IsNamedColor) c = c.NearestKnownColor();
            if (alpha == 255) return c;
            return Color.FromArgb(alpha,c);
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
            return Color.FromArgb(color.A, result);
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
            if (color.IsKnownColor) return color.Name;
            var result = KnownColors.FirstOrDefault(c => c.R == color.R && c.G == color.G && c.B == color.B);
            return !result.IsEmpty ? (color.A != 255 ? $"({color.A},{result.Name})" : result.Name) : (color.A!=255 ? $"({color.A},{color.R},{color.G},{color.B})" : $"({color.R},{color.G},{color.B})");
        }

        //Nice color ordering from https://en.wikipedia.org/wiki/Web_colors.
        //Using  Enum.GetNames(typeof(KnownColor)).Select(s => Color.FromName(s)) and sorting by HSL gives accurate but perceptually strange results.
        private const string WebColors = "Black|DarkSlateGray|DimGray|SlateGray|Gray|LightSlateGray|DarkGray|Silver|LightGray|Gainsboro|WhiteSmoke|White|Transparent|MistyRose|AntiqueWhite|Linen|Beige|LavenderBlush|OldLace|AliceBlue|Seashell|GhostWhite|Honeydew|FloralWhite|Azure|MintCream|Snow|Ivory|DarkRed|Red|Firebrick|Crimson|IndianRed|LightCoral|Salmon|DarkSalmon|LightSalmon|OrangeRed|Tomato|DarkOrange|Coral|Orange|DarkKhaki|Gold|Khaki|PeachPuff|Yellow|PaleGoldenrod|Moccasin|PapayaWhip|LightGoldenrodYellow|LemonChiffon|LightYellow|Maroon|Brown|SaddleBrown|Sienna|Chocolate|DarkGoldenrod|Peru|RosyBrown|Goldenrod|SandyBrown|Tan|Burlywood|Wheat|NavajoWhite|Bisque|BlanchedAlmond|Cornsilk|DarkGreen|Green|DarkOliveGreen|ForestGreen|SeaGreen|Olive|OliveDrab|MediumSeaGreen|LimeGreen|Lime|SpringGreen|MediumSpringGreen|DarkSeaGreen|MediumAquamarine|YellowGreen|LawnGreen|Chartreuse|LightGreen|GreenYellow|PaleGreen|Teal|DarkCyan|LightSeaGreen|CadetBlue|DarkTurquoise|MediumTurquoise|Turquoise|Aqua|Cyan|Aquamarine|PaleTurquoise|LightCyan|Navy|DarkBlue|MediumBlue|Blue|MidnightBlue|RoyalBlue|SteelBlue|DodgerBlue|DeepSkyBlue|CornflowerBlue|SkyBlue|LightSkyBlue|LightSteelBlue|LightBlue|PowderBlue|Indigo|Purple|DarkMagenta|DarkViolet|DarkSlateBlue|BlueViolet|DarkOrchid|Fuchsia|Magenta|SlateBlue|MediumSlateBlue|MediumOrchid|MediumPurple|Orchid|Violet|Plum|Thistle|Lavender|MediumVioletRed|DeepPink|PaleVioletRed|HotPink|LightPink|Pink";

        //SystemColors are just sorted alphabetically. There is no advantage to sorting by color as the system colors may change.
        private const string SystemColors = "ActiveBorder|ActiveCaption|ActiveCaptionText|AppWorkspace|ButtonFace|ButtonHighlight|ButtonShadow|Control|ControlDark|ControlDarkDark|ControlLight|ControlLightLight|ControlText|Desktop|GradientActiveCaption|GradientInactiveCaption|GrayText|Highlight|HighlightText|HotTrack|InactiveBorder|InactiveCaption|InactiveCaptionText|Info|InfoText|Menu|MenuBar|MenuHighlight|MenuText|ScrollBar|Window|WindowFrame|WindowText";

        private class ColorItem
        {
            //Pre-computed HSL values for NearestKnownColor()
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
                if (__knownColorItems == null) __knownColorItems = WebColors.Split('|').Concat(SystemColors.Split('|')).Select(s => new ColorItem(s)).ToList();
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
            // adjust these values to place more or less importance on the differences between HSL components of the colors
            const double weightHue = 1.0;
            const double weightSaturation = 1.0;
            const double weightLuminosity = 1.0;

            //const double weightHue = 0.8;
            //const double weightSaturation = 0.1;
            //const double weightLuminosity = 0.1;

            //const double weightHue = 0.475;
            //const double weightSaturation = 0.2875;
            //const double weightLuminosity = 0.2375;

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

        /// <summary>
        /// Dump known colors into a text file that may be loaded into Excel for inspection and validation.
        /// Use the custom macro function 'myRGB()' to fill-in the 'Color' column.
        /// Validate against  http://colormine.org/
        /// </summary>
        public static void DumpColors()
        {
            var fn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "NamedColors.txt");
            using (var sw = new System.IO.StreamWriter(fn))
            {
                sw.WriteLine("Color\tName\tHex (RGB)\tRed (RGB)\tGreen (RGB)\tBlue (RGB)\tHue (HSL)\tSat (HSL)\tLum (HSL)\tHue (HSV)\tSat (HSV)\tValue (HSV)");
                foreach (var c in ColorExtensions.KnownColors)
                {
                    var hsl = (HSLColor)c;
                    var hsv = (HSVColor)c;
                    sw.WriteLine($"\t{c.Name}\t#{ColorExtensions.MakeArgb(0, c.R, c.G, c.B):X6}\t{c.R}\t{c.G}\t{c.B}\t{hsl.Hue}\t{hsl.Saturation}\t{hsl.Luminosity}\t{hsv.Hue}\t{hsv.Saturation}\t{hsv.Value}");
                }
            }
        }
    }
}
