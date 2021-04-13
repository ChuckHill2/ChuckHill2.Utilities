//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SaveLogNameEditor.cs" company="Chuck Hill">
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
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ChuckHill2.LoggerEditor
{
    /// <summary>Provides a user interface for selecting a file name.</summary>
    /// <remarks>Based upon decompiled System.Windows.Forms.Design.FileNameEditor</remarks>
    public class SaveLogNameEditor : UITypeEditor
    {
        private SaveFileDialog dlg;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null && (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService)) != null)
            {
                var initialDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                if (this.dlg == null)
                {
                    this.dlg = new SaveFileDialog();
                    dlg.InitialDirectory = initialDirectory;
                    dlg.ValidateNames = true;
                    dlg.Title = "Select a Log filename";
                    dlg.Filter = "Text Log (*.log)|*.log|CSV Log (*.csv)|*.csv|XML Log (*.xml)|*.xml|Plain Text (*.txt)|*.txt|All Files (*.*)|*.*";
                    dlg.FilterIndex = 1;
                    dlg.DefaultExt = "*.log";
                    dlg.Title = "Select a log file to Save into";
                }

                string fn = value as string ?? "";

                if (string.IsNullOrWhiteSpace(fn)) dlg.FilterIndex = 1;
                else
                {
                    switch (Path.GetExtension(fn).ToLower())
                    {
                        case ".log": dlg.FilterIndex = 1; break;
                        case ".csv": dlg.FilterIndex = 2; break;
                        case ".xml": dlg.FilterIndex = 3; break;
                        case ".txt": dlg.FilterIndex = 4; break;
                        default: dlg.FilterIndex = 5; break;
                    }
                }

                this.dlg.FileName = fn;

                if (this.dlg.ShowDialog() == DialogResult.OK)
                {
                    value = this.dlg.FileName.Replace(initialDirectory+"\\","");
                }
            }

            return value;
        }
    }
}
