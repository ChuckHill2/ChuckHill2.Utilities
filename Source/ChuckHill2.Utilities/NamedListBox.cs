using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Color selector ListBox  control containing 'Custom', 'Known', and 'System' colors. Each group has a dividing line for distinction between the three color sets.
    /// </summary>
    public class NamedColorListBox : ListBox
    {
        private int graphicWidth = 22;  //default pixel values at 96dpi
        private int pixel_2 = 2;
        private int pixel_4 = 4;

        public NamedColorListBox():base()
        {
            base.Margin = new Padding(0);
            base.Name = "NamedListBox";
            base.FormattingEnabled = true;
            base.DrawMode = DrawMode.OwnerDrawFixed;
            base.IntegralHeight = false;

            var pixelFactor = DpiScalingFactor() / 100.0;
            this.graphicWidth = ConvertToGivenDpiPixel(this.graphicWidth, pixelFactor);
            this.pixel_2 = ConvertToGivenDpiPixel(this.pixel_2, pixelFactor);
            this.pixel_4 = ConvertToGivenDpiPixel(this.pixel_4, pixelFactor);

            foreach(var c in ColorExtensions.KnownColors) base.Items.Add(new ColorItem(c.Name, c));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            base.ItemHeight = base.Font.Height;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            var ci = (ColorItem)base.Items[e.Index];

            Graphics g = e.Graphics;
            e.DrawBackground();

            var rc = new Rectangle(e.Bounds.X + this.pixel_2, e.Bounds.Y + this.pixel_2, this.graphicWidth, e.Bounds.Height - this.pixel_4 +1);

            if (ci.Color.A < 255) //add  background trasparency  checkerboard
            {
                using (var br = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.Transparent))
                    g.FillRectangle(br, rc);
            }

            using (var solidBrush = new SolidBrush(ci.Color))
                g.FillRectangle(solidBrush, rc);

            g.DrawRectangle(SystemPens.WindowText, rc.X, rc.Y, rc.Width-1, rc.Height-1);

            var x2 = e.Bounds.X + this.graphicWidth + this.pixel_4;
            var y2 = e.Bounds.Y;
            using (var br = new SolidBrush(e.ForeColor))
                g.DrawString(ci.Name, base.Font, br, x2, y2);

            if (e.Index > 0)
            {
                var ci2 = (ColorItem)base.Items[e.Index-1];
                if (ci.Color.IsSystemColor != ci2.Color.IsSystemColor ||
                    ci.Color.IsKnownColor != ci2.Color.IsKnownColor)
                {
                    g.DrawLine(SystemPens.WindowText, e.Bounds.Left + this.pixel_2, e.Bounds.Y, e.Bounds.Right - this.pixel_2, e.Bounds.Y);
                }
            }
        }

        /// <summary>
        /// Add custom color to list.
        /// Known colors will not be added as they already exist.
        /// </summary>
        /// <param name="c"></param>
        public void AddColor(Color c)
        {
            if (c.IsKnownColor || c.IsEmpty) return;
            if (base.Items.Cast<ColorItem>().FirstOrDefault(ci => Equals(c, ci.Color)) != null) return;
            var name = c.Name;
            if (!c.IsNamedColor)
            {
                var node = base.Items.Cast<ColorItem>().FirstOrDefault(ci => Equals(c, ci.Color, true));
                if (node != null) name = node.Color.Name + c.A.ToString();
                else name = c.A < 255 ? $"({c.A},{c.R},{c.G},{c.B})" : $"({c.R},{c.G},{c.B})";
            }
            base.Items.Insert(0, new ColorItem(name, c));  //Custom named colors go to top of list
        }

        /// <summary>
        /// Remove custom color from list.
        /// Known colors will not be removed.
        /// </summary>
        /// <param name="c"></param>
        public void RemoveColor(Color c)
        {
            if (c.IsKnownColor || c.IsEmpty) return;
            var item = base.Items.Cast<ColorItem>().TakeWhile(ci => !ci.Color.IsKnownColor).FirstOrDefault(ci => Equals(c, ci.Color));
            if (item == null) return;
            base.Items.Remove(item);
        }

        /// <summary>
        /// Get or Set the selected color.
        /// </summary>
        public Color Selected
        {
            get => base.SelectedItem is ColorItem ? ((ColorItem)base.SelectedItem).Color : Color.Empty;
            set
            {
                ColorItem item;
                if (value.IsKnownColor)
                    item = base.Items.Cast<ColorItem>().FirstOrDefault(ci => value.Name.Equals(ci.Color.Name));
                else item = base.Items.Cast<ColorItem>().FirstOrDefault(ci => Equals(value, ci.Color));
                base.SelectedItem = item;
                if (item != null) base.Focus();
            }
        }

        private static int ConvertToGivenDpiPixel(int value, double pixelFactor) => Math.Max(1, (int)(value * pixelFactor + 0.5));

        private static bool Equals(Color c1, Color c2, bool ignoreAlpha = false)
        {
            if (c1.IsEmpty && !c2.IsEmpty) return false;
            if (!c1.IsEmpty && c2.IsEmpty) return false;
            if (c1.IsEmpty && c2.IsEmpty) return true;
            if (ignoreAlpha) return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
            return c1.A == c2.A && c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }

        private class ColorItem
        {
            public readonly Color Color;
            public readonly string Name;
            public ColorItem(string name, Color c) { Name = name; Color = c; }
            public override string ToString() => this.Name;
        }

        private new ObjectCollection Items => throw new InvalidOperationException($"{nameof(Items)} property is disabled for {typeof(NamedColorListBox).Name}. For internal use only.");

        #region public static int DpiScalingFactor()
        [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private enum DeviceCap { VERTRES = 10, DESKTOPVERTRES = 117, LOGPIXELSY = 90 }
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Get current DPI scaling factor as a percentage
        /// </summary>
        /// <returns>Scaling percentage</returns>
        public static float DpiScalingFactor()
        {
            IntPtr hDC = IntPtr.Zero;
            try
            {
                hDC = GetDC(IntPtr.Zero);
                int logpixelsy = GetDeviceCaps(hDC, (int)DeviceCap.LOGPIXELSY);
                float dpiScalingFactor = logpixelsy / 96f;
                //Smaller - 100% == screenScalingFactor=1.0 dpiScalingFactor=1.0
                //Medium - 125% (default) == screenScalingFactor=1.0 dpiScalingFactor=1.25
                //Larger - 150% == screenScalingFactor=1.0 dpiScalingFactor=1.5
                return dpiScalingFactor * 100f;
            }
            finally
            {
                if (hDC != IntPtr.Zero) ReleaseDC(IntPtr.Zero, hDC);
            }
        }
        #endregion
    }
}
