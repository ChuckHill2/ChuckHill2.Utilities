using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Implicitly converts a System.Drawing.Color object to/from a HSVColor object.
    /// See: https://stackoverflow.com/questions/1335426/is-there-a-built-in-c-net-system-api-for-hsv-to-rgb
    /// </summary>
    public struct HSVColor
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
        /// <summary>Gets HSV saturation value for this <see cref="T:System.Drawing.Color" /> structure. Not to be confused with HSL Saturation.</summary>
        /// <returns>The saturation of this <see cref="T:System.Drawing.Color" />. The saturation ranges from 0.0 through 1.0, where 0.0 is grayscale and 1.0 is the most saturated.</returns>
        public double Saturation
        {
            get => __saturation;
            set => __saturation = CheckRange(value);
        }

        private double __value;
        /// <summary>The HSV brightness value for this <see cref="T:System.Drawing.Color" /> structure. Not to be confused with HSL Luminosity/Lightness/Brightness.</summary>
        /// <returns>The brightness of this <see cref="T:System.Drawing.Color" />. The value ranges from 0.0 through 1.0, where 0.0 represents black and 1.0 represents white.</returns>
        public double Value
        {
            get => __value;
            set => __value = CheckRange(value);
        }

        private double CheckRange(double value, double maxValue = 1.0) => value < 0.0 ? 0.0 : value > maxValue ? maxValue : value;

        private static void RGBtoHSV(Color color, out byte alpha, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            alpha = color.A;
            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }
        private static Color HSVtoRGB(byte alpha, double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(alpha, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(alpha, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(alpha, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(alpha, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(alpha, t, p, v);
            else
                return Color.FromArgb(alpha, v, p, q);
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HSVColor hsv) => HSVtoRGB(hsv.Alpha, hsv.Hue, hsv.Saturation, hsv.Value);
        public static implicit operator HSVColor(Color color) => new HSVColor(color);
        public static bool operator ==(HSVColor a, HSVColor b) => a.Equals(b);
        public static bool operator !=(HSVColor a, HSVColor b) => !a.Equals(b);
        #endregion

        public HSVColor(Color color)
        {
            __hue = 0;
            __saturation = 0;
            __value = 0;

            RGBtoHSV(color, out byte alpha, out double hue, out double saturation, out double value);
            this.Alpha = alpha;
            this.Hue = hue;
            this.Saturation = saturation;
            this.Value = value;
        }
        public HSVColor(int alpha, int red, int green, int blue) : this(Color.FromArgb(alpha, red, green, blue)) { }
        public HSVColor(byte alpha, double hue, double saturation, double value)
        {
            __hue = 0;
            __saturation = 0;
            __value = 0;

            this.Alpha = alpha;
            this.Hue = hue;
            this.Saturation = saturation;
            this.Value = value;
        }

        public override string ToString() => $"H: {Hue:#0.##} S: {Saturation:#0.##} V: {Value:#0.##}";
        public override int GetHashCode() => base.GetHashCode();
        public override bool Equals(object obj)
        {
            HSVColor other;
            if (obj is HSVColor) other = (HSVColor)obj;
            else if (obj is Color) other = new HSVColor((Color)obj);
            else return false;

            return this.Alpha == other.Alpha &&
                    (int)(this.Hue * 10000) == (int)(other.Hue * 10000) &&
                    (int)(this.Saturation * 10000) == (int)(other.Saturation * 10000) &&
                    (int)(this.Value * 10000) == (int)(other.Value * 10000);
        }
    }
}
