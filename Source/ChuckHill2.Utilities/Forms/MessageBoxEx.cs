using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Displays a system message box in front of the specified object and with the specified text, caption, buttons, and icon.
    /// </summary>
    /// <remarks>
    /// This is a wrapper for System.Windows.Forms.MessageBox which in turn is a wrapper for Win32 [User32] MessageBox.<br />
    /// The problem with the standard system MessageBox is that it is always centered over the desktop, has no icon in the title bar, and does not handle multi-threading.<br />
    /// * This extended system MessageBox is centered over the owner window. If the owner is not specified, MessageBox is centered over the application window.<br />
    /// * If the parent or application has an icon, then it is also displayed in the caption bar.<br />
    /// * The ability to capture the messagebox content is available via a copy menu item over the messagebox system icon.<br />
    /// * This is also multi-threaded compliant. By definition MessageBox is modal and no more than one instance can be displayed at a time. However, if called from another thread, this will block until the first messagebox is closed.
    /// </remarks>
    public static class MessageBoxEx
    {
        private static readonly HookProc _hookProc = new HookProc(MessageBoxHookProc); //this MUST be static so it won't get garbage collected!
        private static readonly WndProc _wndProc = new WndProc(MessageBoxWindowProc); //this MUST be static so it won't get garbage collected!
        private static readonly System.Drawing.Icon _ico = GetAppIcon();

        [ThreadStatic] private static IntPtr _owner;
        [ThreadStatic] private static IntPtr _hHook = IntPtr.Zero;

        private delegate DialogResult ShowDelegate(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon); //for executing on the owner window thread

        /// <summary>
        /// Displays a message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box.</param>
        /// <param name="text">The text to display in the message box.</param>
        /// <param name="caption">The text to display in the title bar of the message box.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">buttons is not a member of System.Windows.Forms.MessageBoxButtons. -or- icon is not a member of System.Windows.Forms.MessageBoxIcon. -or- defaultButton is not a member of System.Windows.Forms.MessageBoxDefaultButton.</exception>
        /// <exception cref="System.InvalidOperationException">An attempt was made to display the System.Windows.Forms.MessageBox in a process that is not running in User Interactive mode.This is specified by the System.Windows.Forms.SystemInformation.UserInteractive property.</exception>
        public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            MessageBoxDefaultButton defaultButton; //always use the last button in MessageBox as the default.
            switch(buttons)
            {
                case MessageBoxButtons.OK: defaultButton = MessageBoxDefaultButton.Button1; break;
                case MessageBoxButtons.OKCancel: defaultButton = MessageBoxDefaultButton.Button2; break;
                case MessageBoxButtons.AbortRetryIgnore: defaultButton = MessageBoxDefaultButton.Button3; break;
                case MessageBoxButtons.YesNoCancel: defaultButton = MessageBoxDefaultButton.Button3; break;
                case MessageBoxButtons.YesNo: defaultButton = MessageBoxDefaultButton.Button2; break;
                case MessageBoxButtons.RetryCancel: defaultButton = MessageBoxDefaultButton.Button2; break;
                default: defaultButton = MessageBoxDefaultButton.Button1; break;
            }

            owner = Initialize(owner);
            if (owner == null)
            {
                return MessageBox.Show(text, caption, buttons, icon, defaultButton);
            }
            else if (owner is IWin32Window)
            {
                Control c = owner as Control;
                if (c != null && c.InvokeRequired)  //Oops! we are not running on the owner control thread
                    return (DialogResult)c.Invoke(new ShowDelegate(Show), owner, text, caption, buttons, icon);
                return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
            }
            else
            {
                return MessageBox.Show(text, caption, buttons, icon, defaultButton);
            }
        }

        #region -= Win32 API =-
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private const int WH_CALLWNDPROCRET = 12;
        private enum CbtHookAction : int
        {
            HCBT_MOVESIZE = 0,
            HCBT_MINMAX = 1,
            HCBT_QS = 2,
            HCBT_CREATEWND = 3,
            HCBT_DESTROYWND = 4,
            HCBT_ACTIVATE = 5,
            HCBT_CLICKSKIPPED = 6,
            HCBT_KEYSKIPPED = 7,
            HCBT_SYSCOMMAND = 8,
            HCBT_SETFOCUS = 9
        }
        [StructLayout(LayoutKind.Sequential)] private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        } ;
        private const UInt32 WM_SETICON = 0x0080;
        private static readonly IntPtr ICON_SMALL = IntPtr.Zero;
        private static readonly IntPtr ICON_BIG = new IntPtr(1);
        private const int GWL_WNDPROC = -4;
        private const UInt32 WM_DESTROY = 0x0002;
        private const UInt32 WM_INITDIALOG = 0x0110;
        private const UInt32 WM_GETTEXT = 0x000D;
        private const UInt32 BM_CLICK = 0x00F5;
        private const Int32 WM_SYSCOMMAND = 0x112;
        private const Int32 MF_SEPARATOR = 0x800;
        private const Int32 MF_BYPOSITION = 0x400;
        private const Int32 MF_STRING = 0x0;
        private const Int32 IDM_CLIPBOARD = 1000;
        //private const Int32 STM_GETICON = 0x0171;
        //private const Int32 WM_KEYUP = 0x0101;
        //private const Int32 WM_CHAR = 0x0102;
        //private const Int32 WM_HOTKEY = 0x0312;

        [DllImport("kernel32.dll", SetLastError=true)] private static extern int GetCurrentThreadId();
        [DllImport("user32.dll", SetLastError=true)] private static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);
        [DllImport("user32.dll", SetLastError=true)] private static extern int MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", SetLastError=true)] private static extern bool UnhookWindowsHookEx(IntPtr idHook);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)] private static extern IntPtr GetProp(IntPtr hWnd, string lpString);
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Unicode)] private static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr hData);
        [DllImport("user32.dll", SetLastError=true)] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", SetLastError=true, EntryPoint="GetWindowLongPtr")] private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", SetLastError=true)] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int newValue);
        [DllImport("user32.dll", SetLastError=true, EntryPoint="SetWindowLongPtr")] private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newValue);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError=true)] private static extern int GetMenuItemCount(IntPtr hMenu);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll", SetLastError=true)] private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)] private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", SetLastError=true)] private static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr GetDlgItem(IntPtr hParentWnd, int nIDDlgItem);
        [DllImport("user32.dll", SetLastError=true)] private static extern IntPtr SendDlgItemMessage(IntPtr hWnd, int nIDDlgItem, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)] private static extern int GetWindowText(IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpString, int nMaxCount);
        private static string GetWindowText(IntPtr hWnd, int nIDDlgItem=-1)
        {
            StringBuilder sb = new StringBuilder(512);
            if (nIDDlgItem != -1)
            {
                hWnd = GetDlgItem(hWnd, nIDDlgItem);
                if (hWnd == IntPtr.Zero) return string.Empty;
            }
            int length = GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }
        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            //hack: User32.dll:GetWindowLongPtr() only exists in x64, so we hide this here.
            if (IntPtr.Size > 4) return GetWindowLongPtr64(hWnd, nIndex);
            else return new IntPtr(GetWindowLong(hWnd, nIndex));
        }
        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr value)
        {
            //hack: User32.dll:SetWindowLongPtr() only exists in x64, so we hide this here.
            if (IntPtr.Size > 4) return SetWindowLongPtr64(hWnd, nIndex, value);
            else return new IntPtr(SetWindowLong(hWnd, nIndex, value.ToInt32()));
        }
        #endregion -= Win32 API =-

        private static System.Drawing.Icon GetAppIcon()
        {
            System.Drawing.Icon ico = null;
            FormCollection fc = System.Windows.Forms.Application.OpenForms;
            if (fc != null && fc.Count > 0) ico = fc[0].Icon;   //private field fc[0].smallIcon  may be better because we don't have to resize
            if (ico == null) ico = System.Drawing.Icon.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            if (ico != null) ico = new System.Drawing.Icon(ico, SystemInformation.SmallIconSize.Width, SystemInformation.SmallIconSize.Height);
            return ico;
        }

        private static IWin32Window Initialize(IWin32Window owner)
        {
            _owner = IntPtr.Zero;
            if (owner == null)
            {
                owner = System.Windows.Forms.Form.ActiveForm;
            }
            if (owner == null)
            {
                FormCollection fc = System.Windows.Forms.Application.OpenForms;
                if (fc != null && fc.Count > 0) owner = fc[0];
            }
            if (owner == null) { return null; }

            Control c = owner as Control;
            if (c != null && c.InvokeRequired) { return owner; }
            _owner = owner.Handle;

            if (_owner == IntPtr.Zero) { return null; }

            //Temporarily block during MessageBox repositioning only.
            //It's released before the MessageBox is displayed.
            //This is necessary for multi-threaded calls.

            _hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _hookProc, IntPtr.Zero, GetCurrentThreadId());

            return owner;
        }

        private static IntPtr MessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hHook, nCode, wParam, lParam);

            CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            IntPtr hook = _hHook;
            if (msg.message == (int)CbtHookAction.HCBT_ACTIVATE)
            {
                try
                {
                    if (_ico != null) SendMessage(msg.hwnd, WM_SETICON, ICON_SMALL, _ico.Handle);
                    CenterWindow(msg.hwnd);
                    SubClass(msg.hwnd);
                }
                catch { /* If it bombs, we just quietly let it use the plain default MessageBox */ }
                finally { UnhookWindowsHookEx(_hHook); }
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }

        private static void CenterWindow(IntPtr hChildWnd)
        {
            Rectangle recChild = new Rectangle(0, 0, 0, 0);
            bool success = GetWindowRect(hChildWnd, ref recChild);

            int width = recChild.Width - recChild.X;
            int height = recChild.Height - recChild.Y;

            Rectangle recParent = new Rectangle(0, 0, 0, 0);
            success = GetWindowRect(_owner, ref recParent);

            Rectangle recDesktop = Screen.GetWorkingArea(recParent);

            System.Drawing.Point ptCenter = new System.Drawing.Point(0, 0);
            ptCenter.X = recParent.X + ((recParent.Width - recParent.X) / 2);
            ptCenter.Y = recParent.Y + ((recParent.Height - recParent.Y) / 2);

            System.Drawing.Point ptStart = new System.Drawing.Point(0, 0);
            ptStart.X = (ptCenter.X - (width / 2));
            //ptStart.Y = (ptCenter.Y - (height / 2));
            //for some reason, vertical-center does not look as good as positioning at the top third
            ptStart.Y = (ptCenter.Y - (height / 3));

            ptStart.X = (ptStart.X < 0) ? 0 : ptStart.X;
            ptStart.Y = (ptStart.Y < 0) ? 0 : ptStart.Y;

            int right = ptStart.X + width;
            int bottom= ptStart.Y + height;
            ptStart.X = (right  > recDesktop.Width)  ? ptStart.X - (right  - recDesktop.Width)  : ptStart.X;
            ptStart.Y = (bottom > recDesktop.Height) ? ptStart.Y - (bottom - recDesktop.Height) : ptStart.Y;

            int result = MoveWindow(hChildWnd, ptStart.X, ptStart.Y, width, height, false);
        }

        private static void SubClass(IntPtr hWnd)
        {
            IntPtr wndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProc);
            SetProp(hWnd, "MsgBoxEx", SetWindowLongPtr(hWnd, GWL_WNDPROC, wndProcPtr));
        }

        private static IntPtr MessageBoxWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr oldWndProc = GetProp(hWnd, "MsgBoxEx");
            switch (msg)
            {
                case WM_DESTROY:
                    SetWindowLongPtr(hWnd, GWL_WNDPROC, oldWndProc);
                    _hHook = IntPtr.Zero;
                    _owner = IntPtr.Zero;
                    break;
                case WM_INITDIALOG:
                    WndProc_InsertCopyMenuItem(hWnd);
                    break;
                case WM_SYSCOMMAND:
                    if (wParam.ToInt32() != IDM_CLIPBOARD) break;
                    WndProc_Copy2Clip(hWnd);
                    return IntPtr.Zero;
            }
            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }

        private static void WndProc_InsertCopyMenuItem(IntPtr hWnd)
        {
            IntPtr sysMenuHandle = GetSystemMenu(hWnd, false);
            InsertMenu(sysMenuHandle, 0, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(sysMenuHandle, 0, MF_BYPOSITION, IDM_CLIPBOARD, "Copy\tCtrl+C");
        }

        private static void WndProc_Copy2Clip(IntPtr hWnd)
        {
            //IntPtr icon = SendMessage(GetDlgItem(hWnd, 0x0014), STM_GETICON, IntPtr.Zero, IntPtr.Zero);
            //We would like to determine the message severity by retrieving the MessageBox icon but we cannot
            //compare the handle to System.Drawing.SystemIcons.Error.Handle or LoadIcon(IntPtr.Zero,IDI_ERROR)
            //because the handles are all different but refer to the same thing.

            StringBuilder sb = new StringBuilder(GetWindowTextLength(hWnd) + 2);
            SendMessage(hWnd, WM_GETTEXT, sb.Capacity, sb);  //Get caption/title bar string
            sb.Insert(0, '[');  //put title in brackets
            sb.Append(']');
            sb.AppendLine();    //put following text body on the next line
            StringBuilder sb2 = new StringBuilder(GetWindowTextLength(GetDlgItem(hWnd, 0xFFFF)) + 2);
            SendMessage(GetDlgItem(hWnd, 0xFFFF), WM_GETTEXT, sb2.Capacity, sb2);  //Get message body
            sb2.Replace("\r", string.Empty); //In order to create multiple lines in a messagebox or a Label in general, one must
            sb2.Replace("\n", "\r\n");       //use '\n', not '\r\n'. Everywhere else one must use '\r\n', so we fix it here.
            sb.Append(sb2.ToString()); //Munge the title with the body text
            sb.AppendLine();    //add final newline

            System.Windows.Forms.Clipboard.SetData(System.Windows.Forms.DataFormats.Text, sb.ToString());
        }

        #region MessageBox Virtual Button Push
        private static readonly HookProc _waitHookProc = new HookProc(WaitMessageBoxHookProc); //this MUST be static so it won't get garbage collected!
        [ThreadStatic] private static IntPtr _hWaitHook = IntPtr.Zero;
        [ThreadStatic] private static int _buttonId = 0; //Child Class "Button" Id=0x00000002 "Ok"
        [ThreadStatic] private static string _messageboxTitle = string.Empty;

        /// <summary>
        /// Register hook to wait to push messagebox button when it instaniated.
        /// If it is already registered, this API does nothing.
        /// </summary>
        /// <param name="messageboxTitle">Title/Caption of messagebox to wait for. May be only a the first UNIQUE characters of the title.</param>
        /// <param name="button">button to push</param>
        private static void RegisterWaitToPush(string messageboxTitle, DialogResult button)
        {
            if (_hWaitHook != IntPtr.Zero) return; //Already registered!
            switch (button)
            {
                case DialogResult.None:   return; //nothing to do!
                case DialogResult.OK:     _buttonId = 0x0002; break;
                case DialogResult.Cancel: _buttonId = 0x0002; break;
                case DialogResult.Abort:  _buttonId = 0x0002; break;
                case DialogResult.Retry:  _buttonId = 0x0002; break;
                case DialogResult.Ignore: _buttonId = 0x0002; break;
                case DialogResult.Yes:    _buttonId = 0x0002; break;
                case DialogResult.No:     _buttonId = 0x0002; break;
                default:                  return; //nothing to do!
            }
            _messageboxTitle = messageboxTitle;

            _hWaitHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _waitHookProc, IntPtr.Zero, GetCurrentThreadId());
            //if (_hHook == IntPtr.Zero)  System.Diagnostics.Debug.WriteLine($"SetWindowsHookEx: Failed to hook current thread.\n{Win32Exception.GetLastErrorMessage()}");
        }

        /// <summary>
        /// Un-Register hook that waited to push messagebox button when it instaniated.
        /// If it is already unregistered, this API does nothing.
        /// </summary>
        private static void UnregisterWaitToPush()
        {
            if (_hWaitHook == IntPtr.Zero) return;
            bool ok = UnhookWindowsHookEx(_hWaitHook);
            //if (!ok) System.Diagnostics.Debug.WriteLine($"UnhookWindowsHookEx: Failed to unhook current thread.\n{ Win32Exception.GetLastErrorMessage() }");
            _hWaitHook = IntPtr.Zero;
            _buttonId = 0;
            _messageboxTitle = string.Empty;
        }

        private static IntPtr WaitMessageBoxHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hWaitHook, nCode, wParam, lParam);

            CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            IntPtr hook = _hWaitHook;
            if (msg.message == WM_INITDIALOG)
            {
                if (GetWindowText(msg.hwnd).StartsWith(_messageboxTitle))
                {
                    //System.Diagnostics.Debug.WriteLine("[Pushing MessageBox Dialog Button]\n");
                    SendDlgItemMessage(msg.hwnd, _buttonId, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
                    UnregisterWaitToPush();
                }
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }
        #endregion
    }
}
