using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    /// <summary>
    ///  Represents a common dialog box that enables the user to define custom colors.
    /// </summary>
    /// <remarks>
    /// Has the exact same interface as System.Windows.Forms.ColorDialog.
    /// </remarks>
    public class SysColorPickerDialog : ColorDialog
    {
        [Flags]
        private enum CC
        {
            RGBINIT = 0x00000001,  //Causes the dialog box to use the color specified in the rgbResult member as the initial color selection.
            FULLOPEN = 0x00000002,  //Causes the dialog box to display the additional controls that allow the user to create custom colors. If this flag is not set, the user must click the Define Custom Color button to display the custom color controls.
            PREVENTFULLOPEN = 0x00000004,  //Disables the Define Custom Color button.
            SHOWHELP = 0x00000008,  //Causes the dialog box to display the Help button. The hwndOwner member must specify the window to receive the HELPMSGSTRING registered messages that the dialog box sends when the user clicks the Help button.
            ENABLEHOOK = 0x00000010,  //Enables the hook procedure specified in the lpfnHook member of this structure. This flag is used only to initialize the dialog box.
            ENABLETEMPLATE = 0x00000020,  //The hInstance and lpTemplateName members specify a dialog box template to use in place of the default template. This flag is used only to initialize the dialog box.
            ENABLETEMPLATEHANDLE = 0x00000040,  //The hInstance member identifies a data block that contains a preloaded dialog box template. The system ignores the lpTemplateName member if this flag is specified. This flag is used only to initialize the dialog box.
            SOLIDCOLOR = 0x00000080,  //Causes the dialog box to display only solid colors in the set of basic colors.
            ANYCOLOR = 0x00000100,  //Causes the dialog box to display all available colors in the set of basic colors.
        }
        //RunDialog(IntPtr hwndOwner) = 17 = 0x0011 = (int)(CC.ENABLEHOOK | CC.RGBINIT)

        private const int COLOR_HUE = 0x02BF;  //Control ID's
        private const int COLOR_SAT = 0x02C0;
        private const int COLOR_LUM = 0x02C1;
        private const int COLOR_RED = 0x02C2;
        private const int COLOR_GREEN = 0x02C3;
        private const int COLOR_BLUE = 0x02C4;
        private const int COLOR_ADD = 0x02C8;
        private const int COLOR_MIX = 0x02CF;
        private IntPtr hInstance;

        public SysColorPickerDialog()
        {
            Stream manifestResourceStream = typeof(System.Drawing.Design.ColorEditor).Module.Assembly.GetManifestResourceStream(typeof(System.Drawing.Design.ColorEditor), "colordlg.data");
            int length = (int)(manifestResourceStream.Length - manifestResourceStream.Position);
            byte[] numArray = new byte[length];
            manifestResourceStream.Read(numArray, 0, length);
            this.hInstance = Marshal.AllocHGlobal(length);
            Marshal.Copy(numArray, 0, this.hInstance, length);
        }

        protected override IntPtr Instance => this.hInstance;

        protected override int Options => (int)(CC.ENABLETEMPLATEHANDLE | CC.FULLOPEN); //== 66 == 0x42

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!(this.hInstance != IntPtr.Zero))
                    return;
                Marshal.FreeHGlobal(this.hInstance);
                this.hInstance = IntPtr.Zero;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        protected override IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_INITDIALOG:
                    SendDlgItemMessage(hwnd, COLOR_HUE, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, 0);
                    SendDlgItemMessage(hwnd, COLOR_SAT, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, 0);
                    SendDlgItemMessage(hwnd, COLOR_LUM, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, 0);
                    SendDlgItemMessage(hwnd, COLOR_RED, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, 0);
                    SendDlgItemMessage(hwnd, COLOR_GREEN, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, 0);
                    SendDlgItemMessage(hwnd, COLOR_BLUE, EM_SETMARGINS, EC_LEFTMARGIN | EC_RIGHTMARGIN, 0);
                    IntPtr dlgItem1 = GetDlgItem(hwnd, COLOR_MIX);
                    EnableWindow(dlgItem1, false);
                    SetWindowPos(dlgItem1, IntPtr.Zero, 0, 0, 0, 0, SWP_HIDEWINDOW);
                    IntPtr dlgItem2 = GetDlgItem(hwnd, 1);
                    EnableWindow(dlgItem2, false);
                    SetWindowPos(dlgItem2, IntPtr.Zero, 0, 0, 0, 0, SWP_HIDEWINDOW);
                    this.Color = Color.Empty;
                    break;
                case WM_COMMAND:
                    if ((((int)wParam) & 0xFFFF) == COLOR_ADD)
                    {
                        bool x; //translated flag unused
                        this.Color = Color.FromArgb(GetDlgItemInt(hwnd, COLOR_RED, out x, false), GetDlgItemInt(hwnd, COLOR_GREEN, out x, false), GetDlgItemInt(hwnd, COLOR_BLUE, out x, false));
                        PostMessage(hwnd, WM_COMMAND, (IntPtr)1, GetDlgItem(hwnd, 1));
                        break;
                    }
                    break;
            }
            return base.HookProc(hwnd, msg, wParam, lParam);
        }

        #region Win32
        private const int WM_INITDIALOG = 0x0110;
        private const int WM_COMMAND = 0x0111;
        private const int EC_LEFTMARGIN = 0x0001;
        private const int EC_RIGHTMARGIN = 0x0001;
        private const int EC_USEFONTINFO = 0xFFFF;
        private const int EM_SETMARGINS = 0x00D3;
        private const int EM_GETMARGINS = 0x00D4;
        private const int SWP_HIDEWINDOW = 0x0080;

        [DllImport("user32.dll")] static extern IntPtr SendDlgItemMessage(IntPtr hDlg, int nIDDlgItem, int Msg, UIntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] static extern IntPtr SendDlgItemMessage(IntPtr hDlg, int nIDDlgItem, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);
        [DllImport("user32.dll")] static extern int GetDlgItemInt(IntPtr hDlg, int nIDDlgItem, out bool lpTranslated, bool bSigned);
        [DllImport("user32.dll")] static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        [DllImport("user32.dll")] static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        #endregion Win32
    }
}
