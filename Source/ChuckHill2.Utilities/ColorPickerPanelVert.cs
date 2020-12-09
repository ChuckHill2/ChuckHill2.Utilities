using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Cyotek.Windows.Forms;

namespace ChuckHill2.Utilities
{
    [DefaultEvent("PreviewColorChanged")]
    [DefaultProperty("Color")]
    public partial class ColorPickerPanelVert : UserControl
    {
        #region Constants
        private static readonly object EVENT_PREVIEWCOLORCHANGED = new object();
        #endregion

        #region Constructors
        public ColorPickerPanelVert()
        {
            this.InitializeComponent();
            this.screenColorPicker.Image = (Image)new ImageAttribute(typeof(Cyotek.Windows.Forms.ScreenColorPicker), "eyedropper.png").Image.Clone();
            this.BorderStyle = BorderStyle.None;
            this.ShowAlphaChannel = true;
            this.Font = SystemFonts.DialogFont;
        }
        #endregion

        #region Events
        [Category("Property Changed")]
        public event EventHandler PreviewColorChanged
        {
            add { this.Events.AddHandler(EVENT_PREVIEWCOLORCHANGED, value); }
            remove { this.Events.RemoveHandler(EVENT_PREVIEWCOLORCHANGED, value); }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get/Set the color to be modified.
        /// </summary>
        [Category("Data"), Description("Initial Color and returning final color")]
        public Color Color
        {
            get { return colorEditorManager.Color; }
            set { colorEditorManager.Color = value; previewPanel.Color = value; }
        }

        /// <summary>
        /// Force this control to scale/resize width/height proportionately such that the ColorWheel will always completely fill the top of the control.
        /// </summary>
        [Category("Layout"), Description("As the control width is changed, the height is also changed to optimally fit all the child controls.")]
        public bool ProportionalResizing { get; set; }

        private int __bottomHeight = 0;
        /// <summary>
        /// Height of bottom half of this control that excludes the square ColorWheel. Important when one wants to force the square ColorWheel control to use all the surrounding free space in the panel.
        /// </summary>
        private int BottomHeight
        {
            get
            {
                //Height of all controls EXCEPT colorWheel. Used to force the square ColorWheel control to use all the surrounding free space when ColorPickerPanelVert is used in a UITypeEditor.
                //Late initialization because child controls may be resized due to DPI changes. 
                if (__bottomHeight==0) __bottomHeight = (this.ClientRectangle.Height - colorGrid.Top) + (colorWheel.Bottom - colorGrid.Top);
                return __bottomHeight;
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowAlphaChannel { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            colorEditor.ShowAlphaChannel = this.ShowAlphaChannel;

            if (!this.ShowAlphaChannel)
            {
                for (int i = 0; i < colorGrid.Colors.Count; i++)
                {
                    Color color;

                    color = colorGrid.Colors[i];
                    if (color.A != 255)
                    {
                        colorGrid.Colors[i] = Color.FromArgb(255, color);
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="PreviewColorChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnPreviewColorChanged(EventArgs e)
        {
            EventHandler handler;

            handler = (EventHandler)this.Events[EVENT_PREVIEWCOLORCHANGED];

            handler?.Invoke(this, e);
        }

        private void colorEditorManager_ColorChanged(object sender, EventArgs e)
        {
            previewPanel.Color = this.Color;
            previewPanel.Invalidate();

            this.OnPreviewColorChanged(e);
        }

        private void colorGrid_EditingColor(object sender, EditColorCancelEventArgs e)
        {
            e.Cancel = true;

            using (ColorDialog dialog = new ColorDialog
            {
                FullOpen = true,
                Color = e.Color
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    colorGrid.Colors[e.ColorIndex] = dialog.Color;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            //Force ColorWheel to use all the surrounding free space when this is enabled.
            if (ProportionalResizing)
            {
                if (this.Dock == DockStyle.Fill) //used by UITypeEditor.DropDownControl()
                {
                    if (this.Parent != null)
                        this.Parent.Height = this.Width + BottomHeight; //In layout, ColorWheel is at top and is square where its width==height
                }
                else this.Height = this.Width + BottomHeight;
            }

            base.OnResize(e);
        }

        private void previewPanel_Click(object sender, EventArgs e)
        {
            colorEditorManager.Color = previewPanel.Color;
        }
        #endregion
    }
}
