//--------------------------------------------------------------------------
// <summary>
// A custom tooltip component that replaces System.Windows.Forms.ToolTip().
// </summary>
// <copyright file="Win32.cs" company="Chuck Hill">
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
// <author>Chuck Hill</author>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Useful Win32 utilities
    /// </summary>
    public static class Win32
    {
        #region Message Sequence Debugging Tools
        /// <summary>
        /// Convert Window message into string for debugging. 
        /// See protected override void WndProc(ref Message m);
        /// </summary>
        /// <param name="msg">Win32 window procedure message number</param>
        /// <returns>string name of message</returns>
        public static string TranslateWMMessage(int msg) { return TranslateWMMessage(IntPtr.Zero, msg); }
        /// <summary>
        /// Convert Window message into string for debugging.
        /// See protected override void WndProc(ref Message m);
        /// </summary>
        /// <param name="hWnd">Handle of window recieving message from. This enables searching the message list specific to this Win32 common control.</param>
        /// <param name="msg">Win32 window procedure message number</param>
        /// <returns>string name of message</returns>
        public static string TranslateWMMessage(IntPtr hWnd, int msg)
        {
            if (Enum.IsDefined(typeof(WM), msg)) return ((WM)msg).ToString();
            if (hWnd==IntPtr.Zero) return string.Format("0x{0:X4}", msg);
            string name = GetWndClassName(hWnd);
            if (string.IsNullOrEmpty(name)) return string.Format("0x{0:X4}", msg);

            Type t = null;
            if (ContainsI(name, "ComboBox")) t = typeof(CBEM);
            else if (ContainsI(name, "Edit")) t = typeof(EM);
            else if (ContainsI(name, "hotkey")) t = typeof(HKM);
            else if (ContainsI(name, "IPAddress")) t = typeof(IPM);
            else if (ContainsI(name, "Month")) t = typeof(MCM);
            else if (ContainsI(name, "progress")) t = typeof(PBM);
            else if (ContainsI(name, "ListView")) t = typeof(LVM);
            else if (ContainsI(name, "ReBar")) t = typeof(RB);
            else if (ContainsI(name, "status")) t = typeof(SB);
            else if (ContainsI(name, "Toolbar")) t = typeof(TB);
            else if (ContainsI(name, "trackbar")) t = typeof(TBM);
            else if (ContainsI(name, "tooltip")) t = typeof(TTM);
            else if (ContainsI(name, "updown")) t = typeof(UDM);
            else if (ContainsI(name, "Animate")) t = typeof(ACM);
            else if (ContainsI(name, "DateTime")) t = typeof(DTM);
            else return string.Format("0x{0:X4}", msg);
            if (Enum.IsDefined(t, msg)) return Enum.GetName(t,msg);
            return string.Format("0x{0:X4}", msg);
        }
        private static bool ContainsI(string s, string substr) => s.IndexOf(substr, StringComparison.OrdinalIgnoreCase) != -1;
        //used internally by TranslateWMMessage()
        [DllImport("user32.dll")] private static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        private static string GetWndClassName(IntPtr hWnd)
        {
            var sb = new StringBuilder(512);
            int length = GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Convert WM_KEYxxx virtual keys to string for debugging
        /// </summary>
        /// <param name="key">virtual key code (VK_xxx)</param>
        /// <returns>string name of virtual key</returns>
        public static string TranslateVK_KEY(Int32 key)
        {
            switch (key)
            {
                case 0x00: return "VK_NULL";
                case 0x01: return "VK_LBUTTON";
                case 0x02: return "VK_RBUTTON";
                case 0x03: return "VK_CANCEL";
                case 0x04: return "VK_MBUTTON";
                case 0x05: return "VK_XBUTTON1";
                case 0x06: return "VK_XBUTTON2";
                case 0x07: return "VK_bell";
                case 0x08: return "VK_BACK";
                case 0x09: return "VK_TAB";
                case 0x0A: return "VK_linefeed";
                case 0x0B: return "VK_verticaltab";
                case 0x0C: return "VK_CLEAR";
                case 0x0D: return "VK_RETURN";
                case 0x0E: return "VK_so";
                case 0x0F: return "VK_si";
                case 0x10: return "VK_SHIFT";
                case 0x11: return "VK_CONTROL";
                case 0x12: return "VK_MENU";
                case 0x13: return "VK_PAUSE";
                case 0x14: return "VK_CAPITAL";
                case 0x15: return "VK_KANA";
                case 0x17: return "VK_JUNJA";
                case 0x18: return "VK_FINAL";
                case 0x19: return "VK_KANJI";
                case 0x1B: return "VK_ESCAPE";
                case 0x1C: return "VK_CONVERT";
                case 0x1D: return "VK_NONCONVERT";
                case 0x1E: return "VK_ACCEPT";
                case 0x1F: return "VK_MODECHANGE";
                case 0x20: return "VK_SPACE";
                case 0x21: return "VK_PRIOR";
                case 0x22: return "VK_NEXT";
                case 0x23: return "VK_END";
                case 0x24: return "VK_HOME";
                case 0x25: return "VK_LEFT";
                case 0x26: return "VK_UP";
                case 0x27: return "VK_RIGHT";
                case 0x28: return "VK_DOWN";
                case 0x29: return "VK_SELECT";
                case 0x2A: return "VK_PRINT";
                case 0x2B: return "VK_EXECUTE";
                case 0x2C: return "VK_SNAPSHOT";
                case 0x2D: return "VK_INSERT";
                case 0x2E: return "VK_DELETE";
                case 0x2F: return "VK_HELP";
                case 0x30: return "0";
                case 0x31: return "1";
                case 0x32: return "2";
                case 0x33: return "3";
                case 0x34: return "4";
                case 0x35: return "5";
                case 0x36: return "6";
                case 0x37: return "7";
                case 0x38: return "8";
                case 0x39: return "9";
                case 0x3A: return ":";
                case 0x3B: return ";";
                case 0x3C: return "<";
                case 0x3D: return "=";
                case 0x3E: return ">";
                case 0x3F: return "?";
                case 0x40: return "@";
                case 0x41: return "A";
                case 0x42: return "B";
                case 0x43: return "C";
                case 0x44: return "D";
                case 0x45: return "E";
                case 0x46: return "F";
                case 0x47: return "G";
                case 0x48: return "H";
                case 0x49: return "I";
                case 0x4A: return "J";
                case 0x4B: return "K";
                case 0x4C: return "L";
                case 0x4D: return "M";
                case 0x4E: return "N";
                case 0x4F: return "O";
                case 0x50: return "P";
                case 0x51: return "Q";
                case 0x52: return "R";
                case 0x53: return "S";
                case 0x54: return "T";
                case 0x55: return "U";
                case 0x56: return "V";
                case 0x57: return "W";
                case 0x58: return "X";
                case 0x59: return "Y";
                case 0x5A: return "Z";
                case 0x5B: return "VK_LWIN";
                case 0x5C: return "VK_RWIN";
                case 0x5D: return "VK_APPS";
                case 0x5F: return "VK_SLEEP";
                case 0x60: return "VK_NUMPAD0";
                case 0x61: return "VK_NUMPAD1";
                case 0x62: return "VK_NUMPAD2";
                case 0x63: return "VK_NUMPAD3";
                case 0x64: return "VK_NUMPAD4";
                case 0x65: return "VK_NUMPAD5";
                case 0x66: return "VK_NUMPAD6";
                case 0x67: return "VK_NUMPAD7";
                case 0x68: return "VK_NUMPAD8";
                case 0x69: return "VK_NUMPAD9";
                case 0x6A: return "VK_MULTIPLY";
                case 0x6B: return "VK_ADD";
                case 0x6C: return "VK_SEPARATOR";
                case 0x6D: return "VK_SUBTRACT";
                case 0x6E: return "VK_DECIMAL";
                case 0x6F: return "VK_DIVIDE";
                case 0x70: return "VK_F1";
                case 0x71: return "VK_F2";
                case 0x72: return "VK_F3";
                case 0x73: return "VK_F4";
                case 0x74: return "VK_F5";
                case 0x75: return "VK_F6";
                case 0x76: return "VK_F7";
                case 0x77: return "VK_F8";
                case 0x78: return "VK_F9";
                case 0x79: return "VK_F10";
                case 0x7A: return "VK_F11";
                case 0x7B: return "VK_F12";
                case 0x7C: return "VK_F13";
                case 0x7D: return "VK_F14";
                case 0x7E: return "VK_F15";
                case 0x7F: return "VK_F16";
                case 0x80: return "VK_F17";
                case 0x81: return "VK_F18";
                case 0x82: return "VK_F19";
                case 0x83: return "VK_F20";
                case 0x84: return "VK_F21";
                case 0x85: return "VK_F22";
                case 0x86: return "VK_F23";
                case 0x87: return "VK_F24";
                case 0x90: return "VK_NUMLOCK";
                case 0x91: return "VK_SCROLL";
                case 0xA0: return "VK_LSHIFT";
                case 0xA1: return "VK_RSHIFT";
                case 0xA2: return "VK_LCONTROL";
                case 0xA3: return "VK_RCONTROL";
                case 0xA4: return "VK_LMENU";
                case 0xA5: return "VK_RMENU";
                case 0xA6: return "VK_BROWSER_BACK";
                case 0xA7: return "VK_BROWSER_FORWARD";
                case 0xA8: return "VK_BROWSER_REFRESH";
                case 0xA9: return "VK_BROWSER_STOP";
                case 0xAA: return "VK_BROWSER_SEARCH";
                case 0xAB: return "VK_BROWSER_FAVORITES";
                case 0xAC: return "VK_BROWSER_HOME";
                case 0xAD: return "VK_VOLUME_MUTE";
                case 0xAE: return "VK_VOLUME_DOWN";
                case 0xAF: return "VK_VOLUME_UP";
                case 0xB0: return "VK_MEDIA_NEXT_TRACK";
                case 0xB1: return "VK_MEDIA_PREV_TRACK";
                case 0xB2: return "VK_MEDIA_STOP";
                case 0xB3: return "VK_MEDIA_PLAY_PAUSE";
                case 0xB4: return "VK_LAUNCH_MAIL";
                case 0xB5: return "VK_LAUNCH_MEDIA_SELECT";
                case 0xB6: return "VK_LAUNCH_APP1";
                case 0xB7: return "VK_LAUNCH_APP2";
                case 0xBA: return "VK_OEM_1";
                case 0xBB: return "VK_OEM_PLUS";
                case 0xBC: return "VK_OEM_COMMA";
                case 0xBD: return "VK_OEM_MINUS";
                case 0xBE: return "VK_OEM_PERIOD";
                case 0xBF: return "VK_OEM_2";
                case 0xC0: return "VK_OEM_3";
                case 0xDB: return "VK_OEM_4";
                case 0xDC: return "VK_OEM_5";
                case 0xDD: return "VK_OEM_6";
                case 0xDE: return "VK_OEM_7";
                case 0xDF: return "VK_OEM_8";
                case 0xE2: return "VK_OEM_102";
                case 0xE5: return "VK_PROCESSKEY";
                case 0xE7: return "VK_PACKET";
                case 0xF6: return "VK_ATTN";
                case 0xF7: return "VK_CRSEL";
                case 0xF8: return "VK_EXSEL";
                case 0xF9: return "VK_EREOF";
                case 0xFA: return "VK_PLAY";
                case 0xFB: return "VK_ZOOM";
                case 0xFC: return "VK_NONAME";
                case 0xFD: return "VK_PA1";
                case 0xFE: return "VK_OEM_CLEAR";
                default: return string.Format("0x{0:X2}", key);
            }
        }
        #endregion //Message Sequence Debugging Tools

        #region struct RECT
        /// <summary>
        /// Used everywhere when using Win32 RECT's via pInvoke.
        /// Also use this instead of System.Drawing.Rectangle when XML serializing because Rectangle does not serialize well.
        /// Includes implicit conversion between My.PowerUtilities.Win32.RECT and System.Drawing.Rectangle.
        /// </summary>
        [XmlInclude(typeof(Rectangle))] //necessary when using implicit operators
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            [XmlIgnore] public int Left;
            [XmlIgnore] public int Top;
            [XmlIgnore] public int Right;
            [XmlIgnore] public int Bottom;

            public RECT(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; }
            public RECT(Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }
            public RECT(RectangleF r) : this((int)(r.Left + 0.5), (int)(r.Top + 0.5), (int)(r.Right + 0.5), (int)(r.Bottom + 0.5)) { }

            [XmlAttribute] public int X { get { return Left; } set { Right -= (Left - value); Left = value; } }
            [XmlAttribute] public int Y { get { return Top;  } set { Bottom -= (Top - value); Top  = value; } }
            [XmlAttribute] public int Height { get { return Bottom - Top; } set { Bottom = value + Top; } }
            [XmlAttribute] public int Width  { get { return Right - Left; } set { Right = value + Left; } }

            [XmlIgnore] public Point Location { get { return new Point(Left, Top); } set { X = value.X; Y = value.Y; } }
            [XmlIgnore] public Size Size { get { return new Size(Width, Height); } set { Width = value.Width; Height = value.Height; } }

            public static implicit operator Rectangle(RECT r) { return new Rectangle(r.Left, r.Top, r.Width, r.Height); }
            public static implicit operator RectangleF(RECT r) { return new RectangleF(r.Left, r.Top, r.Width, r.Height); }
            public static implicit operator RECT(Rectangle r) { return new RECT(r); }
            public static implicit operator RECT(RectangleF r) { return new RECT(r); }
            public static bool operator ==(RECT r1, RECT r2) { return r1.Equals(r2); }
            public static bool operator !=(RECT r1, RECT r2) { return !r1.Equals(r2); }

            public bool Equals(RECT r) { return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom; }
            public override bool Equals(object obj)
            {
                if (obj is RECT)  return Equals((RECT)obj);
                else if (obj is Rectangle) return Equals(new RECT((Rectangle)obj));
                return false;
            }

            public override int GetHashCode() { return ((Rectangle)this).GetHashCode(); }
            public override string ToString()
            {
                return string.Format("{{X={0},Y={1},Width={2},Height={3}}}", Left, Top, Width, Height);
            }
            public static RECT ToRECT(IntPtr lParam)
            {
                if (lParam == IntPtr.Zero) return new RECT();
                return (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
            }
            public void CopyToIntPtr(IntPtr lParam)
            {
                if (lParam == IntPtr.Zero) throw new ArgumentNullException("IntPtr is Zero");
                Marshal.StructureToPtr(this, lParam, false);
            }
        }
        #endregion

        #region struct WINDOWPOS
        //Used by WM_WINDOWPOSCHANGING, WM_WINDOWPOSCHANGED, WM_NCCALCSIZE, HDM_LAYOUT
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
            DRAWFRAME      = 0x0020,  //Draws a frame (defined in the window's class description) around the window. Same as the SWP_FRAMECHANGED flag.
            FRAMECHANGED   = 0x0020,  //Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
            HIDEWINDOW     = 0x0080,  //Hides the window.
            NOACTIVATE     = 0x0010,  //Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hwndInsertAfter member).
            NOCOPYBITS     = 0x0100,  //Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
            NOMOVE         = 0x0002,  //Retains the current position (ignores the x and y members).
            NOOWNERZORDER  = 0x0200,  //Does not change the owner window's position in the Z order.
            NOREDRAW       = 0x0008,  //Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
            NOREPOSITION   = 0x0200,  //Does not change the owner window's position in the Z order. Same as the SWP_NOOWNERZORDER flag.
            NOSENDCHANGING = 0x0400,  //Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
            NOSIZE         = 0x0001,  //Retains the current size (ignores the cx and cy members).
            NOZORDER       = 0x0004,  //Retains the current Z order (ignores the hwndInsertAfter member).
            SHOWWINDOW     = 0x0040   //Displays the window.
        }
        #endregion

        #region Convert wParam/lParam IntPtr to/from Point & Size objects
        public static ushort LOWORD(int v) { unchecked { return (ushort)(v & 0x0000FFFF); } }
        public static ushort HIWORD(int v) { unchecked { return (ushort)(v >> 16); } }
        public static int MAKELONG(int x, int y) { unchecked { int v = LOWORD(x); return (v | (HIWORD(y) << 16)); } }

        public static Point ToPoint(this IntPtr ptr)
        {
            unchecked
            {
                int lp = ptr.ToInt32();
                int x = (int)(short)LOWORD(lp);
                int y = (int)(short)HIWORD(lp);
                return new Point(x,y);
            }
        }
        public static Size ToSize(this IntPtr ptr)
        {
            unchecked
            {
                int lp = ptr.ToInt32();
                int width = LOWORD(lp);
                int height = HIWORD(lp);
                return new Size(width, height);
            }
        }
        public static IntPtr ToIntPtr(this Point pt)
        {
            return new IntPtr(MAKELONG(pt.X, pt.Y));
        }
        public static IntPtr ToIntPtr(this Size sz)
        {
            return new IntPtr(MAKELONG(sz.Width, sz.Height));
        }
        #endregion

        #region Window Message Enums
        //https://www.autohotkey.com/boards/viewtopic.php?t=39218
        //https://wiki.winehq.org/List_Of_Windows_Messages
        //http://www.lw-tech.com/q1/base.htm
        public enum WM  //System enum
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUIT = 0x0012,
            WM_QUERYOPEN = 0x0013,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_ENDSESSION = 0x0016,
            WM_SHOWWINDOW = 0x0018,
            WM_CTLCOLOR = 0x0019,
            WM_SETTINGCHANGE = 0x001A,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            WM_GETOBJECT = 0x003D,
            WM_COMPACTING = 0x0041,
            WM_COMMNOTIFY = 0x0044,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_POWER = 0x0048,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,
            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCXBUTTONDBLCLK = 0x00AD,
            WM_NCUAHDRAWCAPTION = 0x00AE,
            WM_NCUAHDRAWFRAME = 0x00AF,

            #region "EDIT" EM_ control messages
            EM_GETSEL = 0x00B0,
            EM_SETSEL = 0x00B1,
            EM_GETRECT = 0x00B2,
            EM_SETRECT = 0x00B3,
            EM_SETRECTNP = 0x00B4,
            EM_SCROLL = 0x00B5,
            EM_LINESCROLL = 0x00B6,
            EM_SCROLLCARET = 0x00B7,
            EM_GETMODIFY = 0x00B8,
            EM_SETMODIFY = 0x00B9,
            EM_GETLINECOUNT = 0x00BA,
            EM_LINEINDEX = 0x00BB,
            EM_SETHANDLE = 0x00BC,
            EM_GETHANDLE = 0x00BD,
            EM_GETTHUMB = 0x00BE,
            EM_LINELENGTH = 0x00C1,
            EM_REPLACESEL = 0x00C2,
            EM_GETLINE = 0x00C4,
            EM_SETLIMITTEXT = 0x00C5,
            EM_CANUNDO = 0x00C6,
            EM_UNDO = 0x00C7,
            EM_FMTLINES = 0x00C8,
            EM_LINEFROMCHAR = 0x00C9,
            EM_SETTABSTOPS = 0x00CB,
            EM_SETPASSWORDCHAR = 0x00CC,
            EM_EMPTYUNDOBUFFER = 0x00CD,
            EM_GETFIRSTVISIBLELINE = 0x00CE,
            EM_SETREADONLY = 0x00CF,
            EM_SETWORDBREAKPROC = 0x00D0,
            EM_GETWORDBREAKPROC = 0x00D1,
            EM_GETPASSWORDCHAR = 0x00D2,
            EM_SETMARGINS = 0x00D3,
            EM_GETMARGINS = 0x00D4,
            EM_GETLIMITTEXT = 0x00D5,
            EM_POSFROMCHAR = 0x00D6,
            EM_CHARFROMPOS = 0x00D7,
            EM_SETIMESTATUS = 0x00D8,
            EM_GETIMESTATUS = 0x00D9,
            #endregion

            #region "STATIC" (aka Label) SBM_ control messages
            SBM_SETPOS = 0x00E0,
            SBM_GETPOS = 0x00E1,
            SBM_SETRANGE = 0x00E2,
            SBM_GETRANGE = 0x00E3,
            SBM_ENABLE_ARROWS = 0x00E4,
            SBM_SETRANGEREDRAW = 0x00E6,
            SBM_SETSCROLLINFO = 0x00E9,
            SBM_GETSCROLLINFO = 0x00EA,
            SBM_GETSCROLLBARINFO = 0x00EB,
            #endregion

            #region "BUTTON" BM_ control messages
            BM_GETCHECK = 0x00F0,
            BM_SETCHECK = 0x00F1,
            BM_GETSTATE = 0x00F2,
            BM_SETSTATE = 0x00F3,
            BM_SETSTYLE = 0x00F4,
            BM_CLICK = 0x00F5,
            BM_GETIMAGE = 0x00F6,
            BM_SETIMAGE = 0x00F7,
            BM_SETDONTCLICK = 0x00F8,
            #endregion

            WM_INPUT_DEVICE_CHANGE = 0x00FE,
            WM_INPUT = 0x00FF,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_UNICHAR = 0x0109,
            WM_CONVERTREQUEST = 0x010A,
            WM_CONVERTRESULT = 0x010B,
            WM_INTERIM = 0x010C,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_INITDIALOG = 0x0110,
            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_SYSTIMER = 0x0118, 
            WM_GESTURE = 0x0119,
            WM_GESTURENOTIFY = 0x011A,
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_MENUCOMMAND = 0x0126,
            WM_CHANGEUISTATE = 0x0127,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,

            #region "COMBOBOX" CB_ control messages
            CB_GETEDITSEL = 0x0140,
            CB_LIMITTEXT = 0x0141,
            CB_SETEDITSEL = 0x0142,
            CB_ADDSTRING = 0x0143,
            CB_DELETESTRING = 0x0144,
            CB_DIR = 0x0145,
            CB_GETCOUNT = 0x0146,
            CB_GETCURSEL = 0x0147,
            CB_GETLBTEXT = 0x0148,
            CB_GETLBTEXTLEN = 0x0149,
            CB_INSERTSTRING = 0x014A,
            CB_RESETCONTENT = 0x014B,
            CB_FINDSTRING = 0x014C,
            CB_SELECTSTRING = 0x014D,
            CB_SETCURSEL = 0x014E,
            CB_SHOWDROPDOWN = 0x014F,
            CB_GETITEMDATA = 0x0150,
            CB_SETITEMDATA = 0x0151,
            CB_GETDROPPEDCONTROLRECT = 0x0152,
            CB_SETITEMHEIGHT = 0x0153,
            CB_GETITEMHEIGHT = 0x0154,
            CB_SETEXTENDEDUI = 0x0155,
            CB_GETEXTENDEDUI = 0x0156,
            CB_GETDROPPEDSTATE = 0x0157,
            CB_FINDSTRINGEXACT = 0x0158,
            CB_SETLOCALE = 0x0159,
            CB_GETLOCALE = 0x015A,
            CB_GETTOPINDEX = 0x015B,
            CB_SETTOPINDEX = 0x015C,
            CB_GETHORIZONTALEXTENT = 0x015D,
            CB_SETHORIZONTALEXTENT = 0x015E,
            CB_GETDROPPEDWIDTH = 0x015F,
            CB_SETDROPPEDWIDTH = 0x0160,
            CB_INITSTORAGE = 0x0161,
            CB_MULTIPLEADDSTRING = 0x0163,
            CB_GETCOMBOBOXINFO = 0x0164,
            #endregion

            #region "LISTBOX" LB_ control messages
            LB_ADDSTRING = 0x0180,
            LB_INSERTSTRING = 0x0181,
            LB_DELETESTRING = 0x0182,
            LB_SELITEMRANGEEX = 0x0183,
            LB_RESETCONTENT = 0x0184,
            LB_SETSEL = 0x0185,
            LB_SETCURSEL = 0x0186,
            LB_GETSEL = 0x0187,
            LB_GETCURSEL = 0x0188,
            LB_GETTEXT = 0x0189,
            LB_GETTEXTLEN = 0x018A,
            LB_GETCOUNT = 0x018B,
            LB_SELECTSTRING = 0x018C,
            LB_DIR = 0x018D,
            LB_GETTOPINDEX = 0x018E,
            LB_FINDSTRING = 0x018F,
            LB_GETSELCOUNT = 0x0190,
            LB_GETSELITEMS = 0x0191,
            LB_SETTABSTOPS = 0x0192,
            LB_GETHORIZONTALEXTENT = 0x0193,
            LB_SETHORIZONTALEXTENT = 0x0194,
            LB_SETCOLUMNWIDTH = 0x0195,
            LB_ADDFILE = 0x0196,
            LB_SETTOPINDEX = 0x0197,
            LB_GETITEMRECT = 0x0198,
            LB_GETITEMDATA = 0x0199,
            LB_SETITEMDATA = 0x019A,
            LB_SELITEMRANGE = 0x019B,
            LB_SETANCHORINDEX = 0x019C,
            LB_GETANCHORINDEX = 0x019D,
            LB_SETCARETINDEX = 0x019E,
            LB_GETCARETINDEX = 0x019F,
            LB_SETITEMHEIGHT = 0x01A0,
            LB_GETITEMHEIGHT = 0x01A1,
            LB_FINDSTRINGEXACT = 0x01A2,
            LB_SETLOCALE = 0x01A5,
            LB_GETLOCALE = 0x01A6,
            LB_SETCOUNT = 0x01A7,
            LB_INITSTORAGE = 0x01A8,
            LB_ITEMFROMPOINT = 0x01A9,
            LB_MULTIPLEADDSTRING = 0x01B1,
            LB_GETLISTBOXINFO = 0x01B2,
            #endregion

            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MOUSEWHEEL = 0x020A,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_MOUSEHWHEEL = 0x020E,
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_POINTERDEVICECHANGE = 0x0238,
            WM_POINTERDEVICEINRANGE = 0x0239,
            WM_POINTERDEVICEOUTOFRANGE = 0x023A,
            WM_TOUCH = 0x0240,
            WM_NCPOINTERUPDATE = 0x0241,
            WM_NCPOINTERDOWN = 0x0242,
            WM_NCPOINTERUP = 0x0243,
            WM_POINTERUPDATE = 0x0245,
            WM_POINTERDOWN = 0x0246,
            WM_POINTERUP = 0x0247,
            WM_POINTERENTER = 0x0249,
            WM_POINTERLEAVE = 0x024A,
            WM_POINTERACTIVATE = 0x024B,
            WM_POINTERCAPTURECHANGED = 0x024C,
            WM_TOUCHHITTESTING = 0x024D,
            WM_POINTERWHEEL = 0x024E,
            WM_POINTERHWHEEL = 0x024F,
            WM_IME_REPORT = 0x0280,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_MOUSEHOVER = 0x02A1,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_MOUSELEAVE = 0x02A3,
            WM_WTSSESSION_CHANGE = 0x02B1,
            WM_TABLET_FIRST = 0x02C0,
            WM_TABLET_ADDED = 0x02C8,
            WM_TABLET_DELETED = 0x02C9,
            WM_TABLET_FLICK = 0x02CB,
            WM_TABLET_QUERYSYSTEMGESTURESTATUS = 0x02CC,
            WM_TABLET_LAST = 0x02DF,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_APPCOMMAND = 0x0319,
            WM_THEMECHANGED = 0x031A,
            WM_CLIPBOARDUPDATE = 0x031D,
            WM_DWMCOMPOSITIONCHANGED = 0x031E,
            WM_DWMNCRENDERINGCHANGED = 0x031F,
            WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
            WM_DWMSENDICONICTHUMBNAIL = 0x0323,
            WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
            WM_GETTITLEBARINFOEX = 0x033F,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_QUERYAFXWNDPROC = 0x0360,
            WM_SIZEPARENT = 0x0361,
            WM_SETMESSAGESTRING = 0x0362,
            WM_IDLEUPDATECMDUI = 0x0363,
            WM_INITIALUPDATE = 0x0364,
            WM_COMMANDHELP = 0x0365,
            WM_HELPHITTEST = 0x0366,
            WM_EXITHELPMODE = 0x0367,
            WM_RECALCPARENT = 0x0368,
            WM_SIZECHILD = 0x0369,
            WM_KICKIDLE = 0x036A,
            WM_QUERYCENTERWND = 0x036B,
            WM_DISABLEMODAL = 0x036C,
            WM_FLOATSTATUS = 0x036D,
            WM_ACTIVATETOPLEVEL = 0x036E,
            WM_RESERVED_036F = 0x036F,
            WM_RESERVED_0370 = 0x0370,
            WM_RESERVED_0371 = 0x0371,
            WM_RESERVED_0372 = 0x0372,
            WM_SOCKET_NOTIFY = 0x0373,
            WM_SOCKET_DEAD = 0x0374,
            WM_POPMESSAGESTRING = 0x0375,
            WM_HELPPROMPTADDR = 0x0376,
            WM_OCC_LOADFROMSTREAM = 0x0376,
            WM_OCC_LOADFROMSTORAGE = 0x0377,
            WM_OCC_INITNEW = 0x0378,
            WM_QUEUE_SENTINEL = 0x0379,
            WM_OCC_LOADFROMSTREAM_EX = 0x037A,
            WM_OCC_LOADFROMSTORAGE_EX = 0x037B,
            WM_MFC_INITCTRL = 0x037C,
            WM_RESERVED_037D = 0x037D,
            WM_RESERVED_037E = 0x037E,
            WM_FORWARDMSG = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_DDE_INITIATE = 0x03E0,
            WM_DDE_TERMINATE = 0x03E1,
            WM_DDE_ADVISE = 0x03E2,
            WM_DDE_UNADVISE = 0x03E3,
            WM_DDE_ACK = 0x03E4,
            WM_DDE_DATA = 0x03E5,
            WM_DDE_REQUEST = 0x03E6,
            WM_DDE_POKE = 0x03E7,
            WM_DDE_EXECUTE = 0x03E8,
            WM_CPL_LAUNCH = 0x07E8,
            WM_CPL_LAUNCHED = 0x07E9,

            #region "SysLink" hyperlink common control messages
            LM_HITTEST = 0x0700,
            LM_GETIDEALHEIGHT = 0x0701,
            LM_SETITEM = 0x0702,
            LM_GETITEM = 0x0703,
            #endregion

            WM_ADSPROP_NOTIFY_PAGEINIT = 0x084D,
            WM_ADSPROP_NOTIFY_PAGEHWND = 0x084E,
            WM_ADSPROP_NOTIFY_CHANGE = 0x084F,
            WM_ADSPROP_NOTIFY_APPLY = 0x0850,
            WM_ADSPROP_NOTIFY_SETFOCUS = 0x0851,
            WM_ADSPROP_NOTIFY_FOREGROUND = 0x0852,
            WM_ADSPROP_NOTIFY_EXIT = 0x0853,
            WM_ADSPROP_NOTIFY_ERROR = 0x0856,
 
            #region "SysTreeView32" TreeView common control messages
            TVM_INSERTITEMA = 0x1100,
            TVM_DELETEITEM = 0x1101,
            TVM_EXPAND = 0x1102,
            TVM_GETITEMRECT = 0x1104,
            TVM_GETCOUNT = 0x1105,
            TVM_GETINDENT = 0x1106,
            TVM_SETINDENT = 0x1107,
            TVM_GETIMAGELIST = 0x1108,
            TVM_SETIMAGELIST = 0x1109,
            TVM_GETNEXTITEM = 0x110A,
            TVM_SELECTITEM = 0x110B,
            TVM_GETITEMA = 0x110C,
            TVM_SETITEMA = 0x110D,
            TVM_EDITLABELA = 0x110E,
            TVM_GETEDITCONTROL = 0x110F,
            TVM_GETVISIBLECOUNT = 0x1110,
            TVM_HITTEST = 0x1111,
            TVM_CREATEDRAGIMAGE = 0x1112,
            TVM_SORTCHILDREN = 0x1113,
            TVM_ENSUREVISIBLE = 0x1114,
            TVM_SORTCHILDRENCB = 0x1115,
            TVM_ENDEDITLABELNOW = 0x1116,
            TVM_GETISEARCHSTRINGA = 0x1117,
            TVM_SETTOOLTIPS = 0x1118,
            TVM_GETTOOLTIPS = 0x1119,
            TVM_SETINSERTMARK = 0x111A,
            TVM_SETITEMHEIGHT = 0x111B,
            TVM_GETITEMHEIGHT = 0x111C,
            TVM_SETBKCOLOR = 0x111D,
            TVM_SETTEXTCOLOR = 0x111E,
            TVM_GETBKCOLOR = 0x111F,
            TVM_GETTEXTCOLOR = 0x1120,
            TVM_SETSCROLLTIME = 0x1121,
            TVM_GETSCROLLTIME = 0x1122,
            TVM_SETBORDER = 0x1123,
            TVM_SETINSERTMARKCOLOR = 0x1125,
            TVM_GETINSERTMARKCOLOR = 0x1126,
            TVM_GETITEMSTATE = 0x1127,
            TVM_SETLINECOLOR = 0x1128,
            TVM_GETLINECOLOR = 0x1129,
            TVM_MAPACCIDTOHTREEITEM = 0x112A,
            TVM_MAPHTREEITEMTOACCID = 0x112B,
            TVM_SETEXTENDEDSTYLE = 0x112C,
            TVM_GETEXTENDEDSTYLE = 0x112D,
            TVM_INSERTITEMW = 0x1132,
            TVM_SETHOT = 0x113A,
            TVM_SETAUTOSCROLLINFO = 0x113B,
            TVM_GETITEMW = 0x113E,
            TVM_SETITEMW = 0x113F,
            TVM_GETISEARCHSTRINGW = 0x1140,
            TVM_EDITLABELW = 0x1141,
            TVM_GETSELECTEDCOUNT = 0x1146,
            TVM_SHOWINFOTIP = 0x1147,
            TVM_GETITEMPARTRECT = 0x1148,
            #endregion

            #region "SysHeader32" Header bar control messages
            HDM_GETITEMCOUNT = 0x1200,
            HDM_INSERTITEMA = 0x1201,
            HDM_DELETEITEM = 0x1202,
            HDM_GETITEMA = 0x1203,
            HDM_SETITEMA = 0x1204,
            HDM_LAYOUT = 0x1205,
            HDM_HITTEST = 0x1206,
            HDM_GETITEMRECT = 0x1207,
            HDM_SETIMAGELIST = 0x1208,
            HDM_GETIMAGELIST = 0x1209,
            HDM_INSERTITEMW = 0x120A,
            HDM_GETITEMW = 0x120B,
            HDM_SETITEMW = 0x120C,
            HDM_ORDERTOINDEX = 0x120F,
            HDM_CREATEDRAGIMAGE = 0x1210,
            HDM_GETORDERARRAY = 0x1211,
            HDM_SETORDERARRAY = 0x1212,
            HDM_SETHOTDIVIDER = 0x1213,
            HDM_SETBITMAPMARGIN = 0x1214,
            HDM_GETBITMAPMARGIN = 0x1215,
            HDM_SETFILTERCHANGETIMEOUT = 0x1216,
            HDM_EDITFILTER = 0x1217,
            HDM_CLEARFILTER = 0x1218,
            HDM_GETITEMDROPDOWNRECT = 0x1219,
            HDM_GETOVERFLOWRECT = 0x121A,
            HDM_GETFOCUSEDITEM = 0x121B,
            HDM_SETFOCUSEDITEM = 0x121C,
            #endregion

            #region "SysTabControl32" Tab control messages
            TCM_GETIMAGELIST = 0x1302,
            TCM_SETIMAGELIST = 0x1303,
            TCM_GETITEMCOUNT = 0x1304,
            TCM_GETITEMA = 0x1305,
            TCM_SETITEMA = 0x1306,
            TCM_INSERTITEMA = 0x1307,
            TCM_DELETEITEM = 0x1308,
            TCM_DELETEALLITEMS = 0x1309,
            TCM_GETITEMRECT = 0x130A,
            TCM_GETCURSEL = 0x130B,
            TCM_SETCURSEL = 0x130C,
            TCM_HITTEST = 0x130D,
            TCM_SETITEMEXTRA = 0x130E,
            TCM_ADJUSTRECT = 0x1328,
            TCM_SETITEMSIZE = 0x1329,
            TCM_REMOVEIMAGE = 0x132A,
            TCM_SETPADDING = 0x132B,
            TCM_GETROWCOUNT = 0x132C,
            TCM_GETTOOLTIPS = 0x132D,
            TCM_SETTOOLTIPS = 0x132E,
            TCM_GETCURFOCUS = 0x132F,
            TCM_SETCURFOCUS = 0x1330,
            TCM_SETMINTABWIDTH = 0x1331,
            TCM_DESELECTALL = 0x1332,
            TCM_HIGHLIGHTITEM = 0x1333,
            TCM_SETEXTENDEDSTYLE = 0x1334,
            TCM_GETEXTENDEDSTYLE = 0x1335,
            TCM_GETITEMW = 0x133C,
            TCM_SETITEMW = 0x133D,
            TCM_INSERTITEMW = 0x133E,
            #endregion

            #region "SysPager" page scroller common control messages
            PGM_SETCHILD = 0x1401,
            PGM_RECALCSIZE = 0x1402,
            PGM_FORWARDMOUSE = 0x1403,
            PGM_SETBKCOLOR = 0x1404,
            PGM_GETBKCOLOR = 0x1405,
            PGM_SETBORDER = 0x1406,
            PGM_GETBORDER = 0x1407,
            PGM_SETPOS = 0x1408,
            PGM_GETPOS = 0x1409,
            PGM_SETBUTTONSIZE = 0x140A,
            PGM_GETBUTTONSIZE = 0x140B,
            PGM_GETBUTTONSTATE = 0x140C,
            PGM_SETSCROLLINFO = 0x140D,
            #endregion

            #region "Edit" control messages
            EM_SETCUEBANNER = 0x1501,
            EM_GETCUEBANNER = 0x1502,
            EM_SHOWBALLOONTIP = 0x1503,
            EM_HIDEBALLOONTIP = 0x1504,
            EM_SETHILITE = 0x1505,
            EM_GETHILITE = 0x1506,
            EM_NOSETFOCUS = 0x1507,
            EM_TAKEFOCUS = 0x1508,
            #endregion

            #region "Button" common control messages
            BCM_GETIDEALSIZE = 0x1601,
            BCM_SETIMAGELIST = 0x1602,
            BCM_GETIMAGELIST = 0x1603,
            BCM_SETTEXTMARGIN = 0x1604,
            BCM_GETTEXTMARGIN = 0x1605,
            BCM_SETDROPDOWNSTATE = 0x1606,
            BCM_SETSPLITINFO = 0x1607,
            BCM_GETSPLITINFO = 0x1608,
            BCM_SETNOTE = 0x1609,
            BCM_GETNOTE = 0x160A,
            BCM_GETNOTELENGTH = 0x160B,
            BCM_SETSHIELD = 0x160C,
            #endregion

            #region "Combobox" control messages
            CB_SETMINVISIBLE = 0x1701,
            CB_GETMINVISIBLE = 0x1702,
            CB_SETCUEBANNER = 0x1703,
            CB_GETCUEBANNER = 0x1704,
            #endregion

            #region Common control shared messages
            CCM_SETBKCOLOR = 0x2001,
            CCM_SETCOLORSCHEME = 0x2002,
            CCM_GETCOLORSCHEME = 0x2003,
            CCM_GETDROPTARGET = 0x2004,
            CCM_SETUNICODEFORMAT = 0x2005,
            CCM_GETUNICODEFORMAT = 0x2006,
            CCM_SETVERSION = 0x2007,
            CCM_GETVERSION = 0x2008,
            CCM_SETNOTIFYWINDOW = 0x2009,
            CCM_SETWINDOWTHEME = 0x200B,
            CCM_DPISCALE = 0x200C,
            CCM_LAST = 0x2200,
            #endregion

            WM_APP = 0x8000,
            WM_REFLECT_BASE = 0xBC00,
            WM_RASDIALEVENT = 0xCCCD
        }

        //The following have overlapping message id's with each other (not the above system enums),
        //so we must know the type of control to identify these messages

        public enum ACM  //"SysAnimate32" common control messages
        {
            ACM_OPENA = 0x0464,
            ACM_PLAY = 0x0465,
            ACM_STOP = 0x0466,
            ACM_OPENW = 0x0467,
            ACM_ISPLAYING = 0x0468
        }

        public enum CBEM //"ComboBoxEx32" common control messages
        {
            CBEM_INSERTITEMA = 0x0401,
            CBEM_SETIMAGELIST = 0x0402,
            CBEM_GETIMAGELIST = 0x0403,
            CBEM_GETITEMA = 0x0404,
            CBEM_SETITEMA = 0x0405,
            CBEM_GETCOMBOCONTROL = 0x0406,
            CBEM_GETEDITCONTROL = 0x0407,
            CBEM_SETEXSTYLE = 0x0408,
            CBEM_GETEXSTYLE = 0x0409,
            CBEM_GETEXTENDEDSTYLE = 0x0409,
            CBEM_HASEDITCHANGED = 0x040A,
            CBEM_INSERTITEMW = 0x040B,
            CBEM_SETITEMW = 0x040C,
            CBEM_GETITEMW = 0x040D,
            CBEM_SETEXTENDEDSTYLE = 0x040E
        }

        public enum DL //APIs to make a listbox source and sink drag&drop actions.
        {
            DL_BEGINDRAG = 0x0485,
            DL_DRAGGING = 0x0486,
            DL_DROPPED = 0x0487,
            DL_CANCELDRAG = 0x0488
        }

        public enum DTM //"SysDateTimePick32" control messages
        {
            DTM_GETSYSTEMTIME = 0x1001,
            DTM_SETSYSTEMTIME = 0x1002,
            DTM_GETRANGE = 0x1003,
            DTM_SETRANGE = 0x1004,
            DTM_SETFORMATA = 0x1005,
            DTM_SETMCCOLOR = 0x1006,
            DTM_GETMCCOLOR = 0x1007,
            DTM_GETMONTHCAL = 0x1008,
            DTM_SETMCFONT = 0x1009,
            DTM_GETMCFONT = 0x100A,
            DTM_SETMCSTYLE = 0x100B,
            DTM_GETMCSTYLE = 0x100C,
            DTM_CLOSEMONTHCAL = 0x100D,
            DTM_GETDATETIMEPICKERINFO = 0x100E,
            DTM_GETIDEALSIZE = 0x100F,
            DTM_SETFORMATW = 0x1032
        }

        public enum EM //"Edit" common control messages
        {
            EM_GETLIMITTEXT = 0x0425,
            EM_POSFROMCHAR = 0x0426,
            EM_CHARFROMPOS = 0x0427,
            EM_SCROLLCARET = 0x0431,
            EM_CANPASTE = 0x0432,
            EM_DISPLAYBAND = 0x0433,
            EM_EXGETSEL = 0x0434,
            EM_EXLIMITTEXT = 0x0435,
            EM_EXLINEFROMCHAR = 0x0436,
            EM_EXSETSEL = 0x0437,
            EM_FINDTEXT = 0x0438,
            EM_FORMATRANGE = 0x0439,
            EM_GETCHARFORMAT = 0x043A,
            EM_GETEVENTMASK = 0x043B,
            EM_GETOLEINTERFACE = 0x043C,
            EM_GETPARAFORMAT = 0x043D,
            EM_GETSELTEXT = 0x043E,
            EM_HIDESELECTION = 0x043F,
            EM_PASTESPECIAL = 0x0440,
            EM_REQUESTRESIZE = 0x0441,
            EM_SELECTIONTYPE = 0x0442,
            EM_SETBKGNDCOLOR = 0x0443,
            EM_SETCHARFORMAT = 0x0444,
            EM_SETEVENTMASK = 0x0445,
            EM_SETOLECALLBACK = 0x0446,
            EM_SETPARAFORMAT = 0x0447,
            EM_SETTARGETDEVICE = 0x0448,
            EM_STREAMIN = 0x0449,
            EM_STREAMOUT = 0x044A,
            EM_GETTEXTRANGE = 0x044B,
            EM_FINDWORDBREAK = 0x044C,
            EM_SETOPTIONS = 0x044D,
            EM_GETOPTIONS = 0x044E,
            EM_FINDTEXTEX = 0x044F,
            EM_GETWORDBREAKPROCEX = 0x0450,
            EM_SETWORDBREAKPROCEX = 0x0451,
            EM_SETUNDOLIMIT = 0x0452,
            EM_REDO = 0x0454,
            EM_CANREDO = 0x0455,
            EM_GETUNDONAME = 0x0456,
            EM_GETREDONAME = 0x0457,
            EM_STOPGROUPTYPING = 0x0458,
            EM_SETTEXTMODE = 0x0459,
            EM_GETTEXTMODE = 0x045A,
            EM_AUTOURLDETECT = 0x045B,
            EM_GETAUTOURLDETECT = 0x045C,
            EM_SETPALETTE = 0x045D,
            EM_GETTEXTEX = 0x045E,
            EM_GETTEXTLENGTHEX = 0x045F,
            EM_SHOWSCROLLBAR = 0x0460,
            EM_SETTEXTEX = 0x0461,
            EM_SETPUNCTUATION = 0x0464,
            EM_GETPUNCTUATION = 0x0465,
            EM_SETWORDWRAPMODE = 0x0466,
            EM_GETWORDWRAPMODE = 0x0467,
            EM_SETIMECOLOR = 0x0468,
            EM_GETIMECOLOR = 0x0469,
            EM_SETIMEOPTIONS = 0x046A,
            EM_GETIMEOPTIONS = 0x046B,
            EM_CONVPOSITION = 0x046C,
            EM_SETLANGOPTIONS = 0x0478,
            EM_GETLANGOPTIONS = 0x0479,
            EM_GETIMECOMPMODE = 0x047A,
            EM_FINDTEXTW = 0x047B,
            EM_FINDTEXTEXW = 0x047C,
            EM_RECONVERSION = 0x047D,
            EM_SETIMEMODEBIAS = 0x047E,
            EM_GETIMEMODEBIAS = 0x047F,
            EM_SETBIDIOPTIONS = 0x04C8,
            EM_GETBIDIOPTIONS = 0x04C9,
            EM_SETTYPOGRAPHYOPTIONS = 0x04CA,
            EM_GETTYPOGRAPHYOPTIONS = 0x04CB,
            EM_SETEDITSTYLE = 0x04CC,
            EM_GETEDITSTYLE = 0x04CD,
            EM_OUTLINE = 0x04DC,
            EM_GETSCROLLPOS = 0x04DD,
            EM_SETSCROLLPOS = 0x04DE,
            EM_SETFONTSIZE = 0x04DF,
            EM_GETZOOM = 0x04E0,
            EM_SETZOOM = 0x04E1,
            EM_GETVIEWKIND = 0x04E2,
            EM_SETVIEWKIND = 0x04E3,
            EM_GETPAGE = 0x04E4,
            EM_SETPAGE = 0x04E5,
            EM_GETHYPHENATEINFO = 0x04E6,
            EM_SETHYPHENATEINFO = 0x04E7,
            EM_INSERTTABLE = 0x04E8,
            EM_GETAUTOCORRECTPROC = 0x04E9,
            EM_SETAUTOCORRECTPROC = 0x04EA,
            EM_GETPAGEROTATE = 0x04EB,
            EM_SETPAGEROTATE = 0x04EC,
            EM_GETCTFMODEBIAS = 0x04ED,
            EM_SETCTFMODEBIAS = 0x04EE,
            EM_GETCTFOPENSTATUS = 0x04F0,
            EM_SETCTFOPENSTATUS = 0x04F1,
            EM_GETIMECOMPTEXT = 0x04F2,
            EM_ISIME = 0x04F3,
            EM_GETIMEPROPERTY = 0x04F4,
            EM_CALLAUTOCORRECTPROC = 0x04FF,
            EM_GETTABLEPARMS = 0x0509,
            EM_GETQUERYRTFOBJ = 0x050D,
            EM_SETQUERYRTFOBJ = 0x050E,
            EM_SETEDITSTYLEEX = 0x0513,
            EM_GETEDITSTYLEEX = 0x0514,
            EM_GETSTORYTYPE = 0x0522,
            EM_SETSTORYTYPE = 0x0523,
            EM_GETELLIPSISMODE = 0x0531,
            EM_SETELLIPSISMODE = 0x0532,
            EM_SETTABLEPARMS = 0x0533,
            EM_GETTOUCHOPTIONS = 0x0536,
            EM_SETTOUCHOPTIONS = 0x0537,
            EM_INSERTIMAGE = 0x053A,
            EM_SETUIANAME = 0x0540,
            EM_GETELLIPSISSTATE = 0x0542
        }

        public enum HKM  //"msctls_hotkey32" Hotkey common control messages
        {
            HKM_SETHOTKEY = 0x0401,
            HKM_GETHOTKEY = 0x0402,
            HKM_SETRULES = 0x0403
        }

        public enum IPM //"SysIPAddress32" IP Address edit control
        {
            IPM_CLEARADDRESS = 0x0464,
            IPM_SETADDRESS = 0x0465,
            IPM_GETADDRESS = 0x0466,
            IPM_SETRANGE = 0x0467,
            IPM_SETFOCUS = 0x0468,
            IPM_ISBLANK = 0x0469
        }

        public enum LVM //"SysListView32" control messages
        {
            LVM_GETBKCOLOR = 0x1000,
            LVM_SETBKCOLOR = 0x1001,
            LVM_GETIMAGELIST = 0x1002,
            LVM_SETIMAGELIST = 0x1003,
            LVM_GETITEMCOUNT = 0x1004,
            LVM_GETITEMA = 0x1005,
            LVM_SETITEMA = 0x1006,
            LVM_INSERTITEMA = 0x1007,
            LVM_DELETEITEM = 0x1008,
            LVM_DELETEALLITEMS = 0x1009,
            LVM_GETCALLBACKMASK = 0x100A,
            LVM_SETCALLBACKMASK = 0x100B,
            LVM_GETNEXTITEM = 0x100C,
            LVM_FINDITEMA = 0x100D,
            LVM_GETITEMRECT = 0x100E,
            LVM_SETITEMPOSITION = 0x100F,
            LVM_GETITEMPOSITION = 0x1010,
            LVM_GETSTRINGWIDTHA = 0x1011,
            LVM_HITTEST = 0x1012,
            LVM_ENSUREVISIBLE = 0x1013,
            LVM_SCROLL = 0x1014,
            LVM_REDRAWITEMS = 0x1015,
            LVM_ARRANGE = 0x1016,
            LVM_EDITLABELA = 0x1017,
            LVM_GETEDITCONTROL = 0x1018,
            LVM_GETCOLUMNA = 0x1019,
            LVM_SETCOLUMNA = 0x101A,
            LVM_INSERTCOLUMNA = 0x101B,
            LVM_DELETECOLUMN = 0x101C,
            LVM_GETCOLUMNWIDTH = 0x101D,
            LVM_SETCOLUMNWIDTH = 0x101E,
            LVM_GETHEADER = 0x101F,
            LVM_CREATEDRAGIMAGE = 0x1021,
            LVM_GETVIEWRECT = 0x1022,
            LVM_GETTEXTCOLOR = 0x1023,
            LVM_SETTEXTCOLOR = 0x1024,
            LVM_GETTEXTBKCOLOR = 0x1025,
            LVM_SETTEXTBKCOLOR = 0x1026,
            LVM_GETTOPINDEX = 0x1027,
            LVM_GETCOUNTPERPAGE = 0x1028,
            LVM_GETORIGIN = 0x1029,
            LVM_UPDATE = 0x102A,
            LVM_SETITEMSTATE = 0x102B,
            LVM_GETITEMSTATE = 0x102C,
            LVM_GETITEMTEXTA = 0x102D,
            LVM_SETITEMTEXTA = 0x102E,
            LVM_SETITEMCOUNT = 0x102F,
            LVM_SORTITEMS = 0x1030,
            LVM_SETITEMPOSITION32 = 0x1031,
            LVM_GETSELECTEDCOUNT = 0x1032,
            LVM_GETITEMSPACING = 0x1033,
            LVM_GETISEARCHSTRINGA = 0x1034,
            LVM_SETICONSPACING = 0x1035,
            LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036,
            LVM_GETEXTENDEDLISTVIEWSTYLE = 0x1037,
            LVM_GETSUBITEMRECT = 0x1038,
            LVM_SUBITEMHITTEST = 0x1039,
            LVM_SETCOLUMNORDERARRAY = 0x103A,
            LVM_GETCOLUMNORDERARRAY = 0x103B,
            LVM_SETHOTITEM = 0x103C,
            LVM_GETHOTITEM = 0x103D,
            LVM_SETHOTCURSOR = 0x103E,
            LVM_GETHOTCURSOR = 0x103F,
            LVM_APPROXIMATEVIEWRECT = 0x1040,
            LVM_SETWORKAREAS = 0x1041,
            LVM_GETSELECTIONMARK = 0x1042,
            LVM_SETSELECTIONMARK = 0x1043,
            LVM_SETBKIMAGEA = 0x1044,
            LVM_GETBKIMAGEA = 0x1045,
            LVM_GETWORKAREAS = 0x1046,
            LVM_SETHOVERTIME = 0x1047,
            LVM_GETHOVERTIME = 0x1048,
            LVM_GETNUMBEROFWORKAREAS = 0x1049,
            LVM_SETTOOLTIPS = 0x104A,
            LVM_GETITEMW = 0x104B,
            LVM_SETITEMW = 0x104C,
            LVM_INSERTITEMW = 0x104D,
            LVM_GETTOOLTIPS = 0x104E,
            LVM_SORTITEMSEX = 0x1051,
            LVM_FINDITEMW = 0x1053,
            LVM_GETSTRINGWIDTHW = 0x1057,
            LVM_GETGROUPSTATE = 0x105C,
            LVM_GETFOCUSEDGROUP = 0x105D,
            LVM_GETCOLUMNW = 0x105F,
            LVM_SETCOLUMNW = 0x1060,
            LVM_INSERTCOLUMNW = 0x1061,
            LVM_GETGROUPRECT = 0x1062,
            LVM_GETITEMTEXTW = 0x1073,
            LVM_SETITEMTEXTW = 0x1074,
            LVM_GETISEARCHSTRINGW = 0x1075,
            LVM_EDITLABELW = 0x1076,
            LVM_SETBKIMAGEW = 0x108A,
            LVM_GETBKIMAGEW = 0x108B,
            LVM_SETSELECTEDCOLUMN = 0x108C,
            LVM_SETVIEW = 0x108E,
            LVM_GETVIEW = 0x108F,
            LVM_INSERTGROUP = 0x1091,
            LVM_SETGROUPINFO = 0x1093,
            LVM_GETGROUPINFO = 0x1095,
            LVM_REMOVEGROUP = 0x1096,
            LVM_MOVEGROUP = 0x1097,
            LVM_GETGROUPCOUNT = 0x1098,
            LVM_GETGROUPINFOBYINDEX = 0x1099,
            LVM_MOVEITEMTOGROUP = 0x109A,
            LVM_SETGROUPMETRICS = 0x109B,
            LVM_GETGROUPMETRICS = 0x109C,
            LVM_ENABLEGROUPVIEW = 0x109D,
            LVM_SORTGROUPS = 0x109E,
            LVM_INSERTGROUPSORTED = 0x109F,
            LVM_REMOVEALLGROUPS = 0x10A0,
            LVM_HASGROUP = 0x10A1,
            LVM_SETTILEVIEWINFO = 0x10A2,
            LVM_GETTILEVIEWINFO = 0x10A3,
            LVM_SETTILEINFO = 0x10A4,
            LVM_GETTILEINFO = 0x10A5,
            LVM_SETINSERTMARK = 0x10A6,
            LVM_GETINSERTMARK = 0x10A7,
            LVM_INSERTMARKHITTEST = 0x10A8,
            LVM_GETINSERTMARKRECT = 0x10A9,
            LVM_SETINSERTMARKCOLOR = 0x10AA,
            LVM_GETINSERTMARKCOLOR = 0x10AB,
            LVM_GETSELECTEDCOLUMN = 0x10AE,
            LVM_ISGROUPVIEWENABLED = 0x10AF,
            LVM_GETOUTLINECOLOR = 0x10B0,
            LVM_SETOUTLINECOLOR = 0x10B1,
            LVM_CANCELEDITLABEL = 0x10B3,
            LVM_MAPINDEXTOID = 0x10B4,
            LVM_MAPIDTOINDEX = 0x10B5,
            LVM_ISITEMVISIBLE = 0x10B6,
            LVM_GETEMPTYTEXT = 0x10CC,
            LVM_GETFOOTERRECT = 0x10CD,
            LVM_GETFOOTERINFO = 0x10CE,
            LVM_GETFOOTERITEMRECT = 0x10CF,
            LVM_GETFOOTERITEM = 0x10D0,
            LVM_GETITEMINDEXRECT = 0x10D1,
            LVM_SETITEMINDEXSTATE = 0x10D2,
            LVM_GETNEXTITEMINDEX = 0x10D3
        }

        public enum MCM //"SysMonthCal32" common control messages
        {
            MCM_GETCURSEL = 0x1001,
            MCM_SETCURSEL = 0x1002,
            MCM_GETMAXSELCOUNT = 0x1003,
            MCM_SETMAXSELCOUNT = 0x1004,
            MCM_GETSELRANGE = 0x1005,
            MCM_SETSELRANGE = 0x1006,
            MCM_GETMONTHRANGE = 0x1007,
            MCM_SETDAYSTATE = 0x1008,
            MCM_GETMINREQRECT = 0x1009,
            MCM_SETCOLOR = 0x100A,
            MCM_GETCOLOR = 0x100B,
            MCM_SETTODAY = 0x100C,
            MCM_GETTODAY = 0x100D,
            MCM_HITTEST = 0x100E,
            MCM_SETFIRSTDAYOFWEEK = 0x100F,
            MCM_GETFIRSTDAYOFWEEK = 0x1010,
            MCM_GETRANGE = 0x1011,
            MCM_SETRANGE = 0x1012,
            MCM_GETMONTHDELTA = 0x1013,
            MCM_SETMONTHDELTA = 0x1014,
            MCM_GETMAXTODAYWIDTH = 0x1015,
            MCM_GETCURRENTVIEW = 0x1016,
            MCM_GETCALENDARCOUNT = 0x1017,
            MCM_GETCALENDARGRIDINFO = 0x1018,
            MCM_GETCALID = 0x101B,
            MCM_SETCALID = 0x101C,
            MCM_SIZERECTTOMIN = 0x101D,
            MCM_SETCALENDARBORDER = 0x101E,
            MCM_GETCALENDARBORDER = 0x101F,
            MCM_SETCURRENTVIEW = 0x1020
        }

        public enum PBM //"msctls_progress" progress bar common control messages
        {
            PBM_SETRANGE = 0x0401,
            PBM_SETPOS = 0x0402,
            PBM_DELTAPOS = 0x0403,
            PBM_SETSTEP = 0x0404,
            PBM_STEPIT = 0x0405,
            PBM_SETRANGE32 = 0x0406,
            PBM_GETRANGE = 0x0407,
            PBM_GETPOS = 0x0408,
            PBM_SETBARCOLOR = 0x0409,
            PBM_SETMARQUEE = 0x040A,
            PBM_GETSTEP = 0x040D,
            PBM_GETBKCOLOR = 0x040E,
            PBM_GETBARCOLOR = 0x040F,
            PBM_SETSTATE = 0x0410,
            PBM_GETSTATE = 0x0411
        }

        public enum RB //"ReBarWindow32" common control messages
        {
            RB_INSERTBANDA = 0x0401,
            RB_DELETEBAND = 0x0402,
            RB_GETBARINFO = 0x0403,
            RB_SETBARINFO = 0x0404,
            RB_SETBANDINFOA = 0x0406,
            RB_SETPARENT = 0x0407,
            RB_HITTEST = 0x0408,
            RB_GETRECT = 0x0409,
            RB_INSERTBANDW = 0x040A,
            RB_SETBANDINFOW = 0x040B,
            RB_GETBANDCOUNT = 0x040C,
            RB_GETROWCOUNT = 0x040D,
            RB_GETROWHEIGHT = 0x040E,
            RB_IDTOINDEX = 0x0410,
            RB_GETTOOLTIPS = 0x0411,
            RB_SETTOOLTIPS = 0x0412,
            RB_SETBKCOLOR = 0x0413,
            RB_GETBKCOLOR = 0x0414,
            RB_SETTEXTCOLOR = 0x0415,
            RB_GETTEXTCOLOR = 0x0416,
            RB_SIZETORECT = 0x0417,
            RB_BEGINDRAG = 0x0418,
            RB_ENDDRAG = 0x0419,
            RB_DRAGMOVE = 0x041A,
            RB_GETBARHEIGHT = 0x041B,
            RB_GETBANDINFOW = 0x041C,
            RB_GETBANDINFOA = 0x041D,
            RB_MINIMIZEBAND = 0x041E,
            RB_MAXIMIZEBAND = 0x041F,
            RB_GETBANDBORDERS = 0x0422,
            RB_SHOWBAND = 0x0423,
            RB_SETPALETTE = 0x0425,
            RB_GETPALETTE = 0x0426,
            RB_MOVEBAND = 0x0427,
            RB_GETBANDMARGINS = 0x0428,
            RB_SETEXTENDEDSTYLE = 0x0429,
            RB_GETEXTENDEDSTYLE = 0x042A,
            RB_PUSHCHEVRON = 0x042B,
            RB_SETBANDWIDTH = 0x042C
        }

        public enum SB //"msctls_statusbar32" common control messages
        {
            SB_SETTEXTA = 0x0401,
            SB_GETTEXTA = 0x0402,
            SB_GETTEXTLENGTHA = 0x0403,
            SB_SETPARTS = 0x0404,
            SB_GETPARTS = 0x0406,
            SB_GETBORDERS = 0x0407,
            SB_SETMINHEIGHT = 0x0408,
            SB_SIMPLE = 0x0409,
            SB_GETRECT = 0x040A,
            SB_SETTEXTW = 0x040B,
            SB_GETTEXTLENGTHW = 0x040C,
            SB_GETTEXTW = 0x040D,
            SB_ISSIMPLE = 0x040E,
            SB_SETICON = 0x040F,
            SB_SETTIPTEXTA = 0x0410,
            SB_SETTIPTEXTW = 0x0411,
            SB_GETTIPTEXTA = 0x0412,
            SB_GETTIPTEXTW = 0x0413,
            SB_GETICON = 0x0414
        }

        public enum TB //"ToolbarWindow32" common control messages
        {
            TB_ENABLEBUTTON = 0x0401,
            TB_CHECKBUTTON = 0x0402,
            TB_PRESSBUTTON = 0x0403,
            TB_HIDEBUTTON = 0x0404,
            TB_INDETERMINATE = 0x0405,
            TB_MARKBUTTON = 0x0406,
            TB_ISBUTTONENABLED = 0x0409,
            TB_ISBUTTONCHECKED = 0x040A,
            TB_ISBUTTONPRESSED = 0x040B,
            TB_ISBUTTONHIDDEN = 0x040C,
            TB_ISBUTTONINDETERMINATE = 0x040D,
            TB_ISBUTTONHIGHLIGHTED = 0x040E,
            TB_SETSTATE = 0x0411,
            TB_GETSTATE = 0x0412,
            TB_ADDBITMAP = 0x0413,
            TB_ADDBUTTONSA = 0x0414,
            TB_INSERTBUTTONA = 0x0415,
            TB_DELETEBUTTON = 0x0416,
            TB_GETBUTTON = 0x0417,
            TB_BUTTONCOUNT = 0x0418,
            TB_COMMANDTOINDEX = 0x0419,
            TB_SAVERESTOREA = 0x041A,
            TB_CUSTOMIZE = 0x041B,
            TB_ADDSTRINGA = 0x041C,
            TB_GETITEMRECT = 0x041D,
            TB_BUTTONSTRUCTSIZE = 0x041E,
            TB_SETBUTTONSIZE = 0x041F,
            TB_SETBITMAPSIZE = 0x0420,
            TB_AUTOSIZE = 0x0421,
            TB_GETTOOLTIPS = 0x0423,
            TB_SETTOOLTIPS = 0x0424,
            TB_SETPARENT = 0x0425,
            TB_SETROWS = 0x0427,
            TB_GETROWS = 0x0428,
            TB_GETBITMAPFLAGS = 0x0429,
            TB_SETCMDID = 0x042A,
            TB_CHANGEBITMAP = 0x042B,
            TB_GETBITMAP = 0x042C,
            TB_GETBUTTONTEXTA = 0x042D,
            TB_REPLACEBITMAP = 0x042E,
            TB_SETINDENT = 0x042F,
            TB_SETIMAGELIST = 0x0430,
            TB_GETIMAGELIST = 0x0431,
            TB_LOADIMAGES = 0x0432,
            TB_GETRECT = 0x0433,
            TB_SETHOTIMAGELIST = 0x0434,
            TB_GETHOTIMAGELIST = 0x0435,
            TB_SETDISABLEDIMAGELIST = 0x0436,
            TB_GETDISABLEDIMAGELIST = 0x0437,
            TB_SETSTYLE = 0x0438,
            TB_GETSTYLE = 0x0439,
            TB_GETBUTTONSIZE = 0x043A,
            TB_SETBUTTONWIDTH = 0x043B,
            TB_SETMAXTEXTROWS = 0x043C,
            TB_GETTEXTROWS = 0x043D,
            TB_GETOBJECT = 0x043E,
            TB_GETBUTTONINFOW = 0x043F,
            TB_SETBUTTONINFOW = 0x0440,
            TB_GETBUTTONINFOA = 0x0441,
            TB_SETBUTTONINFOA = 0x0442,
            TB_INSERTBUTTONW = 0x0443,
            TB_ADDBUTTONSW = 0x0444,
            TB_HITTEST = 0x0445,
            TB_SETDRAWTEXTFLAGS = 0x0446,
            TB_GETHOTITEM = 0x0447,
            TB_SETHOTITEM = 0x0448,
            TB_SETANCHORHIGHLIGHT = 0x0449,
            TB_GETANCHORHIGHLIGHT = 0x044A,
            TB_GETBUTTONTEXTW = 0x044B,
            TB_SAVERESTOREW = 0x044C,
            TB_ADDSTRINGW = 0x044D,
            TB_MAPACCELERATORA = 0x044E,
            TB_GETINSERTMARK = 0x044F,
            TB_SETINSERTMARK = 0x0450,
            TB_INSERTMARKHITTEST = 0x0451,
            TB_MOVEBUTTON = 0x0452,
            TB_GETMAXSIZE = 0x0453,
            TB_SETEXTENDEDSTYLE = 0x0454,
            TB_GETEXTENDEDSTYLE = 0x0455,
            TB_GETPADDING = 0x0456,
            TB_SETPADDING = 0x0457,
            TB_SETINSERTMARKCOLOR = 0x0458,
            TB_GETINSERTMARKCOLOR = 0x0459,
            TB_MAPACCELERATORW = 0x045A,
            TB_GETSTRINGW = 0x045B,
            TB_GETSTRINGA = 0x045C,
            TB_SETBOUNDINGSIZE = 0x045D,
            TB_SETHOTITEM2 = 0x045E,
            TB_HASACCELERATOR = 0x045F,
            TB_SETLISTGAP = 0x0460,
            TB_GETIMAGELISTCOUNT = 0x0462,
            TB_GETIDEALSIZE = 0x0463,
            TB_GETMETRICS = 0x0465,
            TB_SETMETRICS = 0x0466,
            TB_GETITEMDROPDOWNRECT = 0x0467,
            TB_SETPRESSEDIMAGELIST = 0x0468,
            TB_GETPRESSEDIMAGELIST = 0x0469
        }

        public enum TBM //"msctls_trackbar32" common control messages
        {
            TBM_GETPOS = 0x0400,
            TBM_GETRANGEMIN = 0x0401,
            TBM_GETRANGEMAX = 0x0402,
            TBM_GETTIC = 0x0403,
            TBM_SETTIC = 0x0404,
            TBM_SETPOS = 0x0405,
            TBM_SETRANGE = 0x0406,
            TBM_SETRANGEMIN = 0x0407,
            TBM_SETRANGEMAX = 0x0408,
            TBM_CLEARTICS = 0x0409,
            TBM_SETSEL = 0x040A,
            TBM_SETSELSTART = 0x040B,
            TBM_SETSELEND = 0x040C,
            TBM_GETPTICS = 0x040E,
            TBM_GETTICPOS = 0x040F,
            TBM_GETNUMTICS = 0x0410,
            TBM_GETSELSTART = 0x0411,
            TBM_GETSELEND = 0x0412,
            TBM_CLEARSEL = 0x0413,
            TBM_SETTICFREQ = 0x0414,
            TBM_SETPAGESIZE = 0x0415,
            TBM_GETPAGESIZE = 0x0416,
            TBM_SETLINESIZE = 0x0417,
            TBM_GETLINESIZE = 0x0418,
            TBM_GETTHUMBRECT = 0x0419,
            TBM_GETCHANNELRECT = 0x041A,
            TBM_SETTHUMBLENGTH = 0x041B,
            TBM_GETTHUMBLENGTH = 0x041C,
            TBM_SETTOOLTIPS = 0x041D,
            TBM_GETTOOLTIPS = 0x041E,
            TBM_SETTIPSIDE = 0x041F,
            TBM_SETBUDDY = 0x0420,
            TBM_GETBUDDY = 0x0421,
            TBM_SETPOSNOTIFY = 0x0422
        }

        public enum TTM //"tooltips_class32" common control messages
        {
            TTM_ACTIVATE = 0x0401,
            TTM_SETDELAYTIME = 0x0403,
            TTM_ADDTOOLA = 0x0404,
            TTM_DELTOOLA = 0x0405,
            TTM_NEWTOOLRECTA = 0x0406,
            TTM_RELAYEVENT = 0x0407,
            TTM_GETTOOLINFOA = 0x0408,
            TTM_SETTOOLINFOA = 0x0409,
            TTM_HITTESTA = 0x040A,
            TTM_GETTEXTA = 0x040B,
            TTM_UPDATETIPTEXTA = 0x040C,
            TTM_GETTOOLCOUNT = 0x040D,
            TTM_ENUMTOOLSA = 0x040E,
            TTM_GETCURRENTTOOLA = 0x040F,
            TTM_WINDOWFROMPOINT = 0x0410,
            TTM_TRACKACTIVATE = 0x0411,
            TTM_TRACKPOSITION = 0x0412,
            TTM_SETTIPBKCOLOR = 0x0413,
            TTM_SETTIPTEXTCOLOR = 0x0414,
            TTM_GETDELAYTIME = 0x0415,
            TTM_GETTIPBKCOLOR = 0x0416,
            TTM_GETTIPTEXTCOLOR = 0x0417,
            TTM_SETMAXTIPWIDTH = 0x0418,
            TTM_GETMAXTIPWIDTH = 0x0419,
            TTM_SETMARGIN = 0x041A,
            TTM_GETMARGIN = 0x041B,
            TTM_POP = 0x041C,
            TTM_UPDATE = 0x041D,
            TTM_GETBUBBLESIZE = 0x041E,
            TTM_ADJUSTRECT = 0x041F,
            TTM_SETTITLEA = 0x0420,
            TTM_SETTITLEW = 0x0421,
            TTM_POPUP = 0x0422,
            TTM_GETTITLE = 0x0423,
            TTM_ADDTOOLW = 0x0432,
            TTM_DELTOOLW = 0x0433,
            TTM_NEWTOOLRECTW = 0x0434,
            TTM_GETTOOLINFOW = 0x0435,
            TTM_SETTOOLINFOW = 0x0436,
            TTM_HITTESTW = 0x0437,
            TTM_GETTEXTW = 0x0438,
            TTM_UPDATETIPTEXTW = 0x0439,
            TTM_ENUMTOOLSW = 0x043A,
            TTM_GETCURRENTTOOLW = 0x043B
        }

        public enum UDM //"msctls_updown32" common control messages
        {
            UDM_SETRANGE = 0x0465,
            UDM_GETRANGE = 0x0466,
            UDM_SETPOS = 0x0467,
            UDM_GETPOS = 0x0468,
            UDM_SETBUDDY = 0x0469,
            UDM_GETBUDDY = 0x046A,
            UDM_SETACCEL = 0x046B,
            UDM_GETACCEL = 0x046C,
            UDM_SETBASE = 0x046D,
            UDM_GETBASE = 0x046E,
            UDM_SETRANGE32 = 0x046F,
            UDM_GETRANGE32 = 0x0470,
            UDM_SETPOS32 = 0x0471,
            UDM_GETPOS32 = 0x0472
        }
        #endregion

        #region WM_NCHITTEST Codes Enum
        public enum HT
        {
            BORDER = 18,  //In the border of a window that does not have a sizing border.
            BOTTOM = 15,  //In the lower-horizontal border of a resizable window (the user can click the mouse to resize the window vertically).
            BOTTOMLEFT = 16,  //In the lower-left corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            BOTTOMRIGHT = 17,  //In the lower-right corner of a border of a resizable window (the user can click the mouse to resize the window diagonally).
            CAPTION = 2,  //In a title bar.
            CLIENT = 1,  //In a client area.
            CLOSE = 20,  //In a Close button.
            ERROR = -2,  //On the screen background or on a dividing line between windows (same as HTNOWHERE, except that the DefWindowProc function produces a system beep to indicate an error).
            GROWBOX = 4,  //In a size box (same as HTSIZE).
            HELP = 21,  //In a Help button.
            HSCROLL = 6,  //In a horizontal scroll bar.
            LEFT = 10,  //In the left border of a resizable window (the user can click the mouse to resize the window horizontally).
            MENU = 5,  //In a menu.
            MAXBUTTON = 9,  //In a Maximize button.
            MINBUTTON = 8,  //In a Minimize button.
            NOWHERE = 0,  //On the screen background or on a dividing line between windows.
            REDUCE = 8,  //In a Minimize button.
            RIGHT = 11,  //In the right border of a resizable window (the user can click the mouse to resize the window horizontally).
            SIZE = 4,  //In a size box (same as HTGROWBOX).
            SYSMENU = 3,  //In a window menu or in a Close button in a child window.
            TOP = 12,  //In the upper-horizontal border of a window.
            TOPLEFT = 13,  //In the upper-left corner of a window border.
            TOPRIGHT = 14,  //In the upper-right corner of a window border.
            TRANSPARENT = -1,  //In a window currently covered by another window in the same thread (the message will be sent to underlying windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
            VSCROLL = 7,  //In the vertical scroll bar.
            ZOOM = 9  //In a Maximize button.
        }
        #endregion

        #region Window Style Enums
        [Flags]
        public enum WS: int
        {
            DS_ABSALIGN = 0x00000001,  //Indicates that the coordinates of the dialog box are screen coordinates. If this style is not specified, the coordinates are client coordinates.
            DS_SYSMODAL = 0x00000002,  //This style is obsolete and is included for compatibility with 16-bit versions of Windows. If you specify this style, the system creates the dialog box with the WS_EX_TOPMOST style. This style does not prevent the user from accessing other windows on the desktop. Do not combine this style with the DS_CONTROL style.
            DS_3DLOOK = 0x00000004,  //Obsolete. The system automatically applies the three-dimensional look to dialog boxes created by applications.
            DS_FIXEDSYS = 0x00000008,  //Causes the dialog box to use the SYSTEM_FIXED_FONT instead of the default SYSTEM_FONT. This is a monospace font compatible with the System font in 16-bit versions of Windows earlier than 3.0.
            DS_NOFAILCREATE = 0x00000010,  //Creates the dialog box even if errors occur — for example, if a child window cannot be created or if the system cannot create a special data segment for an edit control.
            DS_LOCALEDIT = 0x00000020,  //Applies to 16-bit applications only. This style directs edit controls in the dialog box to allocate memory from the application's data segment. Otherwise, edit controls allocate storage from a global memory object.
            DS_SETFONT = 0x00000040,  //Indicates that the header of the dialog box template (either standard or extended) contains additional data specifying the font to use for text in the client area and controls of the dialog box. If possible, the system selects a font according to the specified font data. The system passes a handle to the font to the dialog box and to each control by sending them the WM_SETFONT message. For descriptions of the format of this font data, see DLGTEMPLATE and DLGTEMPLATEEX. If neither DS_SETFONT nor DS_SHELLFONT is specified, the dialog box template does not include the font data.
            DS_MODALFRAME = 0x00000080,  //Creates a dialog box with a modal dialog-box frame that can be combined with a title bar and window menu by specifying the WS_CAPTION and WS_SYSMENU styles.
            DS_NOIDLEMSG = 0x00000100,  //Suppresses WM_ENTERIDLE messages that the system would otherwise send to the owner of the dialog box while the dialog box is displayed.
            DS_SETFOREGROUND = 0x00000200,  //Causes the system to use the SetForegroundWindow function to bring the dialog box to the foreground. This style is useful for modal dialog boxes that require immediate attention from the user regardless of whether the owner window is the foreground window. The system restricts which processes can set the foreground window. For more information, see Foreground and Background Windows.
            DS_CONTROL = 0x00000400,  //Creates a dialog box that works well as a child window of another dialog box, much like a page in a property sheet. This style allows the user to tab among the control windows of a child dialog box, use its accelerator keys, and so on.
            DS_CENTER = 0x00000800,  //Centers the dialog box in the working area of the monitor that contains the owner window. If no owner window is specified, the dialog box is centered in the working area of a monitor determined by the system. The working area is the area not obscured by the taskbar or any appbars.
            DS_CENTERMOUSE = 0x00001000,  //Centers the dialog box on the mouse cursor.
            DS_CONTEXTHELP = 0x00002000,  //Includes a question mark in the title bar of the dialog box. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a control in the dialog box, the control receives a WM_HELP message. The control should pass the message to the dialog box procedure, which should call the function using the HELP_WM_HELP command. The help application displays a pop-up window that typically contains help for the control. Note that DS_CONTEXTHELP is only a placeholder. When the dialog box is created, the system checks for DS_CONTEXTHELP and, if it is there, adds WS_EX_CONTEXTHELP to the extended style of the dialog box. WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
            DS_SHELLFONT = (DS_SETFONT | DS_FIXEDSYS),  //Indicates that the dialog box should use the system font. The typeface member of the extended dialog box template must be set to MS Shell Dlg. Otherwise, this style has no effect. It is also recommended that you use the DIALOGEX Resource, rather than the DIALOG Resource. For more information, see Dialog Box Fonts. The system selects a font using the font data specified in the pointsize, weight, and italic members. The system passes a handle to the font to the dialog box and to each control by sending them the WM_SETFONT message. For descriptions of the format of this font data, see DLGTEMPLATEEX. If neither DS_SHELLFONT nor DS_SETFONT is specified, the extended dialog box template does not include the font data.

            TILED = 0x00000000,  //The window is an overlapped window. An overlapped window has a title bar and a border. Same as the OVERLAPPED style.
            BORDER = 0x00800000,  //The window has a thin-line border.
            CAPTION = 0x00C00000,  //The window has a title bar (includes the WS_BORDER style).
            CHILD = 0x40000000,  //The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
            CHILDWINDOW = 0x40000000,  //Same as the WS_CHILD style.
            CLIPCHILDREN = 0x02000000,  //Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.
            CLIPSIBLINGS = 0x04000000,  //Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated. If CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
            DISABLED = 0x08000000,  //The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.
            DLGFRAME = 0x00400000,  //The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
            GROUP = 0x00020000,  //The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the GROUP style. The first control in each group usually has the TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            HSCROLL = 0x00100000,  //The window has a horizontal scroll bar.
            ICONIC = 0x20000000,  //The window is initially minimized. Same as the MINIMIZE style.
            MAXIMIZE = 0x01000000,  //The window is initially maximized.
            MAXIMIZEBOX = 0x00010000,  //The window has a maximize button. Cannot be combined with the EX_CONTEXTHELP style. The SYSMENU style must also be specified.
            MINIMIZE = 0x20000000,  //The window is initially minimized. Same as the ICONIC style.
            MINIMIZEBOX = 0x00020000,  //The window has a minimize button. Cannot be combined with the EX_CONTEXTHELP style. The SYSMENU style must also be specified.
            OVERLAPPED = 0x00000000,  //The window is an overlapped window. An overlapped window has a title bar and a border. Same as the TILED style.
            OVERLAPPEDWINDOW = (OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX),  //The window is an overlapped window. Same as the TILEDWINDOW style.
            POPUP = -2147483648,  //0x80000000),  //The windows is a pop-up window. This style cannot be used with the CHILD style.
            POPUPWINDOW = (POPUP | BORDER | SYSMENU), //The window is a pop-up window. The CAPTION and POPUPWINDOW styles must be combined to make the window menu visible.
            TABSTOP = 0x00010000,  //The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes the keyboard focus to the next control with the TABSTOP style. You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function. For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
            SIZEBOX    = 0x00040000,  //The window has a sizing border. Same as the THICKFRAME style.
            THICKFRAME = 0x00040000,  //The window has a sizing border. Same as the SIZEBOX style.
            SYSMENU = 0x00080000,  //The window has a window menu on its title bar. The CAPTION style must also be specified.
            TILEDWINDOW = (OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX), //The window is an overlapped window. Same as the OVERLAPPEDWINDOW style.
            VISIBLE = 0x10000000,  //The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.
            VSCROLL = 0x00200000, //The window has a vertical scroll bar.
        }

        [Flags]
        public enum WS_EX
        {
            ACCEPTFILES = 0x00000010,  //The window accepts drag-drop files.
            APPWINDOW = 0x00040000,  //Forces a top-level window onto the taskbar when the window is visible.
            CLIENTEDGE = 0x00000200,  //The window has a border with a sunken edge.
            COMPOSITED = 0x02000000,  //Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. Windows 2000:  This style is not supported.
            CONTEXTHELP = 0x00000400,  //The title bar of the window includes a question mark. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window. Cannot be used with the MAXIMIZEBOX or MINIMIZEBOX styles.
            CONTROLPARENT = 0x00010000,  //The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
            DLGMODALFRAME = 0x00000001,  //The window has a double border; the window can, optionally, be created with a title bar by specifying the CAPTION style in the dwStyle parameter.
            LAYERED = 0x00080000,  //The window is a layered window. This style cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC.  Windows 8:  The LAYERED style is supported for top-level windows and child windows. Previous Windows versions support LAYERED only for top-level windows.
            LAYOUTRTL = 0x00400000,  //If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left.
            LEFT = 0x00000000,  //The window has generic left-aligned properties. This is the default.
            LEFTSCROLLBAR = 0x00004000,  //If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
            LTRREADING = 0x00000000,  //The window text is displayed using left-to-right reading-order properties. This is the default.
            MDICHILD = 0x00000040,  //The window is a MDI child window.
            NOACTIVATE = 0x08000000,  //A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window. To activate the window, use the SetActiveWindow or SetForegroundWindow function. The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the APPWINDOW style.
            NOINHERITLAYOUT = 0x00100000,  //The window does not pass its window layout to its child windows.
            NOPARENTNOTIFY = 0x00000004,  //The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
            NOREDIRECTIONBITMAP = 0x00200000,  //The window does not render to a redirection surface. This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
            OVERLAPPEDWINDOW = (WINDOWEDGE | CLIENTEDGE), //The window is an overlapped window.
            PALETTEWINDOW = (WINDOWEDGE | TOOLWINDOW | TOPMOST), //The window is palette window, which is a modeless dialog box that presents an array of commands.
            RIGHT = 0x00001000,  //The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored. Using the RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
            RIGHTSCROLLBAR = 0x00000000,  //The vertical scroll bar (if present) is to the right of the client area. This is the default.
            RTLREADING = 0x00002000,  //If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
            STATICEDGE = 0x00020000,  //The window has a three-dimensional border style intended to be used for items that do not accept user input.
            TOOLWINDOW = 0x00000080,  //The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE.
            TOPMOST = 0x00000008,  //The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
            TRANSPARENT = 0x00000020,  //The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted. To achieve transparency without these restrictions, use the SetWindowRgn function.
            WINDOWEDGE = 0x00000100, //The window has a border with a raised edge.
        }
        #endregion

        #region FindFile
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindClose(IntPtr hFindFile);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public struct WIN32_FIND_DATA
        {
            public FileAttributes dwFileAttributes;
            public ulong ftCreationTime;
            public ulong ftLastAccessTime;
            public ulong ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }
        #endregion

        #region FileIO
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool DeleteFile(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool RemoveDirectory(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool SetFileAttributes(string lpFileName, FileAttributes attrib);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern FileAttributes GetFileAttributes(string lpFileName);
        [DllImport("Kernel32.dll", SetLastError = true)] public static extern bool CloseHandle(IntPtr hFile);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, EntryPoint = "WriteFile", SetLastError = true)]
        public static extern bool WriteFileA(IntPtr hFile, String lpBuffer, Int32 nNumberOfBytesToWrite, out Int32 lpNumberOfBytesWritten, IntPtr Overlapped);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "WriteFile", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, String lpBuffer, Int32 nNumberOfBytesToWrite, out Int32 lpNumberOfBytesWritten, IntPtr Overlapped);
        [DllImport("kernel32.dll", EntryPoint = "WriteFile", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, Int32 nNumberOfBytesToWrite, out Int32 lpNumberOfBytesWritten, IntPtr Overlapped);
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFile(string name, GENERIC DesiredAccess, FILE_SHARE ShareMode, IntPtr SecurityAttributes, FILE_DISPOSITION CreationDisposition, ATTRIBUTES FlagsAndAttributes, IntPtr hTemplateFile);
        #region CreateFile enums
        [Flags] public enum GENERIC
        {
            READ = unchecked((int)0x80000000),
            WRITE = 0x40000000,
            READWRITE = (READ|WRITE),
            EXECUTE = 0x20000000,
            ALL = 0x10000000
        }
        [Flags] public enum FILE_SHARE
        {
            NONE = 0,
            READ = 0x00000001,
            WRITE = 0x00000002,
            DELETE = 0x00000004
        }
        public enum FILE_DISPOSITION
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }
        [Flags] public enum ATTRIBUTES
        {
            WRITE_THROUGH = unchecked((int)0x80000000),
            OVERLAPPED = 0x40000000,
            NO_BUFFERING = 0x20000000,
            RANDOM_ACCESS = 0x10000000,
            SEQUENTIAL_SCAN = 0x08000000,
            DELETE_ON_CLOSE = 0x04000000,
            BACKUP_SEMANTICS = 0x02000000,
            POSIX_SEMANTICS = 0x01000000,
            OPEN_REPARSE_POINT = 0x00200000,
            OPEN_NO_RECALL = 0x00100000,
            FIRST_PIPE_INSTANCE = 0x00080000,

            READONLY = 0x00000001,
            HIDDEN = 0x00000002,
            SYSTEM = 0x00000004,
            DIRECTORY = 0x00000010,
            ARCHIVE = 0x00000020,
            DEVICE = 0x00000040,
            NORMAL = 0x00000080,
            TEMPORARY = 0x00000100,
            SPARSE_FILE = 0x00000200,
            REPARSE_POINT = 0x00000400,
            COMPRESSED = 0x00000800,
            OFFLINE = 0x00001000,
            NOT_CONTENT_INDEXED = 0x00002000,
            ENCRYPTED = 0x00004000,
            VIRTUAL = 0x00010000
        }
        #endregion
        #endregion

        [DllImport("User32.dll", SetLastError = true)] public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, StringBuilder lParam);
        [DllImport("User32.dll", SetLastError = true)] public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport("User32.dll", SetLastError = true)] public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        [DllImport("User32.dll", SetLastError = true)] public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);
        [DllImport("User32.dll", SetLastError = true)] public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        #region public static bool SetDpiAware()
        enum DPI_AWARENESS_CONTEXT
        {
            UNAWARE = -1,
            AWARE = -2,
            PER_MONITOR_AWARE = -3,
            PER_MONITOR_AWARE_V2 = -4,
            GDISCALED = -5
        }

        [DllImport("User32.dll", SetLastError = true)] private static extern bool SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT ctx);
        [DllImport("User32.dll", SetLastError = true)] private static extern bool SetProcessDPIAware();
        /// <summary>
        /// Set the process-default DPI awareness. This must be called BEFORE any window handles are created. Preferrably the first line in Program.Main().
        /// </summary>
        /// <remarks>
        /// It is recommended that you set the process-default DPI awareness via application manifest. See 
        /// https://docs.microsoft.com/en-us/windows/win32/hidpi/setting-the-default-dpi-awareness-for-a-process 
        /// for more information. Setting the process-default DPI awareness via API call can lead to unexpected 
        /// application behavior.
        /// </remarks>
        public static bool SetDpiAware()
        {
            string releaseId = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "0").ToString();
            if (!int.TryParse(releaseId, out var WINVER)) return false;

            if (WINVER >= 0x0605)
            {
                return SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.PER_MONITOR_AWARE_V2);
            }
            else if (WINVER >= 0x0600)
            {
                return SetProcessDPIAware();
            }

            return false;
        }
        #endregion public static bool SetDpiAware()
    }
}
