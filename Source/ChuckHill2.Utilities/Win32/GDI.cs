//--------------------------------------------------------------------------
// <summary>
//   p/Invoke Methods
// </summary>
// <copyright file="GDI.cs" company="Chuck Hill">
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Win32
{
    public static partial class NativeMethods
    {
        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    True if the operation succeedes, False otherwise. To get extended error information, call <see cref="System.Runtime.InteropServices.Marshal.GetLastWin32Error"/>.
        /// </returns>
        /// <remarks>
        /// Use SelectObject to select a source image into the source DC before trying to BitBlt it.
        /// 
        /// BitBlt only does clipping on the destination DC.
        /// 
        /// If a rotation or shear transformation is in effect in the source device context, BitBlt returns an error.
        /// If other transformations exist in the source device context (and a matching transformation is not in
        /// effect in the destination device context), the rectangle in the destination device context is stretched,
        /// compressed, or rotated, as necessary.
        /// 
        /// If the color formats of the source and destination device contexts do not match, the BitBlt function
        /// converts the source color format to match the destination format.
        /// 
        /// Not all devices support the BitBlt function. For more information, see the RC_BITBLT raster capability
        /// entry in the GetDeviceCaps function as well as the following functions: MaskBlt, PlgBlt, and StretchBlt.
        /// 
        /// BitBlt returns an error if the source and destination device contexts represent different devices. To transfer
        /// data between device contexts for different devices, convert the memory bitmap to a DIB by calling GetDIBits.
        /// To display the DIB to the second device, call SetDIBits or StretchDIBits.
        /// @code{.cs}
        ///             BitBlt(dc1, 0, 0, c.Width, c.Height, dc2, 0, 0, TernaryRasterOperations.SRCCOPY);
        /// @endcode
        /// The BitBlt function can be used to quickly render a Bitmap onto a Control (and much, much more).
        /// For this purpose, it is much faster than the managed alternative, Graphics.DrawImage().
        /// </remarks>
        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("gdi32.dll")] public static extern bool LineTo(IntPtr hdc, int x, int y);
        [DllImport("gdi32.dll")] public static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);
        [DllImport("user32.dll")] public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] public static extern int SetROP2(IntPtr hdc, int fnDrawMode);

        [DllImport("Gdi32.dll")] public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hgdiobj);
        [DllImport("Gdi32.dll")] public static extern bool DeleteObject(IntPtr hDC);

        #region GetTextMetrics
        [DllImport("Gdi32.dll")] private static extern bool GetTextMetricsW(IntPtr hDC, out TEXTMETRICW lptm);
        public static TEXTMETRICW GetTextMetrics(Graphics g, Font font)
        {
            var hDC = g.GetHdc();
            TEXTMETRICW textMetric;
            IntPtr hFont = font.ToHfont();
            try
            {
                IntPtr hFontPrev = SelectObject(hDC, hFont);
                bool result = GetTextMetricsW(hDC, out textMetric);
                SelectObject(hDC, hFontPrev);
            }
            finally
            {
                DeleteObject(hFont);
                g.ReleaseHdc(hDC);
            }

            return textMetric;
        }
        public static TEXTMETRICW GetTextMetrics(IWin32Window owner, Font font)
        {
            var hWnd = owner?.Handle ?? IntPtr.Zero;
            var hDC = GetDC(hWnd);
            TEXTMETRICW textMetric;
            IntPtr hFont = font.ToHfont();
            try
            {
                IntPtr hFontPrev = SelectObject(hDC, hFont);
                bool result = GetTextMetricsW(hDC, out textMetric);
                SelectObject(hDC, hFontPrev);
            }
            finally
            {
                DeleteObject(hFont);
                ReleaseDC(hWnd, hDC);
            }

            return textMetric;
        }
        #endregion

        #region public static bool SetDpiAware()
        private enum DPI_AWARENESS_CONTEXT
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
            int.TryParse(releaseId, out var WINVER);  //Is registry ReleaseId a numeric string?

            //Due to Microsoft's sketchy/undocumented versioning practices, we just wrap everything in a try/catch block. Just in case the Win32 API does not exist for this OS..
            try
            {
                if (WINVER >= 0x0605)
                    return SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.PER_MONITOR_AWARE_V2);

                if (WINVER >= 0x0600)
                    return SetProcessDPIAware();

                if (WINVER == 0) //ReleaseId is a non-numeric string?
                    return SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.PER_MONITOR_AWARE_V2);
            }
            catch
            {
                return false;
            }

            return false;
        }
        #endregion public static bool SetDpiAware()
    }

    /// <summary>
    /// Values are in Logical Units. For the default mapping mode, MM_TEXT, 1 logical unit is 1 pixel.
    /// For the desktop, the mapping mode is MM_TEXT. See Win32 'int GetMapMode(HDC hdc)'.
    /// For all intents and purposes, the values are in pixels.
    /// </summary>
    /// <remarks>
    /// See https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-textmetricw
    /// </remarks>
    [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TEXTMETRICW
    {
        /// <summary>
        /// The height (ascent + descent) of characters.
        /// </summary>
        public int tmHeight;
        /// <summary>
        /// The ascent (units above the base line) of characters.
        /// </summary>
        public int tmAscent;
        /// <summary>
        /// The descent(units below the base line) of characters.
        /// </summary>
        public int tmDescent;
        /// <summary>
        /// The amount of leading (space) inside the bounds set by the tmHeight member. Accent marks and other diacritical characters may occur in this area.
        /// </summary>
        public int tmInternalLeading;
        /// <summary>
        /// The amount of extra leading (space) that the application adds between rows.
        /// </summary>
        public int tmExternalLeading;
        /// <summary>
        /// The average width of characters in the font (generally defined as the width of the letter x ). This value does not include the overhang required for bold or italic characters.
        /// </summary>
        public int tmAveCharWidth;
        /// <summary>
        /// The width of the widest character in the font.
        /// </summary>
        public int tmMaxCharWidth;
        /// <summary>
        /// The weight of the font.
        /// </summary>
        public int tmWeight;
        /// <summary>
        /// The extra width per string that may be added to some synthesized fonts such as Bold or Italic.
        /// </summary>
        public int tmOverhang;
        /// <summary>
        /// The horizontal aspect of the device for which the font was designed.
        /// </summary>
        public int tmDigitizedAspectX;
        /// <summary>
        /// The vertical aspect of the device for which the font was designed.
        /// </summary>
        public int tmDigitizedAspectY;
        /// <summary>
        /// The value of the first character defined in the font.
        /// </summary>
        public ushort tmFirstChar;
        /// <summary>
        /// The value of the last character defined in the font.
        /// </summary>
        public ushort tmLastChar;
        /// <summary>
        /// The value of the character to be substituted for characters not in the font.
        /// </summary>
        public ushort tmDefaultChar;
        /// <summary>
        /// The value of the character that will be used to define word breaks for text justification.
        /// </summary>
        public ushort tmBreakChar;
        /// <summary>
        /// Specifies an italic font if it is nonzero.
        /// </summary>
        public byte tmItalic;
        /// <summary>
        /// Specifies an underlined font if it is nonzero.
        /// </summary>
        public byte tmUnderlined;
        /// <summary>
        /// A strikeout font if it is nonzero.
        /// </summary>
        public byte tmStruckOut;
        /// <summary>
        /// Specifies information about the pitch, the technology, and the family of a physical font.
        /// </summary>
        public byte tmPitchAndFamily;
        /// <summary>
        /// The character set of the font.
        /// </summary>
        public byte tmCharSet;
    }

    #region enum TernaryRasterOperations
    public enum TernaryRasterOperations : uint
    {
        /// <summary>dest = source</summary>
        SRCCOPY = 0x00CC0020,
        /// <summary>dest = source OR dest</summary>
        SRCPAINT = 0x00EE0086,
        /// <summary>dest = source AND dest</summary>
        SRCAND = 0x008800C6,
        /// <summary>dest = source XOR dest</summary>
        SRCINVERT = 0x00660046,
        /// <summary>dest = source AND (NOT dest)</summary>
        SRCERASE = 0x00440328,
        /// <summary>dest = (NOT source)</summary>
        NOTSRCCOPY = 0x00330008,
        /// <summary>dest = (NOT src) AND (NOT dest)</summary>
        NOTSRCERASE = 0x001100A6,
        /// <summary>dest = (source AND pattern)</summary>
        MERGECOPY = 0x00C000CA,
        /// <summary>dest = (NOT source) OR dest</summary>
        MERGEPAINT = 0x00BB0226,
        /// <summary>dest = pattern</summary>
        PATCOPY = 0x00F00021,
        /// <summary>dest = DPSnoo</summary>
        PATPAINT = 0x00FB0A09,
        /// <summary>dest = pattern XOR dest</summary>
        PATINVERT = 0x005A0049,
        /// <summary>dest = (NOT dest)</summary>
        DSTINVERT = 0x00550009,
        /// <summary>dest = BLACK</summary>
        BLACKNESS = 0x00000042,
        /// <summary>dest = WHITE</summary>
        WHITENESS = 0x00FF0062,
        /// <summary>
        /// Capture window as seen on screen.  This includes layered windows
        /// such as WPF windows with AllowsTransparency="true"
        /// </summary>
        CAPTUREBLT = 0x40000000
    }
    #endregion


}
