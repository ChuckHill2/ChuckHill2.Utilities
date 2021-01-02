using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChuckHill2.Utilities.Extensions;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Displays a small message box in front of the specified object and with the specified text, caption, buttons, and icon.
    /// </summary>
    /// <remarks>
    /// Functionally equivalant to MessageBoxEx except it is much smaller and more discreet.
    /// Plus it may be used modalless (i.e. returns immediately).
    /// </remarks>
    public class MiniMessageBox : Form
    {
        [ThreadStatic] private static MiniMessageBox MMDialog = null;
        [ThreadStatic] private static DialogResult MMResult = DialogResult.None;
        private Icon CaptionIcon = GetAppIcon();
        private Font CaptionFont;
        private Image MessageIcon;
        private string MessageIconString; //for clipboard
        private string Message;
        private string Caption;
        private string[] ButtonNames;
        private bool IsModal;
        private Rectangle rcCaptionIcon; //painted positions on form
        private Rectangle rcCaption;
        private Rectangle rcMessageIcon;
        private Rectangle rcMessage;

        /// <summary>
        /// Displays a tiny modal (i.e. waits) message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        /// <remarks>This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main</remarks>
        public static DialogResult ShowDialog(IWin32Window owner, string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            using(var dlg = new MiniMessageBox(true, text, caption, buttons, icon))
            {
                return dlg.ShowDialog(GetOwner(owner));
            }
        }

        /// <summary>
        /// Displays a tiny modalless (i.e. returns immediately) message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box OR ((MessageBoxButtons) -1) for no buttons. In which case, the calling code is responsible for closing the dialog via Hide().</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <remarks>
        /// This is thread static. Meaning it must be closed within the context of the same thread. See Hide().
        /// This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main
        /// </remarks>
        public static void Show(IWin32Window owner, string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            MMDialog = new MiniMessageBox(false, text, caption, buttons, icon);
            MMDialog.Show(GetOwner(owner));
        }

        /// <summary>
        /// Closes a modalless dialog if it is not already closed.
        /// Only valid within the context of the thread that called Show().
        /// </summary>
        /// <returns>The final Dialog result value of the closed modalless dialog. Only meaningful for a modalless dialog. If dialog closed by user via a MessagBoxButton, this will return the last closing value.</returns>
        public static new DialogResult Hide()
        {
            if (MMDialog !=null)
            {
                MMDialog.Close();
                MMDialog.Dispose();
                MMDialog = null;
                MMResult = DialogResult.None;
                return MMResult;
            }
            else
            {
                return MMResult;
            }
        }

        private static void StaticButtonClick(object sender, EventArgs e)
        {
            //This is for Modalless dialogs that return immediately upon calling Show()
            Button btn = (Button)sender;
            switch(btn.Name)
            {
                case "OK": MMResult = DialogResult.OK; break;
                case "Cancel": MMResult = DialogResult.Cancel; break;
                case "Abort": MMResult = DialogResult.Abort; break;
                case "Retry": MMResult = DialogResult.Retry; break;
                case "Ignore": MMResult = DialogResult.Ignore; break;
                case "Yes": MMResult = DialogResult.Yes; break;
                case "No": MMResult = DialogResult.No; break;
                default: MMResult = DialogResult.None; break;
            }

            MMDialog.Close();
            MMDialog.Dispose();
            MMDialog = null;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            //This is for Modal dialogs that hang until user presses a button via ShowDialog()
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "OK": this.DialogResult = DialogResult.OK; break;
                case "Cancel": this.DialogResult = DialogResult.Cancel; break;
                case "Abort": this.DialogResult = DialogResult.Abort; break;
                case "Retry": this.DialogResult = DialogResult.Retry; break;
                case "Ignore": this.DialogResult = DialogResult.Ignore; break;
                case "Yes": this.DialogResult = DialogResult.Yes; break;
                case "No": this.DialogResult = DialogResult.No; break;
                default: this.DialogResult = DialogResult.None; break;
            }

            this.Close();
        }

        //private constructor
        private MiniMessageBox(bool isModal, string msg, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "MiniMessageBox";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "MiniMessageBox";
            this.ResumeLayout(false);

            // With modalless dialogs, the calling code has control when to close the dialog via Hide(); but modal dialogs
            // need an action for the user to close the dialog as the calling code must wait. Thus a valid enum value is required.
            if (isModal && (int)buttons < 0 || (int)buttons > 5)
            {
                buttons = MessageBoxButtons.OK;
            }

            IsModal = isModal;
            CaptionFont = new Font(this.Font, FontStyle.Regular);
            MessageIcon = GetMessageIcon(icon, out MessageIconString);
            Message = string.IsNullOrWhiteSpace(msg) ? null : msg.Trim();
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
            ButtonNames = GetButtonNames(buttons);
        }

        protected override void OnShown(EventArgs e)
        {
            this.SuspendLayout();
            const int Spacing = 3;
            Size szButton = Size.Empty;
            Size szButtonGroup = Size.Empty;
            if (ButtonNames.Length > 0)
            {
                szButton = new Size(ButtonNames.Max(s => ComputeTextDimensions(null, s, this.Font, int.MaxValue).Width) + 10, ComputeTextDimensions(null, "H", this.Font, int.MaxValue).Height + 10);
                szButtonGroup = new Size(ButtonNames.Length * (szButton.Width + Spacing) - Spacing, szButton.Height);
            }

            Size szCaptionIcon = Size.Empty;
            Size szCaption = Size.Empty;
            if (Caption != null)
            {
                szCaptionIcon = CaptionIcon == null ? new Size(0, 0) : CaptionIcon.Size;
                szCaption = string.IsNullOrEmpty(Caption) ? Size.Empty : ComputeTextDimensions(null, Caption, CaptionFont, int.MaxValue);
                if (szCaptionIcon.Height > szCaption.Height) szCaption.Height = szCaptionIcon.Height;
            }

            var szMessageIcon = MessageIcon == null ? Size.Empty : MessageIcon.Size;
            Size szMessage = new Size(0, szMessageIcon.Height);
            if (Message != null)
            {
                szMessage = ComputeTextDimensions(null, Message, this.Font);
                int max = Math.Max(Math.Max(szButtonGroup.Width, szCaptionIcon.Width + Spacing + szCaption.Width), szMessageIcon.Width + Spacing + szMessage.Width);
                szMessage = ComputeTextDimensions(null, Message, this.Font, max - szMessageIcon.Width - Spacing);
                if (szMessageIcon.Height > szMessage.Height) szMessage.Height = szMessageIcon.Height;
            }

            Width = Math.Max(Math.Max(szButtonGroup.Width, szCaptionIcon.Width + Spacing + szCaption.Width), szMessageIcon.Width + Spacing + szMessage.Width);
            Height = szMessage.Height;
            if (Caption != null) Height += szCaption.Height + Spacing;
            if (ButtonNames.Length > 0) Height += szButtonGroup.Height + Spacing;

            Width += (Spacing + 1) * 2;  //Add border spacing
            Height += (Spacing + 1) * 2;

            rcCaptionIcon = new Rectangle(Spacing + 1, 1, szCaptionIcon.Width, szCaptionIcon.Height);
            rcCaption = new Rectangle(rcCaptionIcon.Right + Spacing, rcCaptionIcon.Top, szCaption.Width, szCaption.Height);
            rcMessageIcon = new Rectangle(rcCaptionIcon.Left, rcCaption.Bottom + Spacing + 1, szMessageIcon.Width, szMessageIcon.Height);
            rcMessage = new Rectangle((MessageIcon == null ? rcCaptionIcon.Left : rcMessageIcon.Right + Spacing), rcMessageIcon.Top, szMessage.Width, szMessage.Height);

            int xOffset = (Width - szButtonGroup.Width) / 2;
            foreach (var buttonName in ButtonNames)
            {
                var btn = new Button();
                btn.Name = buttonName;
                btn.Text = buttonName;
                btn.Size = szButton;
                btn.Location = new Point(xOffset, rcMessage.Bottom + Spacing);
                xOffset += szButton.Width + Spacing;
                if (IsModal) btn.Click += ButtonClick;
                else btn.Click += StaticButtonClick;
                this.Controls.Add(btn);
            }

            Rectangle ownerBounds = this.Owner == null ? Screen.FromControl(this).WorkingArea : this.Owner.DesktopBounds;
            Rectangle ownerClientRetangle = this.Owner == null ? Screen.FromControl(this).WorkingArea : this.Owner.ClientRectangle;
            this.Location = new Point(
                (ownerBounds.Width - this.DesktopBounds.Width) / 2 + ownerBounds.X,
                (ownerBounds.Height - this.DesktopBounds.Height) / 2 + ownerBounds.Y - ownerClientRetangle.Height / 6);

            this.ResumeLayout(false);
            base.OnShown(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (CaptionFont !=null)
            {
                CaptionFont.Dispose();
                CaptionFont = null;
            }

            if (CaptionIcon != null)
            {
                CaptionIcon.Dispose();
                CaptionIcon = null;
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

            base.OnPaint(e);
            if (Caption != null)
            {
                var rc = new RectangleF(0, 0, this.Size.Width, rcCaption.Bottom);
                using (var br = new LinearGradientBrush(rc, SystemColors.ActiveCaption, SystemColors.GradientActiveCaption, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(br, rc);
                if (CaptionIcon != null)
                {
                    e.Graphics.DrawIcon(CaptionIcon, rcCaptionIcon);
                    //e.Graphics.DrawRectangle(Pens.Red, rcCaptionIcon.X, rcCaptionIcon.Y, rcCaptionIcon.Width - 1, rcCaptionIcon.Height - 1);
                }

                TextRenderer.DrawText(e.Graphics, Caption, CaptionFont, rcCaption, SystemColors.ActiveCaptionText, Color.Transparent, flags);
                //e.Graphics.DrawRectangle(Pens.Red, rcCaption.X, rcCaption.Y, rcCaption.Width - 1, rcCaption.Height - 1);

                e.Graphics.DrawLine(SystemPens.ActiveBorder, 0, rc.Bottom, rc.Right, rc.Bottom);
            }

            if (MessageIcon !=null)
            {
                e.Graphics.DrawImage(MessageIcon, rcMessageIcon);
                //e.Graphics.DrawRectangle(Pens.Red, rcMessageIcon.X, rcMessageIcon.Y, rcMessageIcon.Width-1, rcMessageIcon.Height-1);

            }
            if (Message != null)
            {
                TextRenderer.DrawText(e.Graphics, Message, this.Font, rcMessage, SystemColors.ActiveCaptionText, Color.Transparent, flags);
                //e.Graphics.DrawRectangle(Pens.Red, rcMessage.X, rcMessage.Y, rcMessage.Width - 1, rcMessage.Height - 1);
            }

            e.Graphics.DrawRectangle(SystemPens.ActiveBorder, 0, 0, this.Width - 1, this.Height - 1);
        }

        //Copy n' Paste content to the clipboard
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == (Keys.C|Keys.Control))
                Clipboard.SetData(DataFormats.Text, $"{(Caption==null?"":Caption+"\r\n")}{(MessageIcon==null?"": MessageIconString+" ")}{Message??""}");

            base.OnKeyUp(e);
        }

        #region Click n'Drag this Form
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            base.OnMouseDown(e);
        }
        #endregion //  Click n'Drag this Form

        private static string[] GetButtonNames(MessageBoxButtons b)
        {
            switch (b)
            {
                case MessageBoxButtons.OK: return new string[] { "OK" };
                case MessageBoxButtons.OKCancel: return new string[] { "OK", "Cancel" };
                case MessageBoxButtons.AbortRetryIgnore: return new string[] { "Abort", "Retry", "Ignore" };
                case MessageBoxButtons.YesNoCancel: return new string[] { "Yes", "No", "Cancel" };
                case MessageBoxButtons.YesNo: return new string[] { "Yes", "No", };
                case MessageBoxButtons.RetryCancel: return new string[] { "Retry", "Cancel" };
                default: return new string[0];
            }
        }

        private static Image GetMessageIcon(MessageBoxIcon icon, out string iconString)
        {
            // 'iconString' is for pasting into the clipboard.
            switch (icon)
            {
                case MessageBoxIcon.Error: iconString = "[Error]"; return global::ChuckHill2.Utilities.Properties.Resources.error24;
                case MessageBoxIcon.Question: iconString = "[Question]"; return global::ChuckHill2.Utilities.Properties.Resources.question24;
                case MessageBoxIcon.Warning: iconString = "[Warning]"; return global::ChuckHill2.Utilities.Properties.Resources.warning24;
                case MessageBoxIcon.Information: iconString = "[Information]"; return global::ChuckHill2.Utilities.Properties.Resources.info24;
                default: iconString = ""; return null;
            }
        }

        private static Icon GetAppIcon()
        {
            //These icons must be disposed after use.
            Icon ico = null;
            FormCollection fc = System.Windows.Forms.Application.OpenForms;
            if (fc != null && fc.Count > 0) ico = fc[0].Icon == null ? null : new Icon(fc[0].Icon, SystemInformation.SmallIconSize.Width, SystemInformation.SmallIconSize.Height);
            if (ico == null) ico = GDI.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, 16);
            return ico;
        }

        /// <summary>
        /// Compute the dimensions of 3x2 rectangle box that will fit the supplied text.
        /// </summary>
        /// <param name="graphics">Graphics object used to measure string. Or NULL to use legacy TextRenderer (necessary to mirror Forms.Label control wrapping).</param>
        /// <param name="s">Supplied text. If string contains newlines, autowrap is disabled.</param>
        /// <param name="font">Font to use for measuring.</param>
        /// <param name="maxWidth"> If 0, then autowrap and fit text into a 3x2 rectangle. Should not contain newlines. Else autowrap at this width.</param>
        /// <returns>size of fitted box.</returns>
        private static Size ComputeTextDimensions(Graphics graphics, string s, Font font, int maxWidth = 0)
        {
            const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            Func<float, int> ceil = (f) => (int)(f > (int)f ? f + 1 : f);

            if (s == null && s.Length == 0) return Size.Empty;

            if (s.Any(c => c == '\n')) maxWidth = int.MaxValue; //We don't autowrap if the string contains newlines.

            if (maxWidth > 0)
            {
                if (graphics != null) return Size.Ceiling(graphics.MeasureString(s, font, new SizeF(maxWidth, int.MaxValue), (StringFormat)null));
                else return TextRenderer.MeasureText(s, font, new Size(maxWidth, int.MaxValue), flags);
            }

            int width = 50;
            SizeF sizef = SizeF.Empty;
            SizeF prevsizef = SizeF.Empty;
            double ratio = 0;

            while (ratio < 3)
            {
                if (graphics != null) sizef = graphics.MeasureString(s, font, width);
                else sizef = TextRenderer.MeasureText(s, font, new Size(width, 99999), flags);
                if (sizef == prevsizef) break;
                ratio = sizef.Width / (double)sizef.Height;
                width += 25;
                prevsizef = sizef;
            }

            width = ceil(sizef.Width);
            for (int i = 0; i < 50; i++)
            {
                width--;
                SizeF size2;
                if (graphics != null) size2 = graphics.MeasureString(s, font, width);
                else size2 = TextRenderer.MeasureText(s, font, new Size(width, 99999), flags);
                if (size2.Height > sizef.Height) break;
            }

            return new Size(width + 1, ceil(sizef.Height));
        }

        private static IWin32Window GetOwner(IWin32Window owner)
        {
            //Bullit-proof that we have an owner. Default null == desktop.
            if (owner == null) owner = System.Windows.Forms.Form.ActiveForm;
            if (owner == null)
            {
                FormCollection fc = System.Windows.Forms.Application.OpenForms;
                if (fc != null && fc.Count > 0) owner = fc[0];
            }
            return owner;
        }
    }
}
