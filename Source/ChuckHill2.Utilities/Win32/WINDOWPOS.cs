using System;
using System.Runtime.InteropServices;

namespace ChuckHill2.Win32
{
    public static partial class NativeMethods
    {
        [DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SWP uFlags);
    }

    /// <summary>
    /// Returned by WM_WINDOWPOSCHANGING, WM_WINDOWPOSCHANGED, WM_NCCALCSIZE, HDM_LAYOUT
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public SWP flags;

        public override string ToString()
        {
            return string.Format("{{hWnd=0x{0:X8},hWndInsertAfter=0x{1:X8},x={2},y={3},cx={4},cy={5},flags={6}}}",
                hwnd, hwndInsertAfter, x, y, cx, cy, flags);
        }

        public static WINDOWPOS ToWINDOWPOS(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero) return new WINDOWPOS();
            return (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
        }

        public void CopyToIntPtr(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero) throw new ArgumentNullException("IntPtr is Zero");
            Marshal.StructureToPtr(this, lParam, false);
        }
    }

    [Flags]
    public enum SWP
    {
        /// <summary>
        /// Draws a frame (defined in the window's class description) around the window. Same as the SWP_FRAMECHANGED flag.
        /// </summary>
        DRAWFRAME = 0x0020,
        /// <summary>
        /// Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
        /// </summary>
        FRAMECHANGED = 0x0020,
        /// <summary>
        /// Hides the window.
        /// </summary>
        HIDEWINDOW = 0x0080,
        /// <summary>
        /// Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hwndInsertAfter member).
        /// </summary>
        NOACTIVATE = 0x0010,
        /// <summary>
        /// Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
        /// </summary>
        NOCOPYBITS = 0x0100,
        /// <summary>
        /// Retains the current position (ignores the x and y members).
        /// </summary>
        NOMOVE = 0x0002,
        /// <summary>
        /// Does not change the owner window's position in the Z order.
        /// </summary>
        NOOWNERZORDER = 0x0200,
        /// <summary>
        /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
        /// </summary>
        NOREDRAW = 0x0008,
        /// <summary>
        /// Does not change the owner window's position in the Z order. Same as the SWP_NOOWNERZORDER flag.
        /// </summary>
        NOREPOSITION = 0x0200,
        /// <summary>
        /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
        /// </summary>
        NOSENDCHANGING = 0x0400,
        /// <summary>
        /// Retains the current size (ignores the cx and cy members).
        /// </summary>
        NOSIZE = 0x0001,
        /// <summary>
        /// Retains the current Z order (ignores the hwndInsertAfter member).
        /// </summary>
        NOZORDER = 0x0004,
        /// <summary>
        /// Displays the window.
        /// </summary>
        SHOWWINDOW = 0x0040
    }
}
