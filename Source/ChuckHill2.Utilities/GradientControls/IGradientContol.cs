using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    // NOTE: These components must reside in a separate assembly in order to
    // allow the Designer to be able to serialize these new GradientBrush properties.

    // NOTE: For performance reasons, components in the auto-populated area of 
    // the Toolbox do not display custom bitmaps, and the ToolboxBitmapAttribute 
    // is not supported. To display an icon for a custom component in the 
    // Toolbox, use the Choose Toolbox Items dialog box to load your component.

    // SplitContainer contains array of SplitterPanel so this would take significant effort to imlement gradient backgrounds.
    // TabControl contains array of TabPage so this would take significant effort to imlement gradient backgrounds.

    /// <summary>
    /// Interface for gradient control common properties plus requisite minimum basic control properties for supporting drawing.
    /// </summary>
    public interface IGradientControl
    {
        /// <summary>
        /// The gradient brush used to fill the background.
        /// This is a complete replacement for Control.BackColor
        /// </summary>
        GradientBrush BackgroundGradient { get; set; }

        /// <summary>
        /// Occurs when the value of the BackgroundGradient property changes.
        /// This is a complete replacement for Control.BackColorChanged.
        /// </summary>
        event EventHandler BackgroundGradientChanged;

        #region Base System.Windows.Forms.Control Properties
        //! @cond DOXYGENHIDE 

        /// <summary>
        /// Retrieves the rectangle of the inner area of this control.
        /// </summary>
        Rectangle ClientRectangle { get; }

        /// <summary>
        /// The background color of the component.
        /// </summary>
        Color BackColor { get; }

        /// <summary>
        /// Indicates whether the control is mirrored.
        /// </summary>
        bool IsMirrored { get; }

        /// <summary>
        /// The background image used for the control.
        /// </summary>
        Image BackgroundImage { get; set; }

        /// <summary>
        /// The background image layout used for the control.
        /// </summary>
        ImageLayout BackgroundImageLayout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether control's elements are aligned to support locales using right-to-left fonts.
        /// </summary>
        RightToLeft RightToLeft { get; set; }

        /// <summary>
        /// The font used to display text in the control.
        /// </summary>
        Font Font { get; set; }

        //! @endcond  
        #endregion
    }

    /// <summary>
    /// Ripped, stripped, and tweaked helper class for just what we need for Gradient controls.
    /// </summary>
    internal static class GradientControlPaint
    {
        // We need these to properly mirror the control's background paint actions without cloning everything!
        private static MethodInfo miRenderColorTransparent = typeof(Control).GetMethod("RenderColorTransparent", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Color) }, null);
        private static MethodInfo miPaintTransparentBackground = typeof(Control).GetMethod("PaintTransparentBackground", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(PaintEventArgs), typeof(Rectangle) }, null);
        private static MethodInfo miIsImageTransparent = typeof(ControlPaint).GetMethod("IsImageTransparent", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Image) }, null);
        private static Type DisplayInformationType = Type.GetType("System.Windows.Forms.DisplayInformation, " + typeof(Control).Assembly.FullName, false, false);
        private static PropertyInfo piDisplayInformation_HighContrast = DisplayInformationType.GetProperty("HighContrast", BindingFlags.Static | BindingFlags.Public);
        private static MethodInfo miDrawBackgroundImage = typeof(ControlPaint).GetMethod("DrawBackgroundImage", BindingFlags.Static | BindingFlags.NonPublic, null,
            new[] { typeof(Graphics), typeof(Image), typeof(Color), typeof(ImageLayout), typeof(Rectangle), typeof(Rectangle), typeof(Point), typeof(RightToLeft) }, null);

        //private static bool RenderColorTransparent(Control ctrl, Color c) => !(ctrl is Form) && c.A < byte.MaxValue;
        private static bool RenderColorTransparent(Control control, Color c) => (bool)miRenderColorTransparent.Invoke(control, new object[] { c });
        private static void PaintTransparentBackground(Control control, PaintEventArgs e, Rectangle rectangle) => miPaintTransparentBackground.Invoke(control, new object[] { e, rectangle });
        private static bool ControlPaint_IsImageTransparent(Image img) => (bool)miIsImageTransparent.Invoke(null, new object[] { img });
        private static bool DisplayInformation_HighContrast => (bool)piDisplayInformation_HighContrast.GetValue(null);
        private static void ControlPaint_DrawBackgroundImage(
            Graphics g, Image backgroundImage, Color backColor, ImageLayout backgroundImageLayout, Rectangle bounds, Rectangle clipRect, Point scrollOffset, RightToLeft rightToLeft)
            => miDrawBackgroundImage.Invoke(null, new object[] { g, backgroundImage, backColor, backgroundImageLayout, bounds, clipRect, scrollOffset, rightToLeft });

        /// <summary>
        /// This is a complete replacement for base.OnPaintBackground(e) within method protected override void OnPaintBackground(PaintEventArgs e).
        /// This supports gradient backgrounds with optional background image overlays.
        /// The original only supported solid backgrounds with optional background image overlays.
        /// </summary>
        /// <param name="control">The control that needs its background painted.</param>
        /// <param name="e">Paint event args</param>
        /// <param name="gradientBrush">Gradient brush object.</param>
        internal static void PaintBackground(IGradientControl gradientControl, PaintEventArgs e)
        {
            GradientBrush gradientBrush = gradientControl.BackgroundGradient;
            Control control = (Control)gradientControl;

            Rectangle paintRect = control.ClientRectangle;
            if (control is GradientGroupBox)
            {
                const int borderWidth = 1;
                int offset = control.Font.Height / 2;
                paintRect.X += borderWidth;
                paintRect.Width -= (borderWidth * 2);
                paintRect.Y += offset + 1;
                paintRect.Height -= (offset + borderWidth * 2)+1;
                PaintTransparentBackground(control, e, control.ClientRectangle);
            }
            else if (RenderColorTransparent(control, gradientBrush.Color1) ||
                     RenderColorTransparent(control, gradientBrush.Color2)) PaintTransparentBackground(control, e, control.ClientRectangle);

            int num;
            switch (control)
            {
                case Form _:
                //case MdiClient _:
                    num = control.IsMirrored ? 1 : 0;
                    break;
                default:
                    num = 0;
                    break;
            }
            bool flag = num != 0;
            if (control.BackgroundImage != null && !DisplayInformation_HighContrast && !flag)
            {
                if (control.BackgroundImageLayout == ImageLayout.Tile && ControlPaint_IsImageTransparent(control.BackgroundImage))
                    PaintTransparentBackground(control, e, control.ClientRectangle);

                var scrollOffset1 = control is ScrollableControl && ((ScrollableControl)control).AutoScroll ? ((ScrollableControl)control).AutoScrollPosition : Point.Empty;

                if (ControlPaint_IsImageTransparent(control.BackgroundImage))
                    PaintBackColor(e, paintRect, gradientBrush);
                ControlPaint_DrawBackgroundImage(e.Graphics, control.BackgroundImage, control.BackColor, control.BackgroundImageLayout, control.ClientRectangle, e.ClipRectangle, scrollOffset1, control.RightToLeft);
            }
            else
                PaintBackColor(e, paintRect, gradientBrush);
        }

        private static void PaintBackColor(PaintEventArgs e, Rectangle bounds, GradientBrush gradientBrush)
        {
            if (gradientBrush.Color1.A == 0 && gradientBrush.Color2.A == 0) return;
            using (Brush br = gradientBrush.GetBrush(bounds))
                e.Graphics.FillRectangle(br, bounds);
        }
    }
}
