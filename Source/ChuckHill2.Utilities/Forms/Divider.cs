//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Divider.cs" company="Chuck Hill">
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using ControlDesigner = System.Windows.Forms.Design.ControlDesigner;

// =======================================================================
// This file has NO external dependencies as such this file may be reused anywhere.
// =======================================================================

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Line styles for the Divider custom control.
    /// </summary>
    public enum DividerLineStyle
    {
        /// <summary>
        /// Solid colored divider.
        /// </summary>
        Solid = 0,  //same as LineStyle.Solid
        /// <summary>
        ///  A divider made up of dashes (e.g. - - - - - - ). Use background color for alternating colors.
        /// </summary>
        Dash = 1,  //same as LineStyle.Dash
        /// <summary>
        /// A divider made up of dots.  (e.g. ··········· ). Use background color for alternating colors.
        /// </summary>
        Dot = 2,  //same as LineStyle.Dot
        /// <summary>
        /// A divider made up of a series of alternating dashes and dots.  (e.g. · - · - · - · - ). Use background color for alternating colors.
        /// </summary>
        DashDot = 3,  //same as LineStyle.DashDot
        /// <summary>
        /// A divider made up of a series of alternating dashes and double dots.  (e.g. · · - · · - · · - · · - ). Use background color for alternating colors.
        /// </summary>
        DashDotDot = 4,  //same as LineStyle.DashDotDot
        /// <summary>
        /// Gradient divider from left to right.
        /// </summary>
        Horizontal,  //The rest are custom gradients
        /// <summary>
        /// Gradient divider from top to bottom.
        /// </summary>
        Vertical,
        /// <summary>
        /// Gradient divider forward '/' diagonal from top-left to bottom-right
        /// </summary>
        ForwardDiagonal,
        /// <summary>
        /// Gradient divider backward '\' diagonal from top-right to bottom-left
        /// </summary>
        BackwardDiagonal,
        /// <summary>
        /// Gradient divider from center to periphery.
        /// </summary>
        Center,
        /// <summary>
        /// Gradient divider from left and right to center.
        /// </summary>
        CenterHorizontal,
        /// <summary>
        /// Gradient divider from top and bottom to center.
        /// </summary>
        CenterVertical,
        /// <summary>
        ///  Gradient divider forward '/' diagonal from top-left and bottom-right to center.
        /// </summary>
        CenterForwardDiagonal,
        /// <summary>
        ///  Gradient divider backward '\' diagonal from top-right and bottom-left to center.
        /// </summary>
        CenterBackwardDiagonal
    }

    /// <summary>
    /// Simple horizontal or vertical divider/separator line. Supports multiple line styles, colors and gradients.
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(Form))]
    [Designer(typeof(DividerDesigner))]
    [DefaultEvent("Click")]
    [DefaultProperty("Thickness")]
    public class Divider: Control
    {
        #region Divider-Specific Properties
        /// <summary>
        /// Get or set the orientation of the divider line.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(Orientation.Horizontal)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("Orientation of divider.")]
        public Orientation Orientation
        {
            get => __Orientation;
            set
            {
                if (__Orientation == value) return;
                __Orientation = value;

                this.SuspendLayout();
                var b = this.Bounds;
                var centerX = b.Left + b.Width/2;
                var centerY = b.Top + b.Height / 2;
                var left = centerX - b.Height / 2;
                var top = centerY - b.Width / 2;
                SetBounds(left, top, b.Height, b.Width);
                this.Invalidate();
                this.ResumeLayout();
            }
        }
        private Orientation __Orientation = Orientation.Horizontal;

        /// <summary>
        /// Get or set the divider line thickness.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(1)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("Divider thickness.")]
        public int Thickness
        {
            get => __Thickness;
            set
            {
                if (__Thickness == value) return;
                __Thickness = value;
                if (Orientation == Orientation.Horizontal) Height = Thickness;
                else Width = Thickness;
                this.Invalidate();
            }
        }
        private int __Thickness = 1;

        /// <summary>
        /// Get or set the foreground image for the divider line. If defined the divider line style is ignored.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(null)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("Foreground image to use for this control. If defined, all other line styles are ignored.")]
        public Image ForeGroundImage
        {
            get => __ForeGroundImage;
            set
            {
                if (__ForeGroundImage == value) return;
                __ForeGroundImage = value;
                this.Invalidate();
            }
        }
        private Image __ForeGroundImage = null;

        /// <summary>
        /// Get or set the layout style how the foreground image is drawn.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(ImageLayout.Tile)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("The layout style how the foreground image is drawn..")]
        public ImageLayout ForeGroundImageLayout
        {
            get => __ForeGroundImageLayout;
            set
            {
                if (__ForeGroundImageLayout == value) return;
                __ForeGroundImageLayout = value;
                this.Invalidate();
            }
        }
        private ImageLayout __ForeGroundImageLayout = ImageLayout.Tile;

        /// <summary>
        /// Get or set the style of divider line. Ignored if he foreground image is defined.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(DividerLineStyle.Solid)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("The style of divider line. Ignored if he foreground image is defined.")]
        public DividerLineStyle LineStyle 
        {
            get => __LineStyle;
            set
            {
                if (__LineStyle == value) return;
                __LineStyle = value;
                this.Invalidate();
            }
        }
        private DividerLineStyle __LineStyle = DividerLineStyle.Solid;

        /// <summary>
        /// Get or set the color of the divider line.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("The color of the divider line.")]
        public override Color ForeColor { get => base.ForeColor; set => base.ForeColor = value; }

        /// <summary>
        /// Get or set the 2nd color of a 2-color gradient. Used only when a gradient line style is selected.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("The 2nd color of a 2-color gradient. Used only when a gradient line style is selected.")]
        public Color ForeColor2
        {
            get => __ForeColor2;
            set
            {
                if (__ForeColor2 == value) return;
                __ForeColor2 = value;
                this.Invalidate();
            }
        }
        private Color __ForeColor2 = Control.DefaultForeColor;
        private bool ShouldSerializeForeColor2() => ForeColor2 != Control.DefaultForeColor;
        private void ResetForeColor2() => ForeColor2 = Control.DefaultForeColor;

        /// <summary>
        /// Controls the overall brightness and ratio of red to green to blue hues. Enables a more uniform intensity across the gradient. Used only when a gradient line style is selected.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [DefaultValue(false)]
        [Category("Appearance"), Description("Controls the overall brightness and ratio of red to green to blue hues. Enables a more uniform intensity across the gradient. Used only when a gradient line style is selected.")]
        public bool GammaCorrection
        {
            get => __GammaCorrection;
            set
            {
                if (__GammaCorrection == value) return;
                __GammaCorrection = value;
                this.Invalidate();
            }
        }
        private bool __GammaCorrection = false;

        /// <summary>
        /// Get or set a colored solid single pixel border around border line. Ignored if Thickness < 3 pixels.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Category("Appearance"), Description("A colored solid single pixel border around border line. Ignored if Thickness < 3 pixels.")]
        public Color BorderColor
        {
            get => __BorderColor;
            set
            {
                if (__BorderColor == value) return;
                __BorderColor = value;
                this.Invalidate();
            }
        }
        private Color __BorderColor = Color.Empty;
        private bool ShouldSerializeBorderColor() => BorderColor != Color.Empty && BorderColor != Color.Transparent;
        private void ResetBorderColor() => BorderColor = Color.Empty;
        #endregion Divider-Specific Properties

        #region Hidden/Disabled Properties
        private const string NOTUSED = "Not used in " + nameof(DividerLineStyle) + ".";
        //! @cond DOXYGENHIDE
        #pragma warning disable CS0067, CS0809, CS0109 //Properties and Events never used
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text { get => base.Text; set => base.Text = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Cursor Cursor { get => base.Cursor; set => base.Cursor = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font { get => base.Font; set => base.Font = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override RightToLeft RightToLeft { get => base.RightToLeft; set => base.RightToLeft = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseWaitCursor { get => base.UseWaitCursor; set => base.UseWaitCursor = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool AllowDrop { get => base.AllowDrop; set => base.AllowDrop = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImeMode ImeMode { get => base.ImeMode; set => base.ImeMode = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MaximumSize { get => base.MaximumSize; set => base.MaximumSize = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Size MinimumSize { get => base.MinimumSize; set => base.MinimumSize = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DockStyle Dock { get => base.Dock; set => base.Dock = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool AutoSize { get => base.AutoSize; set => base.AutoSize = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CausesValidation { get => base.AutoSize; set => base.AutoSize = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AccessibleRole AccessibleRole { get => base.AccessibleRole; set => base.AccessibleRole = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override BindingContext BindingContext { get => base.BindingContext; set => base.BindingContext = value; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ControlBindingsCollection DataBindings { get => base.DataBindings; }

        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ChangeUICues;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ControlAdded;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ControlRemoved;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler HelpRequested;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ImeModeChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler QueryAccessibilityHelp;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler StyleChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DragDrop;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DragEnter;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DragLeave;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DragOver;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler GiveFeedback;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler QueryContinueDrag;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler Validating;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler Validated;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BindingContextChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler CausesValidationChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler CursorChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DockChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FontChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler RightToLeftChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TextChanged;
        #pragma warning restore CS0067, CS0809, CS0109 //Properties and Events never used
        //! @endcond
        #endregion Hidden/Disabled Properties

        public Divider()
        {
            base.AccessibleRole = AccessibleRole.Separator; 
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (Orientation == Orientation.Horizontal)
                base.SetBoundsCore(x, y, width, Thickness, specified);
            else
                base.SetBoundsCore(x, y, Thickness, height, specified);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            if (ForeGroundImage != null)
            {
                DrawBackgroundImage(g, ForeGroundImage, BackColor, ForeGroundImageLayout, Orientation, this.ClientRectangle, this.ClientRectangle, Point.Empty, RightToLeft.No);
            }
            else if (LineStyle == DividerLineStyle.Solid)
            {
                Brush brush = ForeGroundImage != null ? (Brush)new TextureBrush(ForeGroundImage) : new SolidBrush(this.ForeColor);
                g.FillRectangle(brush, this.ClientRectangle);
                brush.Dispose();
            }
            else if (LineStyle <= DividerLineStyle.DashDotDot)
             {
                Pen pen = new Pen(this.ForeColor, Thickness);
                //Note: Pen.Alignment is always centered on the line. Only when pen is used for enclosed shapes is alignment relevant.
                // See https://stackoverflow.com/questions/27964001/pen-alignment-is-not-working-while-drawing-line

                pen.DashStyle = (DashStyle)LineStyle;

                if (Orientation == Orientation.Horizontal)
                    g.DrawLine(pen, 0,Height/2, Width, Height/2);
                else
                    g.DrawLine(pen, Width/2, 0, Width/2, this.Height);

                pen.Dispose();
            }
            else
            {
                Brush brush = GetGradientBrush(this.ClientRectangle);
                g.FillRectangle(brush, this.ClientRectangle);
                brush.Dispose();
            }

            //Add border around divider
            if (this.Thickness >= 3 && BorderColor != Color.Empty && BorderColor != Color.Transparent)
            {
                Pen pen = new Pen(this.BorderColor);
                g.DrawRectangle(pen, 0,0,this.Width-1,this.Height-1);
                pen.Dispose();
            }
        }

        //Lifted from typeof(System.Windows.Forms.ControlPaint).GetMethod("DrawBackgroundImage", BindingFlags.Static | BindingFlags.NonPublic...) with Orientation tweaks.
        private static void DrawBackgroundImage(Graphics g, Image image, Color backColor, ImageLayout imageLayout, Orientation orient, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
        {
            if (g == null) throw new ArgumentNullException(nameof(g));
            if (imageLayout == ImageLayout.Tile)
            {
                using (TextureBrush textureBrush = new TextureBrush(image, WrapMode.Tile))
                {
                    if (scrollOffset != Point.Empty)
                        textureBrush.TranslateTransform((float)scrollOffset.X, (float)scrollOffset.Y);

                    if (orient == Orientation.Vertical)
                    {
                        textureBrush.RotateTransform(-90);
                        var scale = bounds.Width / (float)image.Height;
                        textureBrush.ScaleTransform(scale, scale, MatrixOrder.Append);
                    }
                    else
                    {
                        var scale = bounds.Height / (float)image.Height;
                        textureBrush.ScaleTransform(scale, scale, MatrixOrder.Append);
                    }

                    g.FillRectangle((Brush)textureBrush, clipRect);
                }
            }
            else
            {
                Rectangle backgroundImageRectangle = CalculateBackgroundImageRectangle(bounds, image, imageLayout);

                if (rightToLeft == RightToLeft.Yes && imageLayout == ImageLayout.None)
                    backgroundImageRectangle.X += clipRect.Width - backgroundImageRectangle.Width;

                using (SolidBrush solidBrush = new SolidBrush(backColor))
                    g.FillRectangle((Brush)solidBrush, clipRect);

                if (orient == Orientation.Vertical) g.RotateTransform(-90);
                if (!clipRect.Contains(backgroundImageRectangle))
                {
                    switch (imageLayout)
                    {
                        case ImageLayout.None:
                            backgroundImageRectangle.Offset(clipRect.Location);
                            Rectangle destRect1 = backgroundImageRectangle;
                            destRect1.Intersect(clipRect);
                            Rectangle rectangle1 = new Rectangle(Point.Empty, destRect1.Size);
                            g.DrawImage(image, destRect1, rectangle1.X, rectangle1.Y, rectangle1.Width, rectangle1.Height, GraphicsUnit.Pixel);
                            break;
                        case ImageLayout.Stretch:
                        case ImageLayout.Zoom:
                            backgroundImageRectangle.Intersect(clipRect);
                            g.DrawImage(image, backgroundImageRectangle);
                            break;
                        default:
                            Rectangle destRect2 = backgroundImageRectangle;
                            destRect2.Intersect(clipRect);
                            Rectangle rectangle2 = new Rectangle(new Point(destRect2.X - backgroundImageRectangle.X, destRect2.Y - backgroundImageRectangle.Y), destRect2.Size);
                            g.DrawImage(image, destRect2, rectangle2.X, rectangle2.Y, rectangle2.Width, rectangle2.Height, GraphicsUnit.Pixel);
                            break;
                    }
                }
                else
                {
                    ImageAttributes imageAttr = new ImageAttributes();
                    imageAttr.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(image, backgroundImageRectangle, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttr);
                    imageAttr.Dispose();
                }
                if (orient == Orientation.Vertical) g.RotateTransform(0);
            }
        }

        private static Rectangle CalculateBackgroundImageRectangle(Rectangle bounds, Image backgroundImage, ImageLayout imageLayout)
        {
            Rectangle backgroundImageRectangle = bounds;
            if (backgroundImage != null)
            {
                switch (imageLayout)
                {
                    case ImageLayout.None:
                        backgroundImageRectangle.Size = backgroundImage.Size;
                        break;
                    case ImageLayout.Center:
                        backgroundImageRectangle.Size = backgroundImage.Size;
                        Size size1 = bounds.Size;
                        if (size1.Width > backgroundImageRectangle.Width)
                            backgroundImageRectangle.X = (size1.Width - backgroundImageRectangle.Width) / 2;
                        if (size1.Height > backgroundImageRectangle.Height)
                        {
                            backgroundImageRectangle.Y = (size1.Height - backgroundImageRectangle.Height) / 2;
                            break;
                        }
                        break;
                    case ImageLayout.Stretch:
                        backgroundImageRectangle.Size = bounds.Size;
                        break;
                    case ImageLayout.Zoom:
                        Size size2 = backgroundImage.Size;
                        float num1 = (float)bounds.Width / (float)size2.Width;
                        float num2 = (float)bounds.Height / (float)size2.Height;
                        if ((double)num1 < (double)num2)
                        {
                            backgroundImageRectangle.Width = bounds.Width;
                            backgroundImageRectangle.Height = (int)((double)size2.Height * (double)num1 + 0.5);
                            if (bounds.Y >= 0)
                            {
                                backgroundImageRectangle.Y = (bounds.Height - backgroundImageRectangle.Height) / 2;
                                break;
                            }
                            break;
                        }
                        backgroundImageRectangle.Height = bounds.Height;
                        backgroundImageRectangle.Width = (int)((double)size2.Width * (double)num2 + 0.5);
                        if (bounds.X >= 0)
                        {
                            backgroundImageRectangle.X = (bounds.Width - backgroundImageRectangle.Width) / 2;
                            break;
                        }
                        break;
                }
            }
            return backgroundImageRectangle;
        }

        #region Gradient Brush Support
        #region private static void SetGammaCorrection(PathGradientBrush pbr, bool enable)
        //Hack: Unlike LinearGradientBrush GammaCorrection property is not exposed for PathGradientBrush!
        //http://www.jose.it-berater.org/gdiplus/reference/flatapi/pathgradientbrush/gdipsetpathgradientgammacorrection.htm
        [DllImport("gdiplus.dll")] private static extern int GdipSetPathGradientGammaCorrection(IntPtr hBrush, bool useGammaCorrection);
        private static readonly FieldInfo _fiNativeBrush = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
        private static void SetGammaCorrection(PathGradientBrush pbr, bool enable) => GdipSetPathGradientGammaCorrection((IntPtr)_fiNativeBrush.GetValue(pbr), enable);
        #endregion

        /// <summary>
        /// Retrieve the gradient brush for the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle that the gradient is going to fill. If null, defaults to a solid brush using the first color.</param>
        /// <returns>Created gradient brush. It is up to the caller to dispose of the brush.</returns>
        private Brush GetGradientBrush(Rectangle? rc = null) => GetGradientBrush(!rc.HasValue ? (RectangleF?)null : new RectangleF(rc.Value.X, rc.Value.Y, rc.Value.Width, rc.Value.Height));

        /// <summary>
        /// Retrieve the gradient brush for the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle that the gradient is going to fill. If null, defaults to a solid brush using the first color.</param>
        /// <returns>Created gradient brush. It is up to the caller to dispose of the brush.</returns>
        private Brush GetGradientBrush(RectangleF? rect)
        {
            //https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-create-a-path-gradient?view=netframeworkdesktop-4.8
            //https://www.codeproject.com/Articles/20018/Gradients-made-easy
            if (!rect.HasValue || LineStyle == DividerLineStyle.Solid) return new SolidBrush(ForeColor);
            var rc = rect.Value;
            var center = new PointF(rc.Width / 2f, rc.Height / 2f);

            LinearGradientBrush lbr = null;
            switch (LineStyle)
            {
                case DividerLineStyle.Horizontal:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.Horizontal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipX };
                    break;

                case DividerLineStyle.Vertical:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.Vertical) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipY };
                    break;

                case DividerLineStyle.ForwardDiagonal:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.ForwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    break;

                case DividerLineStyle.BackwardDiagonal:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.BackwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    break;

                case DividerLineStyle.Center:
                    GraphicsPath path = new GraphicsPath();
                    rc.Inflate(rc.Width / 4.8f, rc.Height / 4.8f);
                    path.AddEllipse(rc);
                    PathGradientBrush pbr = new PathGradientBrush(path);
                    path.Dispose();
                    SetGammaCorrection(pbr, GammaCorrection);
                    pbr.CenterPoint = center;
                    pbr.CenterColor = ForeColor;
                    pbr.SurroundColors = new[] { ForeColor2 };
                    return pbr;

                case DividerLineStyle.CenterHorizontal:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.Horizontal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipX };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;

                case DividerLineStyle.CenterVertical:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.Vertical) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipY };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;

                case DividerLineStyle.CenterForwardDiagonal:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.ForwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;

                case DividerLineStyle.CenterBackwardDiagonal:
                    lbr = new LinearGradientBrush(rc, ForeColor, ForeColor2, LinearGradientMode.BackwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;
            }

            return lbr;
        }

        #endregion Gradient Brush Support
    }

    internal class DividerDesigner : ControlDesigner
    {
        //https://learn.microsoft.com/en-us/previous-versions/37899azc(v=vs.140)?redirectedfrom=MSDN
        //https://learn.microsoft.com/en-us/previous-versions/c5z9s1h4(v=vs.140)?redirectedfrom=MSDN

        public DividerDesigner() => this.AutoResizeHandles = true;

        public override SelectionRules SelectionRules
        {
            get
            {
                var baserules = base.SelectionRules & ~SelectionRules.AllSizeable;
                object component = (object)this.Component;
                PropertyDescriptor property = TypeDescriptor.GetProperties(component)["Orientation"];
                var x = (property?.GetValue(component) is Orientation orient && orient == Orientation.Vertical)
                    ? SelectionRules.TopSizeable | SelectionRules.BottomSizeable
                    : SelectionRules.LeftSizeable | SelectionRules.RightSizeable;
                return x | baserules;
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            //https://social.msdn.microsoft.com/Forums/windows/en-US/61079140-b4d4-45cd-a8d2-8fa3b0e501c5/enablingdisableing-properties-in-the-property-grid?forum=winformsdesigner
            //https://stackoverflow.com/questions/58841974/need-to-hide-a-designer-only-property-from-propertygrid-for-a-net-winforms-cont
            //https://www.google.com/search?q=c%23+Forms+designer+PreFilterProperties+hide+disable+properties&oq=c%23+Forms+designer+PreFilterProperties+hide+disable+properties&aqs=chrome..69i57j69i58.25974j0j7&sourceid=chrome&ie=UTF-8

            PropertyDescriptor property1 = (PropertyDescriptor)properties[(object)"LineStyle"];
            if (property1?.GetValue(Component) is DividerLineStyle ls && ls <= DividerLineStyle.DashDotDot)
            {
                var names = new string[] { "ForeColor2", "GammaCorrection" };
                foreach (var name in names)
                {
                    PropertyDescriptor p = (PropertyDescriptor)properties[(object)name];
                    properties.Remove(name);
                    var attrs = p.Attributes.Cast<Attribute>().ToList();
                    attrs.Add(new BrowsableAttribute(false));
                    attrs.Add(new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
                    properties[name] = TypeDescriptor.CreateProperty(p.ComponentType, name, p.PropertyType, attrs.ToArray());
                }
            }

            base.PreFilterProperties(properties);
        }
    }
}
