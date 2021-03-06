//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="StatusWindow.cs" company="Chuck Hill">
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
using System.IO;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Friendly scrolling status window. String data that exceeds user-specified MaxLength
    /// is deleted to minimize the memory footprint. Supports multi-threaded environments.
    /// </summary>
    [ToolboxBitmap(typeof(System.Windows.Forms.TextBox))]
    public class StatusWindow : System.Windows.Forms.TextBox
    {
        /// <summary>
        /// Open Stream/File to write output to.
        /// The status window only holds so much text and eventually scrolls off (see this.MaxLength).
        /// This allows ALL the output to be captured.
        /// </summary>
        public TextWriter AltOutput = null;

        /// <summary>
        /// Friendly scrolling status window. String data that exceeds user-specified MaxLength
        /// is deleted to minimize the memory footprint. Supports multi-threaded environments.
        /// Initializes a new instance of the ChuckHill2.StatusWindow class.
        /// </summary>
        public StatusWindow() : base()
        {
            base.Multiline = true;
            base.ScrollBars = System.Windows.Forms.ScrollBars.Both;
        }

        #region Hide Unused TextBox Properties from Designer
        //! @cond DOXYGENHIDE
        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool Multiline { get { return base.Multiline; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new System.Windows.Forms.ScrollBars ScrollBars { get { return base.ScrollBars; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AcceptsReturn { get { return base.AcceptsReturn; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoCompleteStringCollection AutoCompleteCustomSource { get { return base.AutoCompleteCustomSource; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoCompleteMode AutoCompleteMode { get { return base.AutoCompleteMode; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AutoCompleteSource AutoCompleteSource { get { return base.AutoCompleteSource; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new CharacterCasing CharacterCasing { get { return base.CharacterCasing; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new char PasswordChar { get { return base.PasswordChar; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool UseSystemPasswordChar { get { return base.UseSystemPasswordChar; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool HideSelection { get { return base.HideSelection; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string[] Lines  { get { return base.Lines; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ContextMenuStrip ContextMenuStrip  { get { return base.ContextMenuStrip; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool AllowDrop { get { return base.AllowDrop; } set { } }

        /// <summary> This is not used.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ControlBindingsCollection DataBindings { get { return base.DataBindings; } }
        //! @endcond
        #endregion

        private delegate void StatusWindowCallback(string format, params object[] args);
        /// <summary>
        /// Append a formatted string to the existing text in the status window.
        /// May be safely called from another thread.
        /// </summary>
        /// <param name="format">A string.Format-like composite format string.</param>
        /// <param name="args">zero or more values used by the format string</param>
        public void AppendText(string format, params object[] args)
        {
            if (base.InvokeRequired)  //some events occur on another thread!
            {
                base.BeginInvoke(new StatusWindowCallback(AppendText), new object[] { format, args });
                return;
            }
            if (this.IsDisposed) return;

            string s = string.Format(format,args); //Safe Format

            if (AltOutput != null)
            {
                try
                {
                    AltOutput.WriteLine(s);
                    AltOutput.Flush();
                }
                catch { AltOutput = null; }
            }

            int maxLength = base.MaxLength;
            if (((s.Length + 1) + base.TextLength) > maxLength)  //remove oldest text from window
            {
                //int line = base.GetLineFromCharIndex(maxLength/2);  //inefficient
                //int index = base.GetFirstCharIndexFromLine(line + 1); //round up to remove whole lines.
                base.SelectionStart = 0;
                base.SelectionLength = maxLength / 2;
                base.SelectedText = string.Empty;
                //base.Text = base.Text.Substring(index); //avoid retrieving string
            }
            //int i = (base.SelectionStart >= base.Text.Length ? base.MaxLength : base.SelectionStart);
            base.SelectionStart = maxLength;
            base.AppendText(s);
            if (s.Length<1 || s[s.Length - 1] != '\n') base.AppendText(Environment.NewLine);
            //base.SelectionStart = i;
            //base.ScrollToCaret();
        }
        /// <summary>
        /// Appends text to the current text of a text box.
        /// May be safely called from another thread.
        /// </summary>
        /// <param name="text">The text to append to the current contents of the text box.</param>
        public new void AppendText(string text)
        {
            this.AppendText(text, (object)null);
        }

    }
}
