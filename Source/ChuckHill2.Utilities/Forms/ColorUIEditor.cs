//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ColorUIEditor.cs" company="Chuck Hill">
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;

/// <summary>
/// Forms-specific utilities and controls.
/// </summary>
namespace ChuckHill2.Forms
{
    ///  @image html ColorUIEditor.png
    /// <summary>Provides an enhanced <see cref="T:System.Drawing.Design.UITypeEditor" /> for visually picking a color.</summary>
    /// <remarks>
    /// Includes several ways to enter a color, including transparency.
    /// Pressing Escape returns without changing the color (e.g. Cancel).
    /// Pressing Enter or clicking outside of the editor will commit the color (e.g. Ok).
    /// Editor popup (and all child controls) resizes to fit the column width.<br />
    /// **Usage:**
    /// <pre>
    ///     [Editor(typeof(ColorUIEditor), typeof(UITypeEditor))]
    ///     public Color MyColor { get; set; }
    /// </pre>
    /// </remarks>
    public class ColorUIEditor : UITypeEditor
    {
        private ColorPickerPanelVert dropdownControl; //color picker panel
        IWindowsFormsEditorService service;
        private bool isCancelled;
        private Size preferredSize;

        #region Override Methods
        //! @cond DOXYGENHIDE

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.DropDown;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (!(value is Color)) return value;
            if (provider != null)
            {
                service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (service != null)
                {
                    if (this.dropdownControl == null)
                    {
                        this.dropdownControl = new ColorPickerPanelVert();
                        ((Control)this.dropdownControl).Text = "ColorPickerPanelVertControl";
                        preferredSize = this.dropdownControl.Size;
                        AddPreviewKeyDown(this.dropdownControl);
                        this.dropdownControl.ProportionalResizing = true;
                    }

                    dropdownControl.Color = (Color)value;

                    // The UITypeEditor may have changed the width during a previous call.
                    // We reset it here so it will be resized to the current column width.
                    // The height is proportionately scaled via ProportionalResizing flag.
                    this.dropdownControl.Size = preferredSize;

                    isCancelled = false;
                    service.DropDownControl(this.dropdownControl);
                    if (!isCancelled) value = dropdownControl.Color;
                }
            }
            return value;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;
        public override void PaintValue(PaintValueEventArgs e)
        {
            if (!(e.Value is Color)) return;
            var color = (Color)e.Value;

            if (color.A < 255) //add  background trasparency  checkerboard
                using (var br = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.Transparent))
                    e.Graphics.FillRectangle(br, e.Bounds);

            using (var solidBrush = new SolidBrush(color))
                e.Graphics.FillRectangle(solidBrush, e.Bounds);
        }

        //! @endcond
        #endregion

        private void AddPreviewKeyDown(Control control)
        {
            //Handle all the keystrokes for all the child controls, recursively. So no matter who has the focus, we will get the keystrokes.
            if (!control.HasChildren) return;
            foreach (Control c in control.Controls)
            {
                AddPreviewKeyDown(c);
                c.PreviewKeyDown += ColorUI_PreviewKeyDown;
            }
        }

        private void ColorUI_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //Detect OK (e.g.Enter) or Cancel (e.g.Escape)
            if (e.KeyValue == (int)Keys.Escape)
            {
                isCancelled = true;
                e.IsInputKey = false;
                service.CloseDropDown();
            }
            else if (e.KeyValue == (int)Keys.Enter)
            {
                e.IsInputKey = false;
                service.CloseDropDown();
            }
        }
    }
}
