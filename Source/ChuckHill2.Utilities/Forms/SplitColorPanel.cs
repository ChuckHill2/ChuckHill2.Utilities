//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SplitColorPanel.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    ///  @image html SplitColorPanel.png
    /// <summary>
    /// Split screen color preview panel. The left side is the nearest known color and the right side is the current color.
    /// Clicking on the panel will set the current color to the nearest known color.
    /// It is up to the caller to subscribe to the click event to handle the new color.
    /// A tooltip may be shown displaying the name of the two colors.
    /// </summary>
    [DefaultEvent("Click")]
    [DefaultProperty("Color")]
    public class SplitColorPanel : Control
    {
        private Brush _textureBrush;
        private ToolTip tt;
        private string _nearestKnownName;
        private Color _nearestKnownColor;

        /// <summary>
        /// Initializes a new instance of the SplitColorPanel class.
        /// </summary>
        public SplitColorPanel() : base()
        {
            base.Text = "SplitColorPanel";
            base.DoubleBuffered = true;
            base.CausesValidation = false;
            if (!DesignMode) tt = new ToolTip();
        }

        #region Hidden/Unused Properties
        private const string NOTUSED = "Not used in " + nameof(SplitColorPanel) + ".";
        //! @cond DOXYGENHIDE
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color BackColor { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image BackgroundImage { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageLayout BackgroundImageLayout { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CausesValidation { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContextMenuStrip ContextMenuStrip { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Font Font { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Color ForeColor { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new RightToLeft RightToLeft { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseWaitCursor { get; set; }

        #pragma warning disable CS0067 //The event is never used
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackColorChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageLayoutChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ContextMenuStripChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FontChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ForeColorChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TextChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event CancelEventHandler Validating;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler Validated;
        #pragma warning restore CS0067 //The event is never used
        //! @endcond
        #endregion Hidden/Unused Properties

        private Color __color;
        /// <summary>
        /// Color to display. May include alpha transparency.
        /// </summary>
        [Category("Appearance"), Description("Color to preview or nearest known color upon click.")]
        public Color Color
        {
            get => __color;
            set
            {
                __color = value;

                if (__color.IsNamedColor)
                {
                    _nearestKnownColor = __color;
                    _nearestKnownName = __color.Name;
                }
                else
                {
                    if (__color.A == 0)
                    {
                        _nearestKnownColor = Color.Transparent;
                        _nearestKnownName = _nearestKnownColor.Name;
                    }
                    else
                    {
                        _nearestKnownColor = __color.NearestKnownColor();
                        _nearestKnownName = _nearestKnownColor.Name;
                        if (__color.A != 255)
                        {
                            _nearestKnownColor = Color.FromArgb(__color.A, _nearestKnownColor);
                            _nearestKnownName = $"({__color.A},{_nearestKnownName})";
                        }
                    }
                }
            }
        }
        private bool ShouldSerializeColor() => Color != Color.Transparent;  //In lieu of using [DefaultValue(someConst)]
        private void ResetColor() => Color = Color.Transparent;

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

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //base.OnPaintBackground(pevent); --We do *all* the painting in OnPaint()
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rc = base.ClientRectangle;
            if (this.Color.A != 255)
            {
                if (_textureBrush == null)
                {
                    //Color.Transparent is not supported in a bare naked Control so we walk up the parent chain until we find a non-transparent/translucent color
                    Control parent = this.Parent;
                    while (parent != null && parent.BackColor.A < 255) parent = parent.Parent;
                    var backColor = parent != null && parent.BackColor.A == 255 ? parent.BackColor : SystemColors.Control;
                    _textureBrush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.FromArgb(225, 225, 225), backColor);
                }

                e.Graphics.FillRectangle(_textureBrush, rc);
            }

            //left side: nearest known color
            using (Brush brush = new SolidBrush(this._nearestKnownColor))
                e.Graphics.FillRectangle(brush, rc.Left, rc.Top, rc.Width / 2, rc.Bottom);

            //right side: current color
            using (Brush brush = new SolidBrush(this.Color))
                e.Graphics.FillRectangle(brush, rc.Left + rc.Width / 2, rc.Top, rc.Right, rc.Bottom);

            //Border
            e.Graphics.DrawRectangle(Pens.Black, rc.Left, rc.Top, rc.Right - 1, rc.Bottom - 1);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            this.tt?.SetToolTip(this, $"{this._nearestKnownName} | {this.Color.GetName()}");
        }

        protected override void OnClick(EventArgs e)
        {
            this.Color = this._nearestKnownColor;
            base.OnClick(e);
        }
    }
}
