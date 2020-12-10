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
    public struct HSLColor
    {
        /// <summary>Gets/Sets the alpha transparency component value of this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The alpha transparency component value of this <see cref="T:System.Drawing.Color" />. Alpha ranges from 0 through 255, where 0 is completely transparent and 255 is completely opaque.</returns>
        public byte Alpha { get; set; }

        private double __hue;
        /// <summary>Gets the HSB/HSL/HSV hue value, in degrees, for this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The hue, in degrees, of this <see cref="T:System.Drawing.Color" />. The hue is measured in degrees, ranging from 0.0 through 360.0, in the HSB/HSL/HSV color spaces.</returns>
        public double Hue
        {
            get => __hue;
            set => __hue = CheckRange(value, 360);
        }

        private double __saturation;
        /// <summary>Gets HSB/HSL saturation value for this <see cref="T:System.Drawing.Color" /> structure.</summary>
        /// <returns>The saturation of this <see cref="T:System.Drawing.Color" />. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
        public double Saturation
        {
            get => __saturation;
            set => __saturation = CheckRange(value);
        }

        private double __luminosity;
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

            if (s == 0) r = g = b = (byte)(l * 255 + 0.5);
            else
            {
                double v1, v2;
                double hue = h / 360;

                v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
                v1 = 2 * l - v2;

                r = (byte)((255 * HSLToRGB_Hue(v1, v2, hue + (1.0 / 3))) + 0.5);
                g = (byte)((255 * HSLToRGB_Hue(v1, v2, hue)) + 0.5);
                b = (byte)((255 * HSLToRGB_Hue(v1, v2, hue - (1.0 / 3))) + 0.5);
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
        public static bool operator ==(HSLColor a, HSLColor b) => a.Equals(b);
        public static bool operator !=(HSLColor a, HSLColor b) => !a.Equals(b);
        #endregion

        /// <summary>
        /// Create a new HSL struct from a  <see cref="T:System.Drawing.Color" /> structure.
        /// </summary>
        /// <param name="color"></param>
        public HSLColor(Color color) : this()
        {
            __hue = 0;
            __saturation = 0;
            __luminosity = 0;

            Alpha = color.A;
            Hue = color.GetHue();
            Saturation = color.GetSaturation();
            Luminosity = color.GetBrightness();
        }

        /// <summary>
        /// Create a new HSL struct from RGB color values.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public HSLColor(int alpha, int red, int green, int blue) : this(Color.FromArgb(alpha,red,green,blue)) { }

        /// <summary>
        /// Create a new HSL struct from HSL color values..
        /// </summary>
        /// <param name="alpha">Alpha transparency (0-255)</param>
        /// <param name="hue">Hue (0.0-360.0)</param>
        /// <param name="saturation">Saturation (0.0-1.0)</param>
        /// <param name="luminosity">Luminosity (0.0-1.0)</param>
        public HSLColor(int alpha, double hue, double saturation, double luminosity)
        {
            __hue = 0;
            __saturation = 0;
            __luminosity = 0;

            Alpha = (byte)(alpha < 0 ? 0 : alpha > 255 ? 255 : alpha);
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

        /// <summary>
        /// Get HSL values in Excel HSL color scale (e.g. 0-255)
        /// </summary>
        /// <param name="a">out Alpha transparency (0-255)</param>
        /// <param name="h">out HLS Hue (0-255)</param>
        /// <param name="s">out HLS Saturation (0-255)</param>
        /// <param name="l">out HLS Luminosity (0-255)</param>
        public void ToExcelScale(out int a, out int h, out int s, out int l)
        {
            a = this.Alpha;
            h = (int)(Hue / 360.0 * 255.0 + 0.5);
            s = (int)(Saturation * 255.0 + 0.5);
            l = (int)(Luminosity * 255.0 + 0.5);
        }

        /// <summary>
        /// Create HSLColor from values in Excel color scale (e.g. 0-255)
        /// </summary>
        /// <param name="a">Alpha transparency (0-255)</param>
        /// <param name="h">HLS Hue (0-255)</param>
        /// <param name="s">HLS Saturation (0-255)</param>
        /// <param name="l">HLS Luminosity (0-255)</param>
        public static HSLColor FromExcelScale(int a, int h, int s, int l)
        {
            Func<int, byte> check = (i) => (byte)(i < 0 ? 0 : i > 255 ? 255 : i);
            return new HSLColor(check(a),  check(h) / 255.0 * 360.0,  check(s) / 255.0,  check(l) / 255.0);
        }

        /// <summary>
        /// Get HSL values in native Win32 HSL color scale (e.g. 0-240)
        /// </summary>
        /// <param name="a">out Alpha transparency (0-255)</param>
        /// <param name="h">out HLS Hue (0-240)</param>
        /// <param name="s">out HLS Saturation (0-240)</param>
        /// <param name="l">out HLS Luminosity (0-240)</param>
        public void ToWin32Scale(out int a, out int h, out int s, out int l)
        {
            a = this.Alpha;
            h = (int)(Hue / 360.0 * 240.0 + 0.5);
            s = (int)(Saturation * 240.0 + 0.5);
            l = (int)(Luminosity * 240.0 + 0.5);
        }

        /// <summary>
        /// Create HSLColor from values in native Win32 HSL color scale (e.g. 0-240)
        /// </summary>
        /// <param name="a">Alpha transparency (0-255)</param>
        /// <param name="h">HLS Hue (0-240)</param>
        /// <param name="s">HLS Saturation (0-240)</param>
        /// <param name="l">HLS Luminosity (0-240)</param>
        public static HSLColor FromWin32Scale(int a, int h, int s, int l)
        {
            Func<int, byte> check = (i) => (byte)(i < 0 ? 0 : i > 240 ? 240 : i);
            return new HSLColor((byte)(a < 0 ? 0 : a > 255 ? 255 : a),  check(h) / 240.0 * 360.0,  check(s) / 240.0,  check(l) / 240.0);
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
