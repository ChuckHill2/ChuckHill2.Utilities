using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;


namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Split screen preview color panel. The left side is the nearest known color and the right side is the current color.
    /// Clicking on the color panel will set the current color to the nearest known color.
    /// It is up to the caller to subscribe to the click event to handle the new color.
    /// </summary>
    [DefaultEvent("Click")]
    [DefaultProperty("Color")]
    public class SplitColorPanel : Panel
    {
        private Brush _textureBrush;
        private ToolTip tt;

        public SplitColorPanel() : base()
        {
            base.BackColor = Color.Transparent;
            base.BorderStyle = BorderStyle.FixedSingle;
            base.DoubleBuffered = true;
            tt = new ToolTip();
        }

        private Color __color;
        /// <summary>
        /// Color to preview. May include alpha transparency.
        /// </summary>
        [Category("Data"), Description("Color to preview or nearest known color upon click.")]
        public Color Color
        {
            get => __color;
            set
            {
                __color = value;

                if (__color.IsNamedColor)
                {
                    NearestKnownColor = __color;
                    NearestKnownName = __color.Name;
                }
                else
                {
                    if (__color.A == 0)
                    {
                        NearestKnownColor = Color.Transparent;
                        NearestKnownName = NearestKnownColor.Name;
                    }
                    else
                    {
                        NearestKnownColor = ((HSLColor)__color).NearestKnownColor();
                        NearestKnownName = NearestKnownColor.Name;
                        if (__color.A != 255)
                        {
                            NearestKnownColor = Color.FromArgb(__color.A, NearestKnownColor);
                            NearestKnownName = $"({__color.A},{NearestKnownName})";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the formatted name of the nearest known color. May include the alpha transparency in the name.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NearestKnownName { get; private set; }

        /// <summary>
        /// Get the nearest known color. Alpha transparency is left intact.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color NearestKnownColor { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_textureBrush != null)
                {
                    _textureBrush.Dispose();
                    _textureBrush = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);

            var rc = base.ClientRectangle;
            if (this.Color.A != 255)
            {
                if (_textureBrush == null)
                    _textureBrush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Silver, Color.White);

                e.Graphics.FillRectangle(_textureBrush, base.ClientRectangle);
            }

            using (Brush brush = new SolidBrush(this.NearestKnownColor))
                e.Graphics.FillRectangle(brush, rc.Left, rc.Top, rc.Width / 2, rc.Bottom);
            //e.Graphics.FillRectangle(brush, base.ClientRectangle);

            using (Brush brush = new SolidBrush(this.Color))
                e.Graphics.FillRectangle(brush, rc.Left + rc.Width / 2, rc.Top, rc.Right, rc.Bottom);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            string name = string.Empty;
            if (Color.IsNamedColor)
            {
                name = Color.Name;
            }
            else
            {
                if (Color.A == 0) name = "Transparent";
                else
                {
                    var clr = HSLColor.GetKnownColors().FirstOrDefault(c => c.R == Color.R && c.G == Color.G && c.B == Color.B);
                    if (clr == Color.Empty) name = Color.A != 255 ? $"({Color.A},{Color.R},{Color.G},{Color.B})" : $"({Color.R},{Color.G},{Color.B})";
                    else name = Color.A != 255 ? $"({Color.A},{clr.Name})" : clr.Name;
                }
            }

            this.tt.SetToolTip(this, $"{NearestKnownName} | {name}");

            //tt.Show($"{NearestKnownName} | {name}", this);
        }

        protected override void OnClick(EventArgs e)
        {
            this.Color = NearestKnownColor;
            base.OnClick(e);
        }
    }
}
