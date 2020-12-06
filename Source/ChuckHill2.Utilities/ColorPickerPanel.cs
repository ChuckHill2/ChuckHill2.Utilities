using System;
using System.ComponentModel;
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
    public partial class ColorPickerPanel : UserControl
    {
        #region Constants
        private static readonly object _eventPreviewColorChanged = new object();
        #endregion

        #region Constructors
        public ColorPickerPanel()
        {
            this.InitializeComponent();
            this.screenColorPicker.Image = (Image)new ImageAttribute(typeof(Cyotek.Windows.Forms.ScreenColorPicker), "eyedropper.png").Image.Clone();
            this.BorderStyle = BorderStyle.None;
            this.ShowAlphaChannel = true;
            this.Font = SystemFonts.DialogFont;
            AddPreviewKeyDownChildren(this.Controls);
        }

        private void AddPreviewKeyDownChildren(ControlCollection controls)
        {
            foreach(Control c in controls)
            {
                if (c.HasChildren) AddPreviewKeyDownChildren(c.Controls);
                c.PreviewKeyDown += Control_PreviewKeyDown;
            }
        }

        private void Control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }
        #endregion

        #region Events
        [Category("Property Changed")]
        public event EventHandler PreviewColorChanged
        {
            add { this.Events.AddHandler(_eventPreviewColorChanged, value); }
            remove { this.Events.RemoveHandler(_eventPreviewColorChanged, value); }
        }
        #endregion

        #region Properties
        [Category("Data"), Description("Initial Color.")]
        public Color Color
        {
            get { return colorEditorManager.Color; }
            set { colorEditorManager.Color = value; }
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

            handler = (EventHandler)this.Events[_eventPreviewColorChanged];

            handler?.Invoke(this, e);
        }

        private void colorEditorManager_ColorChanged(object sender, EventArgs e)
        {
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

        private void previewPanel_Click(object sender, EventArgs e)
        {
            this.Color = previewPanel.Color;
        }
        #endregion
    }
}
