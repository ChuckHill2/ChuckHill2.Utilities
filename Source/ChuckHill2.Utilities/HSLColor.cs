//--------------------------------------------------------------------------
// <copyright file="HSLColor.cs" company="Omnicell Inc.">
//     Copyright (c) Omnicell Inc. All rights reserved.
//     Reproduction or transmission in whole or in part, in 
//     any form or by any means, electronic, mechanical, or otherwise, is 
//     prohibited without the prior written consent of the copyright owner.
// </copyright>
// <author>Chuck Hill</author>
// <summary>
// </summary>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Implicitly converts a System.Drawing.Color object to/from a HSLColor object.
    /// Works identically in operation to the System ColorDialog. (i.e. HLS values range from 0 to 240)
    /// See: http://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/
    /// </summary>
    public class HSLColor
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double hue = 1.0;
        private double saturation = 1.0;
        private double luminosity = 1.0;
        private byte alpha = 255; //this is passthru when converting Color => HSLColor => Color

        private const double scale = 240.0;

        /// <summary>
        /// Range = 0.0 to 240.0
        /// Values less than 0.0 are automatically set to 0.0.
        /// Values greater than 240.0 are automatically set to 240.0.
        /// </summary>
        public double Hue
        {
            get { return hue * scale; }
            set { hue = CheckRange(value / scale); }
        }
        /// <summary>
        /// Range = 0.0 to 240.0
        /// Values less than 0.0 are automatically set to 0.0.
        /// Values greater than 240.0 are automatically set to 240.0.
        /// </summary>
        public double Saturation
        {
            get { return saturation * scale; }
            set { saturation = CheckRange(value / scale); }
        }
        /// <summary>
        /// Range = 0.0 to 240.0
        /// Values less than 0.0 are automatically set to 0.0.
        /// Values greater than 240.0 are automatically set to 240.0.
        /// Same as HSB Brightness.
        /// </summary>
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSLColor hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor.luminosity != 0)
            {
                if (hslColor.saturation == 0)
                    r = g = b = hslColor.luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.hue);
                    b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb(hslColor.alpha, (int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }

        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }

        private static double GetTemp2(HSLColor hslColor)
        {
            double temp2;
            if (hslColor.luminosity < 0.5)  //<=??
                temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
            else
                temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
            return temp2;
        }

        public static implicit operator HSLColor(Color color)
        {
            HSLColor hslColor = new HSLColor();
            hslColor.alpha = color.A;
            hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.luminosity = color.GetBrightness();
            hslColor.saturation = color.GetSaturation();
            return hslColor;
        }
        #endregion

        private void SetRGB(int alpha, int red, int green, int blue)
        {
            HSLColor hslColor = (HSLColor)Color.FromArgb(alpha, red, green, blue);
            this.alpha = hslColor.alpha;
            this.hue = hslColor.hue;
            this.saturation = hslColor.saturation;
            this.luminosity = hslColor.luminosity;
        }

        public HSLColor() { }
        public HSLColor(Color color)
        {
            SetRGB(color.A, color.R, color.G, color.B);
        }
        public HSLColor(int alpha, int red, int green, int blue)
        {
            SetRGB(alpha, red, green, blue);
        }
        public HSLColor(byte alpha, double hue, double saturation, double luminosity)
        {
            this.alpha = alpha;
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

        private class ColorItem
        {
            public readonly Color color;
            public readonly HSLColor hsl;
            public ColorItem(string colorName)
            {
                color = Color.FromName(colorName);
                hsl = new HSLColor(color);
            }
        }

        private static List<ColorItem> __knownColors;
        private static List<ColorItem> KnownColors
        {
            get
            {
                if (__knownColors == null) __knownColors = Enum.GetNames(typeof(KnownColor))
                 .Select(s => new ColorItem(s))
                 .OrderBy(ci => ci.color.IsSystemColor)
                 .ThenBy(ci => ci.color.IsSystemColor ? ci.color.Name : "0")
                 .ThenBy(ci => ci.hsl.Hue)
                 .ThenBy(ci => ci.hsl.Saturation)
                 .ThenBy(ci => ci.hsl.Luminosity)
                 .ToList();

                return __knownColors;
            }
        }

        public static Color[] GetKnownColors() => KnownColors.Select(ci => ci.color).ToArray();

        public Color NearestKnownColor()
        {
            // adjust these values to place more or less importance on
            // the differences between HSV components of the colors
            const double weightHue = 0.8;
            const double weightSaturation = 0.1;
            const double weightLuminosity = 0.1;

            double residual = double.MaxValue;
            int index = 0;
            for (int i = 0; i < KnownColors.Count; i++)
            {
                var hsl = KnownColors[i].hsl;
                var dH = Math.Abs(hsl.Hue - this.Hue);
                var dS = Math.Abs(hsl.Saturation - this.Saturation);
                var dL = Math.Abs(hsl.Luminosity - this.Luminosity);
                var r = Math.Pow(weightHue * Math.Pow(dH, 2) + weightSaturation * Math.Pow(dS, 2) + weightLuminosity + Math.Pow(dL, 2), 0.5);
                if (r < residual)
                {
                    residual = r;
                    index = i;
                }
            }

            return KnownColors[index].color;
        }

        private static readonly ConstructorInfo ciColor = typeof(Color).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(long), typeof(short), typeof(string), typeof(KnownColor) }, null);
        private static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(uint)((int)red << 16 | (int)green << 8 | (int)blue | (int)alpha << 24) & (long)uint.MaxValue;
        /// <summary>
        /// Created a 'Named' color with an alpha value with something other than 255 appended to the name. (e.g. 'Red64').
        /// If RGB value is not a known color, the nearest one is found.
        /// Where:
        ///    color.IsKnownColor == false
        ///    color.IsSystemColor == false
        ///    color.IsNamedColor == true
        /// If the provided color is not a named color, the nearest named color is returned including the optional alpha transparency.
        /// If alpha==255 (the default), a new named color is not created and is returned as-is.
        /// At a minimum, it is a good way to convert a random RGB color to it's nearest named equivalant.
        /// </summary>
        /// <param name="c">Color to convert</param>
        /// <param name="alpha">The transparency value to give the new color.</param>
        /// <returns></returns>
        public static Color MakeNamedColor(Color c, byte alpha = 255)
        {
            //const short StateKnownColorValid = 1;
            const short StateARGBValueValid = 2;
            const short StateNameValid = 8;

            if (!c.IsNamedColor)
            {
                c = ((HSLColor)c).NearestKnownColor();
            }

            if (alpha == 255) return c;

            return (Color)ciColor.Invoke(new object[] { MakeArgb(alpha, c.R, c.G, c.B), (short)(StateARGBValueValid | StateNameValid), c.Name + alpha.ToString(), 0 });
        }

        /// <summary>
        /// Strip meta-properties form color object (e.g. Name, IsNamedColor=false, IsKnownColor=false, IsSystemColor=false)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color MakeUnnamedColor(Color c) => Color.FromArgb(c.A, c.R, c.G, c.B);

    }
}
