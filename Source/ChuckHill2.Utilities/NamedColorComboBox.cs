using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Specify how the list of colors is ordered
    /// </summary>
    public enum OrderBy
    {
        /// <summary>
        /// Colors are ordered by color and shade.
        /// </summary>
        Color,
        /// <summary>
        /// Colors are ordered by name alphabetically.
        /// </summary>
        Name
    }

    public class NamedColorComboBox : ComboBox
    {
        private int graphicWidth = 22;  //default pixel values at 96dpi

        private Rectangle ImageBounds;
        private Point TextOffset;

        private OrderBy __orderBy = OrderBy.Color;
        /// <summary>
        ///  Specify how the list of colors is ordered.
        /// </summary>
        [Category("Appearance"), Description("Specify how the list of colors is ordered.")]
        [DefaultValue(OrderBy.Color)]
        public OrderBy OrderBy
        {
            get => __orderBy;
            set
            {
                if (__orderBy == value) return;
                __orderBy = value;
                this.SuspendLayout();

                var customItems = base.Items.Cast<ColorItem>().TakeWhile(ci => !ci.Color.IsKnownColor);
                base.Items.Clear();
                if (__orderBy == OrderBy.Color)
                {
                    foreach (var ci in customItems) base.Items.Add(ci); //Custom items are always at the top of the list.
                    foreach (var c in ColorExtensions.KnownColors) base.Items.Add(new ColorItem(c.Name, c));
                }
                else
                {
                    foreach (var ci in customItems) base.Items.Add(ci);
                    foreach (var c in ColorExtensions.KnownColors.OrderBy(c => c.Name)) base.Items.Add(new ColorItem(c.Name, c));
                }

                this.ResumeLayout();
            }
        }

        #region Hidden/Disabled Properties
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoCompleteStringCollection AutoCompleteCustomSource { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoCompleteSource AutoCompleteSource { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoCompleteMode AutoCompleteMode { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object DataSource { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string DisplayMember { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ComboBoxStyle DropDownStyle { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string FormatString { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FormattingEnabled { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Text { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ObjectCollection Items { get; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ValueMember { get; set; }
        #endregion

        public NamedColorComboBox() : base()
        {
            base.Name = "NamedColorComboBox";
            base.DrawMode = DrawMode.OwnerDrawFixed;
            base.DropDownStyle = ComboBoxStyle.DropDownList;

            var pixelFactor = DpiScalingFactor() / 100.0;
            this.graphicWidth = ConvertToGivenDpiPixel(this.graphicWidth, pixelFactor);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (this.OrderBy == OrderBy.Color)
                foreach (var c in ColorExtensions.KnownColors) base.Items.Add(new ColorItem(c.Name, c));
            else
                foreach (var c in ColorExtensions.KnownColors.OrderBy(c => c.Name)) base.Items.Add(new ColorItem(c.Name, c));

            ImageBounds = new Rectangle(2, 1, graphicWidth, base.ItemHeight - 1 - 2);
            TextOffset = new Point(2 + graphicWidth + 2, -1); //-1 because we want to be vertically centered in in the blue selected rectangle
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index == -1) return;

            var ci = (ColorItem)base.Items[e.Index];

            Graphics g = e.Graphics;
            e.DrawBackground();

            var imageBounds = ImageBounds;
            imageBounds.X += e.Bounds.X;
            imageBounds.Y += e.Bounds.Y;

            var textOffset = TextOffset;
            textOffset.X += e.Bounds.X;
            textOffset.Y += e.Bounds.Y;

            if (ci.Color.A < 255) //add  background trasparency  checkerboard
            {
                using (var br = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.Transparent))
                    g.FillRectangle(br, imageBounds);
            }

            using (var solidBrush = new SolidBrush(ci.Color))
                g.FillRectangle(solidBrush, imageBounds);

            g.DrawRectangle(SystemPens.WindowText, imageBounds.X, imageBounds.Y, imageBounds.Width - 1, imageBounds.Height - 1);

            TextRenderer.DrawText(g, ci.Name, base.Font, textOffset, base.ForeColor, Color.Transparent);

            // Create a divider line between CustomColors, WebColors, and SystemColors or if
            // sorted alphabetically, just between CustomColors and all other known colors.

            if (e.Index >= 0 && e.Index < base.Items.Count-1)
            {
                var ci2 = (ColorItem)base.Items[e.Index + 1]; //compare current vs next color
                if (OrderBy == OrderBy.Color)
                {
                    if (ci.Color.IsSystemColor != ci2.Color.IsSystemColor ||
                        ci.Color.IsKnownColor != ci2.Color.IsKnownColor)
                    {
                        g.DrawLine(SystemPens.WindowText, imageBounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - imageBounds.Left, e.Bounds.Bottom - 1);
                    }
                }
                else
                {
                    if (ci.Color.IsKnownColor != ci2.Color.IsKnownColor)
                    {
                        g.DrawLine(SystemPens.WindowText, imageBounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - imageBounds.Left, e.Bounds.Bottom - 1);
                    }
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
            if (base.Items.Cast<ColorItem>().TakeWhile(ci => !ci.Color.IsKnownColor).FirstOrDefault(ci => Equals(c, ci.Color)) != null) return;
            base.Items.Insert(0, new ColorItem(c.GetName(), c));  //Custom named colors go to top of list
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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Selected
        {
            get => base.SelectedItem is ColorItem ? ((ColorItem)base.SelectedItem).Color : Color.Empty;
            set
            {
                ColorItem item = base.Items.Cast<ColorItem>().FirstOrDefault(ci => Equals(value, ci.Color));
                base.SelectedItem = item;
            }
        }

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

        private static int ConvertToGivenDpiPixel(int value, double pixelFactor) => Math.Max(1, (int)(value * pixelFactor + 0.5));
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
