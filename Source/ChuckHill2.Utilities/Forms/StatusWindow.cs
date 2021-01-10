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
        [Browsable(false)] public override bool Multiline { get { return base.Multiline; } set { } }
        [Browsable(false)] public new System.Windows.Forms.ScrollBars ScrollBars { get { return base.ScrollBars; } set { } }
        [Browsable(false)] public new bool AcceptsReturn { get { return base.AcceptsReturn; } set { } }
        [Browsable(false)] public new AutoCompleteStringCollection AutoCompleteCustomSource { get { return base.AutoCompleteCustomSource; } set { } }
        [Browsable(false)] public new AutoCompleteMode AutoCompleteMode { get { return base.AutoCompleteMode; } set { } }
        [Browsable(false)] public new AutoCompleteSource AutoCompleteSource { get { return base.AutoCompleteSource; } set { } }
        [Browsable(false)] public new CharacterCasing CharacterCasing { get { return base.CharacterCasing; } set { } }
        [Browsable(false)] public new char PasswordChar { get { return base.PasswordChar; } set { } }
        [Browsable(false)] public new bool UseSystemPasswordChar { get { return base.UseSystemPasswordChar; } set { } }
        [Browsable(false)] public new bool HideSelection { get { return base.HideSelection; } set { } }
        [Browsable(false)] public new string[] Lines  { get { return base.Lines; } set { } }
        [Browsable(false)] public new ContextMenuStrip ContextMenuStrip  { get { return base.ContextMenuStrip; } set { } }
        [Browsable(false)] public new bool AllowDrop { get { return base.AllowDrop; } set { } }
        [Browsable(false)] public new ControlBindingsCollection DataBindings { get { return base.DataBindings; } }
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
