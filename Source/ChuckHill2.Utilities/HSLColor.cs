using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Implicitly converts a System.Drawing.Color object to/from a HSLColor object.
    /// See: http://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/
    /// </summary>
    public class HSLColor
    {
        /// <summary>Gets/Sets the alpha transparency component value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The alpha transparency component value of this <see cref="T:System.Drawing.Color" />. Alpha ranges from 0 through 255, where 0 is completely transparent and 255 is completely opaque.</returns>
        public byte Alpha { get; set; }

        private double __hue = 0.0;
        /// <summary>Gets the HSB/HSL/HSV hue value, in degrees, for this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The hue, in degrees, of this <see cref="T:System.Drawing.Color" />. The hue is measured in degrees, ranging from 0.0 through 360.0, in the HSB/HSL/HSV color spaces.</returns>
        public double Hue
        {
            get => __hue;
            set => __hue = CheckRange(value, 360);
        }

        private double __saturation = 1.0;
        /// <summary>Gets HSB/HSL saturation value for this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The saturation of this <see cref="T:System.Drawing.Color" />. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
        public double Saturation
        {
            get => __saturation;
            set => __saturation = CheckRange(value);
        }

        private double __luminosity = 1.0;
        /// <summary>The HSB/HSL brightness/luminosity value for this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The brightness/luminosity of this <see cref="T:System.Drawing.Color" />. The luminosity ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
        public double Luminosity
        {
            get => __luminosity;
            set => __luminosity = CheckRange(value);
        }

        private double CheckRange(double value, double maxValue = 1.0) => value < 0.0 ? 0.0 : value > maxValue ? maxValue : value;

        private static Color HSLToRGB(byte a, double h, double s, double l)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (s == 0) r = g = b = (byte)(l * 255);
            else
            {
                double v1, v2;
                double hue = h / 360;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)(255 * HSLToRGB_Hue(v1, v2, hue + (1.0f / 3)));
                g = (byte)(255 * HSLToRGB_Hue(v1, v2, hue));
                b = (byte)(255 * HSLToRGB_Hue(v1, v2, hue - (1.0f / 3)));
            }

            return Color.FromArgb(a,r,g,b);
        }
        private static double HSLToRGB_Hue(double v1, double v2, double vH)
        {
            if (vH < 0) vH += 1;
            if (vH > 1) vH -= 1;
            if ((6 * vH) < 1) return (v1 + (v2 - v1) * 6 * vH);
            if ((2 * vH) < 1) return v2;
            if ((3 * vH) < 2) return (v1 + (v2 - v1) * ((2.0f / 3) - vH) * 6);
            return v1;
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSLColor hsl) => HSLToRGB(hsl.Alpha, hsl.Hue, hsl.Saturation, hsl.Luminosity);
        public static implicit operator HSLColor(Color color) => new HSLColor(color);
        #endregion

        public HSLColor() { }
        public HSLColor(Color color)
        {
            this.Alpha = color.A;
            this.Hue = color.GetHue();
            this.Saturation = color.GetSaturation();
            this.Luminosity = color.GetBrightness();
        }
        public HSLColor(int alpha, int red, int green, int blue) : this(Color.FromArgb(alpha,red,green,blue)) { }
        public HSLColor(byte alpha, double hue, double saturation, double luminosity)
        {
            this.Alpha = alpha;
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }

        /// <summary>
        /// Get HSL values in Excel HSL format (e.g. 0-255)
        /// </summary>
        /// <returns>array of h,s,l values</returns>
        public int[] ToExcel()
        {
            return new[]
            {
                (int)(Hue / 360 * 255 + 0.5),
                (int)(Saturation * 255 + 0.5),
                (int)(Luminosity * 255 + 0.5)
            };
        }

        /// <summary>
        /// Get HSL values in native Win32 HSL format (e.g. 0-240)
        /// </summary>
        /// <returns>array of h,s,l values</returns>
        public int[] ToWin32()
        {
            return new[]
            {
                (int)(Hue / 360 * 240 + 0.5),
                (int)(Saturation * 240 + 0.5),
                (int)(Luminosity * 240 + 0.5)
            };
        }

        public override string ToString() => $"H: {Hue:#0.##} S: {Saturation:#0.##} L: {Luminosity:#0.##}";
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        {
            HSLColor other;
            if (obj is HSLColor) other = (HSLColor)obj;
            else if (obj is Color) other = new HSLColor((Color)obj);
            else return false;

            return this.Alpha == other.Alpha &&
                    (int)(this.Hue * 10000) == (int)(other.Hue * 10000) &&
                    (int)(this.Saturation * 10000) == (int)(other.Saturation * 10000) &&
                    (int)(this.Luminosity * 10000) == (int)(other.Luminosity * 10000);
        }
    }
}
