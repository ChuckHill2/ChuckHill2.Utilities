//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ColorPickerPanelHorz.cs" company="Chuck Hill">
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
using System.Windows.Forms;
using Cyotek.Windows.Forms;
using ChuckHill2.Extensions;

namespace ChuckHill2.Forms
{
    ///  @image html ColorPickerPanelHorz.png
    /// <summary>
    /// A Color picker/chooser panel that allows the user to visually select a color by modifying various color properties and see the results live in the preview.
    /// This resizable panel is oriented Horizontally.
    /// </summary>
    /// <remarks>
    /// Note: When a color swatch has focus, pressing enter will popup the standard color dialog to change the color of the selected item.
    /// </remarks>
    [DefaultEvent("PreviewColorChanged")]
    [DefaultProperty("Color")]
    public partial class ColorPickerPanelHorz : UserControl
    {
        #region Constants
        private static readonly object _eventPreviewColorChanged = new object();
        #endregion

        #region Constructors
        /// <summary>
        ///  Initializes a new instance of the ColorPickerPanelHorz class.
        /// </summary>
        public ColorPickerPanelHorz()
        {
            this.InitializeComponent();
            this.screenColorPicker.Image = Image.FromStream(typeof(Cyotek.Windows.Forms.ScreenColorPicker).GetManifestResourceStream("eyedropper.png"));
            this.Font = SystemFonts.DialogFont;
        }
        #endregion

        #region Events
        /// <summary>
        /// Triggered when the preview color has changed.
        /// </summary>
        [Category("Property Changed"), Description("Triggered when the preview color has changed.")]
        public event EventHandler PreviewColorChanged
        {
            add { this.Events.AddHandler(_eventPreviewColorChanged, value); }
            remove { this.Events.RemoveHandler(_eventPreviewColorChanged, value); }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Get/Set the color to be modified.
        /// </summary>
        [Category("Data"), Description("Initial Color and final color.")]
        public Color Color
        {
            get { return colorEditorManager.Color; }
            set { colorEditorManager.Color = value; }
        }

        /// <summary>
        /// Allow color transparency to be changed by providing a translucency slider and translucent color swatches.
        /// </summary>
        [Category("Layout"), Description("Allow color transparency to be changed by providing a translucency slider and translucent color swatches.")]
        [DefaultValue(true)]
        public bool ShowAlphaChannel { get; set; } = true;
        #endregion

        #region Methods
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

        protected virtual void OnPreviewColorChanged(EventArgs e)
        {
            EventHandler handler;
            handler = (EventHandler)this.Events[_eventPreviewColorChanged];
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

        private void previewPanel_Click(object sender, EventArgs e)
        {
            this.Color = previewPanel.Color;
        }
        #endregion
    }
}
