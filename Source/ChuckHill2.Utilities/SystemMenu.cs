using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Handy static methods for adding menu items to a Form's system menu.
    /// </summary>
    public static class SystemMenu
    {
        private static readonly WndProc _wndProc = new WndProc(WindowProc); //this MUST be static so it won't get garbage collected!

        #region -= Win32 API =-
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private const int GWL_WNDPROC = -4;
        private const UInt32 WM_DESTROY = 0x0002;
        private const UInt32 WM_INITDIALOG = 0x0110;
        private const Int32 WM_SYSCOMMAND = 0x112;
        private const Int32 MF_SEPARATOR = 0x800;
        private const Int32 MF_BYPOSITION = 0x400;
        private const Int32 MF_STRING = 0x0;
        private const Int32 SC_MENUSTART = 0xF000;  //Built-in System menu ID's start here.

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetProp(IntPtr hWnd, string lpString);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr hData);
        [DllImport("User32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        [DllImport("User32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int newValue);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr newValue);
        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern int GetMenuItemCount(IntPtr hMenu);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

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

        /// <summary>
        /// Built-in System Menu ID's. Useful if the user-assigned SystemMenuHandler wants to handle any system menu items.
        /// </summary>
        public enum SC
        {
            MENUSTART = 0xF000,   //Built-in System menu ID's start here.
            CLOSE = 0xF060,       //Closes the window.
            CONTEXTHELP = 0xF180, //Changes the cursor to a question mark with a pointer. If the user then clicks a control in the dialog box, the control receives a WM_HELP message.
            DEFAULT = 0xF160,     //Selects the default item; the user double-clicked the window menu.
            HOTKEY = 0xF150,      //Activates the window associated with the application-specified hot key. The lParam parameter identifies the window to activate.
            HSCROLL = 0xF080,     //Scrolls horizontally.
            ISSECURE = 0x0001,    //Indicates whether the screen saver is secure.
            KEYMENU = 0xF100,     //Retrieves the window menu as a result of a keystroke. For more information, see the Remarks section.
            MAXIMIZE = 0xF030,    //Maximizes the window.
            MINIMIZE = 0xF020,    //Minimizes the window.
            MONITORPOWER = 0xF170, //Sets the state of the display. This command supports devices that have power-saving features, such as a battery-powered personal computer. The lParam parameter can have the following values: -1 (the display is powering on) 1 (the display is going to low power) 2 (the display is being shut off)
            MOUSEMENU = 0xF090,   //Retrieves the window menu as a result of a mouse click.
            MOVE = 0xF010,        //Moves the window.
            NEXTWINDOW = 0xF040,  //Moves to the next window.
            PREVWINDOW = 0xF050,  //Moves to the previous window.
            RESTORE = 0xF120,     //Restores the window to its normal position and size.
            SCREENSAVE = 0xF140,  //Executes the screen saver application specified in the [boot] section of the System.ini file.
            SIZE = 0xF000,        //Sizes the window.
            TASKLIST = 0xF130,    //Activates the Start menu.
            VSCROLL = 0xF070     //Scrolls vertically.
        }

        /// <summary>
        /// Event handler for the System Menu
        /// </summary>
        /// <param name="id">The system menu item id</param>
        /// <returns>true if handled. Meaning, the default handler will not be called.</returns>
        public delegate bool SystemMenuHandler(int id);

        /// <summary>
        /// Assign a handler for the system menu events. There can only be one.
        /// Any previously set handler will be replaced. This cannot be called
        /// in the constructor because the underlying Win32 window has not yet
        /// been created. Should be called during the Form.Load event.
        /// </summary>
        /// <param name="form">form to assign the handler to</param>
        /// <param name="handler">Delegate handler to handle the events</param>
        public static void SetHandler(Form form, SystemMenuHandler handler)
        {
            IntPtr hWnd = form.Handle;
            if (hWnd == null || hWnd == IntPtr.Zero) return;
            //IntPtr handle = Marshal.GetFunctionPointerForDelegate(handler); //This only works for static methods!
            IntPtr handle = GCHandle.ToIntPtr(GCHandle.Alloc(handler));
            SetProp(hWnd, "SystemMenuHandler", handle);

            if (GetProp(hWnd, "SystemMenu") != IntPtr.Zero) return;  //Only subclass once!
            IntPtr wndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProc);
            SetProp(hWnd, "SystemMenu", SetWindowLongPtr(hWnd, GWL_WNDPROC, wndProcPtr));
        }

        /// <summary>
        /// System menu item to insert. This API cannot be called
        /// in the constructor because the underlying Win32 window has not yet
        /// been created. Should be called during the Form.Load event.
        /// </summary>
        /// <param name="form">form that will host this system menu item</param>
        /// <param name="pos">
        /// Position in the system menu where this menuitem will be placed.
        /// Use a ridiculously high positive number to just append the menuitem.
        /// Use a ridiculously high negative number to just insert at th top of the menu list. Same as 0.
        /// A negative number inserts starting from the bottom of the list.
        /// </param>
        /// <param name="name">Display name of the menuitem. If name is null or empty, a separator will be inserted.</param>
        /// <param name="id">
        /// ID of the menu item. This value is what the SystemMenuHandler delegate will handle.
        /// The built-in System menu Id's start at 0xF000 (61440), so a custom id between &gt; 1 and &lt; 0xF000 are available to use. To override system menu items see enum SystemMenu.SC.
        /// </param>
        public static void Insert(Form form, int pos, string name=null, int id=0)
        {
            IntPtr hWnd = form.Handle;
            if (hWnd == null || hWnd == IntPtr.Zero) return;
            IntPtr hSysMenu = GetSystemMenu(hWnd, false);
            int maxPos = GetMenuItemCount(hSysMenu);
            if (pos > maxPos) pos = maxPos;
            if (pos <= -maxPos) pos = 0;
            if (pos < 0) pos = maxPos + pos;
            if (string.IsNullOrEmpty(name))
                InsertMenu(hSysMenu, pos, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            else
                InsertMenu(hSysMenu, pos, MF_BYPOSITION, id, name);
        }

        private static IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr oldWndProc = GetProp(hWnd, "SystemMenu");
            try
            {
                switch (msg)
                {
                    case WM_DESTROY:
                        //Automatically uninstall ourself upon Form close
                        SetWindowLongPtr(hWnd, GWL_WNDPROC, oldWndProc);
                        break;
                    case WM_SYSCOMMAND:
                        //if (wParam.ToInt32() >= SC_MENUSTART) break;
                        IntPtr handler = GetProp(hWnd, "SystemMenuHandler");
                        if (handler != IntPtr.Zero)
                        {
                            //SystemMenuHandler syshandler = Marshal.GetDelegateForFunctionPointer(handler, typeof(SystemMenuHandler)) as SystemMenuHandler;   //This only works for static methods!
                            SystemMenuHandler syshandler = GCHandle.FromIntPtr(handler).Target as SystemMenuHandler;
                            if (syshandler.Invoke(wParam.ToInt32())) return IntPtr.Zero;
                        }
                        break;
                }
            }
            catch
            {
                //if we get an unhandled error, uninstall ourself.
                SetWindowLongPtr(hWnd, GWL_WNDPROC, oldWndProc);
            }
            return CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }
    }
}
