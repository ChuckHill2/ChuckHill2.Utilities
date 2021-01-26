using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Represents a window or dialog box that makes up an application's user interface.
    /// Extends System.Windows.Forms.Form to support a gradient color for the background.
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(Form))]
    public class GradientForm : Form, IGradientControl
    {
        private GradientBrush __backgroundGradient = null;
        /// <summary> The gradient brush used to fill the background.</summary>
        [Category("Appearance"), Description("The gradient brush used to fill the background.")]
        public GradientBrush BackgroundGradient
        {
            get => __backgroundGradient == null ? new GradientBrush(this.Parent) : __backgroundGradient;
            set { __backgroundGradient = value; OnBackgroundGradientChanged(EventArgs.Empty); }
        }
        private bool ShouldSerializeBackgroundGradient() => !BackgroundGradient.Equals(new GradientBrush(this.Parent));
        private void ResetBackgroundGradient() => BackgroundGradient = null;

        /// <summary>
        /// Occurs when the value of the BackgroundGradient property changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Event raised when the value of the BackColor property is changed on Control.")]
        public event EventHandler BackgroundGradientChanged;

        #region Hidden/Unused Properties
        //! @cond DOXYGENHIDE
        /// <summary> This is not used. See the BackgroundGradient property.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor { get => BackgroundGradient.Color1; set => BackgroundGradient.Color1 = value; }

        /// <summary> This is not used. See the BackgroundGradientChanged event.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackColorChanged { add { } remove { } }
        //! @endcond
        #endregion Hidden/Unused Properties

        /// <summary>Raises the BackgroundGradientChanged event.</summary>
        /// <param name="e">An empty EventArgs that contains no event data.</param>
        protected virtual void OnBackgroundGradientChanged(EventArgs e)
        {
            this.Invalidate();
            this.BackgroundGradientChanged?.Invoke(this,EventArgs.Empty);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            GradientControlPaint.PaintBackground(this, e);
        }
    }
}