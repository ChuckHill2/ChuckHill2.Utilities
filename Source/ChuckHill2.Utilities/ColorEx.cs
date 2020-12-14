//#define USE_COLORMINE  // install the ColorMine nuget package, first
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ChuckHill2.Utilities
{
    public static class ColorEx
    {
        // Nice perceptual color ordering from https://en.wikipedia.org/wiki/Web_colors.
        // Using  Enum.GetNames(typeof(KnownColor)).Select(s => Color.FromName(s)) and sorting by HSL gives accurate but perceptually strange results, so we hardcode it here.
        private const string WebColors = "Black|DarkSlateGray|DimGray|SlateGray|Gray|LightSlateGray|DarkGray|Silver|LightGray|Gainsboro|WhiteSmoke|White|Transparent|MistyRose|AntiqueWhite|Linen|Beige|LavenderBlush|OldLace|AliceBlue|Seashell|GhostWhite|Honeydew|FloralWhite|Azure|MintCream|Snow|Ivory|DarkRed|Red|Firebrick|Crimson|IndianRed|LightCoral|Salmon|DarkSalmon|LightSalmon|OrangeRed|Tomato|DarkOrange|Coral|Orange|DarkKhaki|Gold|Khaki|PeachPuff|Yellow|PaleGoldenrod|Moccasin|PapayaWhip|LightGoldenrodYellow|LemonChiffon|LightYellow|Maroon|Brown|SaddleBrown|Sienna|Chocolate|DarkGoldenrod|Peru|RosyBrown|Goldenrod|SandyBrown|Tan|Burlywood|Wheat|NavajoWhite|Bisque|BlanchedAlmond|Cornsilk|DarkGreen|Green|DarkOliveGreen|ForestGreen|SeaGreen|Olive|OliveDrab|MediumSeaGreen|LimeGreen|Lime|SpringGreen|MediumSpringGreen|DarkSeaGreen|MediumAquamarine|YellowGreen|LawnGreen|Chartreuse|LightGreen|GreenYellow|PaleGreen|Teal|DarkCyan|LightSeaGreen|CadetBlue|DarkTurquoise|MediumTurquoise|Turquoise|Aqua|Cyan|Aquamarine|PaleTurquoise|LightCyan|Navy|DarkBlue|MediumBlue|Blue|MidnightBlue|RoyalBlue|SteelBlue|DodgerBlue|DeepSkyBlue|CornflowerBlue|SkyBlue|LightSkyBlue|LightSteelBlue|LightBlue|PowderBlue|Indigo|Purple|DarkMagenta|DarkViolet|DarkSlateBlue|BlueViolet|DarkOrchid|Fuchsia|Magenta|SlateBlue|MediumSlateBlue|MediumOrchid|MediumPurple|Orchid|Violet|Plum|Thistle|Lavender|MediumVioletRed|DeepPink|PaleVioletRed|HotPink|LightPink|Pink";
        // SystemColors are just sorted alphabetically. There is no advantage to sorting by color as the system colors may change.
        private const string SystemColors = "ActiveBorder|ActiveCaption|ActiveCaptionText|AppWorkspace|ButtonFace|ButtonHighlight|ButtonShadow|Control|ControlDark|ControlDarkDark|ControlLight|ControlLightLight|ControlText|Desktop|GradientActiveCaption|GradientInactiveCaption|GrayText|Highlight|HighlightText|HotTrack|InactiveBorder|InactiveCaption|InactiveCaptionText|Info|InfoText|Menu|MenuBar|MenuHighlight|MenuText|ScrollBar|Window|WindowFrame|WindowText";
        private const short StateKnownColorValid = 1;
        private const short StateARGBValueValid = 2;
        private const short StateNameValid = 8;
        private static readonly ConstructorInfo ciColor = typeof(Color).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(long), typeof(short), typeof(string), typeof(KnownColor) }, null);
        private static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(uint)((int)red << 16 | (int)green << 8 | (int)blue | (int)alpha << 24) & (long)uint.MaxValue;

        /// <summary>
        /// Assign a custom name to a color.
        /// Where color.IsNamedColor == true, color.IsKnownColor == false, and color.Name == [your name].
        /// </summary>
        /// <param name="c">Source unnamed Color to assign name to.</param>
        /// <param name="name">Name to assign to provided color.</param>
        /// <returns>New named color</returns>
        public static Color MakeNamed(this Color c, string name) => (Color)ciColor.Invoke(new object[] { MakeArgb(c.A, c.R, c.G, c.B), (short)(StateARGBValueValid | StateNameValid), name, 0 });

        /// <summary>
        ///  Retrieve or create a name for the specified color.
        /// </summary>
        /// <param name="color">Color that may or may not have a associated name.</param>
        /// <returns>The color's name or a name created based upon the ARGB values.</returns>
        public static string GetName(this Color color)
        {
            if (color.IsEmpty) return "Empty";
            if (color.A == 0) return Color.Transparent.Name;
            if (color.IsNamedColor) return color.Name;
            var result = KnownColors.FirstOrDefault(c => c.R == color.R && c.G == color.G && c.B == color.B);
            return !result.IsEmpty ? (color.A != 255 ? $"({color.A},{result.Name})" : result.Name) : (color.A!=255 ? $"({color.A},{color.R},{color.G},{color.B})" : $"({color.R},{color.G},{color.B})");
        }

        private class ColorItem
        {
            // Pre-computed LAB colorspace values for NearestKnownColor()
            public readonly Color Color;
            public readonly double L, A, B;
            public ColorItem(string colorName)
            {
                Color = Color.FromName(colorName);
                #if USE_COLORMINE
                   var lab = new ColorMine.ColorSpaces.Rgb { R = Color.R, G = Color.G, B = Color.B }.To<ColorMine.ColorSpaces.Lab>();
                    L = lab.L;
                    A = lab.A;
                    B = lab.B;
                #else
                    var lab = RGBtoLab(this.Color);
                    L = lab[0];
                    A = lab[1];
                    B = lab[2];
                #endif
            }
        }

        private static List<ColorItem> __knownColorItems;  //load-on-demand.
        private static List<ColorItem> KnownColorItems
        {
            get
            {
                if (__knownColorItems == null) __knownColorItems = WebColors.Split('|').Concat(SystemColors.Split('|')).Select(s => new ColorItem(s)).ToList();
                return __knownColorItems;
            }
        }

        private static Color[] __knownColors;  //load-on-demand.
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

        /// <summary>
        /// Given a specified color, get the nearest known web or system color. Transparency is ignored.
        /// </summary>
        /// <param name="color">Color to match</param>
        /// <returns>The known color that most closely resembles the specified color</returns>
        public static Color NearestKnownColor(this Color color)
        {
            // For finding nearest known perceptual color, using the LAB color space gives much better results than weighted HSL.
            double L, A, B;

            #if USE_COLORMINE
                var lab = new ColorMine.ColorSpaces.Rgb { R = color.R, G = color.G, B = color.B }.To<ColorMine.ColorSpaces.Lab>();
                L = lab.L;
                A = lab.A;
                B = lab.B;
            #else
                var lab = RGBtoLab(color);
                L = lab[0];
                A = lab[1];
                B = lab[2];
            #endif

            double distance = double.MaxValue;
            int index = 0;
            for (int i = 0; i < KnownColorItems.Count; i++)
            {
                var item = KnownColorItems[i];
                var dL = Math.Abs(item.L - L);
                var dA = Math.Abs(item.A - A);
                var dB = Math.Abs(item.B - B);
                var r = dL*dL + dA*dA + dB*dB; //(L1-L2)^2 + (a1-a2)^2 + (b1-b2)^2
                if (r < distance)
                {
                    distance = r;
                    index = i;
                }
            }

            return KnownColorItems[index].Color;
        }

        /// <summary>
        /// For debugging only.
        /// Dump known colors into bin\debug\NamedColors.txt that may be in turn, loaded into Excel for inspection and validation.
        /// Use the custom Excel macro function 'myRGB()' to fill-in the 'Color' column.
        /// Validate values against  http://colormine.org/
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void DumpColors()
        {
            Func<Color, Color, bool> Equals = (c1, c2) => c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;

            var fn = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "NamedColors.txt");
            using (var sw = new System.IO.StreamWriter(fn))
            {
                sw.WriteLine("Color\tName\tHex (RGB)" +
                    "\tRed (RGB)\tGreen (RGB)\tBlue (RGB)" +
                    "\tL (LAB)\tA (LAB)\tB (LAB)" +
                    "\tHue (HSL)\tSat (HSL)\tLum (HSL)" +
                    "\tHue (HSV)\tSat (HSV)\tValue (HSV)" +
                    "\tRound Trip (HSL)\tRed (HSL)\tGreen (HSL)\tBlue (HSL)" +
                    "\tRound Trip (HSV)\tRed (HSV)\tGreen (HSV)\tBlue (HSV)"
                    );

                foreach (var c in ColorEx.KnownColors)
                {
                    double L, A, B;
                    #if USE_COLORMINE
                        var lab = new ColorMine.ColorSpaces.Rgb { R = c.R, G = c.G, B = c.B }.To<ColorMine.ColorSpaces.Lab>();
                        L = lab.L;
                        A = lab.A;
                        B = lab.B;
                    #else
                        var lab = RGBtoLab(c);
                        L = lab[0];
                        A = lab[1];
                        B = lab[2];
                    #endif

                    HSLColor hsl = c;
                    HSVColor hsv = c;
                    Color hsl2c = hsl;
                    Color hsv2c = hsv;

                    sw.WriteLine($"\t{c.Name}\t#{ColorEx.MakeArgb(0, c.R, c.G, c.B):X6}" +
                        $"\t{c.R}\t{c.G}\t{c.B}" +
                        $"\t{L}\t{A}\t{B}" +
                        $"\t{hsl.Hue}\t{hsl.Saturation}\t{hsl.Luminosity}" +
                        $"\t{hsv.Hue}\t{hsv.Saturation}\t{hsv.Value}" +
                        $"\t{Equals(hsl2c, c)}\t{hsl2c.R}\t{hsl2c.G}\t{hsl2c.B}" +
                        $"\t{Equals(hsv2c, c)}\t{hsv2c.R}\t{hsv2c.G}\t{hsv2c.B}"
                        );
                }
            }
        }

        #region private static double[] RGBtoLab(Color c)
        // This is not that accurate, but it is good enough when comparing apples to apples in finding the nearest color. Much better than weighted HSL.
        // But if you want accuracy, install the ColorMine nuget package and uncomment '#define USE_COLORMINE', at top of this file.
        // Copied (mostly) from https://patrickwu.space/2016/06/12/csharp-color/#rgb2lab

        /// <summary>
        /// Converts RGB to CIELab.
        /// </summary>
        private static double[] RGBtoLab(Color c)
        {
            var xyz = RGBtoXYZ(c.R, c.G, c.B);
            return XYZtoLab(xyz[0], xyz[1], xyz[2]);
        }

        /// <summary>
        /// Converts RGB to CIE XYZ (CIE 1931 color space)
        /// </summary>
        private static double[] RGBtoXYZ(int red, int green, int blue)
        {
            // normalize red, green, blue values
            double rLinear = (double)red / 255.0;
            double gLinear = (double)green / 255.0;
            double bLinear = (double)blue / 255.0;

            // convert to a sRGB form
            double r = (rLinear > 0.04045) ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = (gLinear > 0.04045) ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = (bLinear > 0.04045) ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

            // converts
            return new double[]
            {
                (r * 0.4124 + g * 0.3576 + b * 0.1805),
                (r * 0.2126 + g * 0.7152 + b * 0.0722),
                (r * 0.0193 + g * 0.1192 + b * 0.9505)
            };
        }

        /// <summary>
        /// Converts CIEXYZ to CIELab.
        /// </summary>
        private static double[] XYZtoLab(double x, double y, double z)
        {
            var D65 = new { X = 0.9505, Y = 1.0, Z = 1.0890 };
            return new double[]
            {
                116.0 * Fxyz(y / D65.Y) - 16,
                500.0 * (Fxyz(x / D65.X) - Fxyz(y / D65.Y)),
                200.0 * (Fxyz(y / D65.Y) - Fxyz(z / D65.Z))
            };
        }

        private static double Fxyz(double t) => ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
        #endregion
    }
}
