using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ChuckHill2
{
    ///  @image html FontMetrics.png
    /// <summary>
    /// Get font metrics in fractional pixels using .NET API only.
    /// </summary>
    /// <remarks>
    /// This is appropriately scaled to the current DPI. See Win32.SetDpiAware()
    /// </remarks>
    public class NetFontMetrics
    {
        //For ToString(): just keep private copy of font properties just in case font gets disposed.
        private readonly string _name;
        private readonly float _size;
        private readonly FontStyle _style;
        private readonly GraphicsUnit _unit;

        // http://csharphelper.com/blog/2014/08/get-font-metrics-in-c/

        public readonly float EmHeightPixels;
        public readonly float AscentPixels;
        public readonly float DescentPixels;
        public readonly float CellHeightPixels;
        public readonly float InternalLeadingPixels;
        public readonly float LineSpacingPixels;
        public readonly float ExternalLeadingPixels;

        /// <summary>
        /// Initializes a new instance of the NetFontMetrics class.
        /// </summary>
        /// <param name="font">Font to retrieve the font metric details from.</param>
        public NetFontMetrics(Font font)
        {
            _name = font.Name; //save properties for ToString() just in case font gets disposed.
            _size = font.Size;
            _style = font.Style;
            _unit = font.Unit;

            float em_height = font.FontFamily.GetEmHeight(font.Style);
            EmHeightPixels = ConvertUnits(font.Size, font.Unit, GraphicsUnit.Pixel);
            float design_to_pixels = EmHeightPixels / em_height;

            AscentPixels = design_to_pixels * font.FontFamily.GetCellAscent(font.Style);
            DescentPixels = design_to_pixels * font.FontFamily.GetCellDescent(font.Style);
            CellHeightPixels = AscentPixels + DescentPixels;
            InternalLeadingPixels = CellHeightPixels - EmHeightPixels;
            LineSpacingPixels = design_to_pixels * font.FontFamily.GetLineSpacing(font.Style);
            ExternalLeadingPixels = LineSpacingPixels - CellHeightPixels;
        }

        public override string ToString() => $"\"{_name}\", {_size}{UnitsString(_unit)}, {_style}";

        private static string UnitsString(GraphicsUnit unit)
        {
            switch (unit)
            {
                case GraphicsUnit.World: return "world";
                case GraphicsUnit.Display: return "display";
                case GraphicsUnit.Pixel: return "px";
                case GraphicsUnit.Point: return "pt";
                case GraphicsUnit.Inch: return "in";
                case GraphicsUnit.Document: return "doc";
                case GraphicsUnit.Millimeter: return "mm";
            }
            return string.Empty;
        }

        [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        private static int DpiY
        {
            get
            {
                //const int LOGPIXELSX = 88;
                const int LOGPIXELSY = 90;
                IntPtr hwnd = IntPtr.Zero; //desktop window...
                var hdc = GetDC(IntPtr.Zero);
                int dpi = GetDeviceCaps(hdc, LOGPIXELSY);
                ReleaseDC(hwnd, hdc);
                return dpi;

                ////The following is the official .NET way to get the DPI but for speed, we go direct with pInvoke above.
                //var g = Graphics.FromHwnd(IntPtr.Zero);
                //var dpi2 = g.DpiY;
                //g.Dispose();
                //return (int)(dpi2 + 0.5f);
            }
        }

        // Convert from one type of unit to another. I don't know how to do Display or World.
        private static float ConvertUnits(float value, GraphicsUnit from_unit, GraphicsUnit to_unit)
        {
            if (from_unit == to_unit) return value;

            // Convert to pixels.
            switch (from_unit)
            {
                case GraphicsUnit.Document:
                    value *= DpiY / 300;
                    break;
                case GraphicsUnit.Inch:
                    value *= DpiY;
                    break;
                case GraphicsUnit.Millimeter:
                    value *= DpiY / 25.4F;
                    break;
                case GraphicsUnit.Pixel:
                    // Do nothing.
                    break;
                case GraphicsUnit.Point:
                    value *= DpiY / 72;
                    break;
                default:
                    throw new Exception($"FontInfo.ConvertUnits: Unknown input unit {from_unit}");
            }

            // Convert from pixels to the new units.
            switch (to_unit)
            {
                case GraphicsUnit.Document:
                    value /= DpiY / 300;
                    break;
                case GraphicsUnit.Inch:
                    value /= DpiY;
                    break;
                case GraphicsUnit.Millimeter:
                    value /= DpiY / 25.4F;
                    break;
                case GraphicsUnit.Pixel:
                    // Do nothing.
                    break;
                case GraphicsUnit.Point:
                    value /= DpiY / 72;
                    break;
                default:
                    throw new Exception($"FontInfo.ConvertUnits: Unknown output unit {to_unit}");
            }

            return value;
        }
    }

    ///  @image html FontMetrics.png
    /// <summary>
    /// Get font metrics in pixels. This uses Win32 API GetTextMetrics().
    /// This is what is used internally by the .NET controls.
    /// </summary>
    /// <remarks>
    /// This is appropriately scaled to the current DPI. See Win32.SetDpiAware()
    /// </remarks>
    public class Win32FontMetrics
    {
        //For ToString(): just keep private copy of font properties just in case font gets disposed.
        private readonly string _name;
        private readonly float _size;
        private readonly FontStyle _style;
        private readonly GraphicsUnit _unit;

        // https://docs.microsoft.com/en-us/windows/win32/gdi/string-widths-and-heights
        // http://csharphelper.com/blog/2014/08/get-font-metrics-in-c/

        public readonly int EmHeightPixels;
        public readonly int AscentPixels;
        public readonly int DescentPixels;
        public readonly int CellHeightPixels;
        public readonly int InternalLeadingPixels;
        public readonly int LineSpacingPixels;
        public readonly int ExternalLeadingPixels;

        /// <summary>
        /// Initializes a new instance of the Win32FontMetrics class.
        /// </summary>
        /// <param name="font">Font to retrieve the font metric details from.</param>
        public Win32FontMetrics(Font font)
        {
            _name = font.Name; //save properties for ToString() just in case font gets disposed.
            _size = font.Size;
            _style = font.Style;
            _unit = font.Unit;

            var tm = GetTextMetrics(font);

            EmHeightPixels = tm.tmHeight;
            AscentPixels = tm.tmAscent;
            DescentPixels = tm.tmDescent;
            CellHeightPixels = tm.tmAscent + tm.tmDescent;
            InternalLeadingPixels = tm.tmInternalLeading;
            LineSpacingPixels = tm.tmHeight + tm.tmExternalLeading;
            ExternalLeadingPixels = tm.tmExternalLeading;
        }

        public override string ToString() => $"\"{_name}\", {_size}{UnitsString(_unit)}, {_style}";

        private static string UnitsString(GraphicsUnit unit)
        {
            switch (unit)
            {
                case GraphicsUnit.World: return "world";
                case GraphicsUnit.Display: return "display";
                case GraphicsUnit.Pixel: return "px";
                case GraphicsUnit.Point: return "pt";
                case GraphicsUnit.Inch: return "in";
                case GraphicsUnit.Document: return "doc";
                case GraphicsUnit.Millimeter: return "mm";
            }
            return string.Empty;
        }

        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("Gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hgdiobj);
        [DllImport("Gdi32.dll")] private static extern bool GetTextMetricsW(IntPtr hDC, out TEXTMETRICW lptm);
        [DllImport("Gdi32.dll")] private static extern bool DeleteObject(IntPtr hDC);

        private static TEXTMETRICW GetTextMetrics(Font font)
        {
            IntPtr hWnd = IntPtr.Zero;
            IntPtr hDC = GetDC(hWnd);
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

        /// <summary>
        /// Values are in Logical Units. For the default mapping mode, MM_TEXT, 1 logical unit is 1 pixel.
        /// For the desktop, the mapping mode is MM_TEXT. See Win32 'int GetMapMode(HDC hdc)'.
        /// For all intents and purposes, the values are in pixels.
        /// </summary>
        /// <remarks>
        /// See https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-textmetricw
        /// </remarks>
        [Serializable, StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TEXTMETRICW
        {
            public int tmHeight;           //The height (ascent + descent) of characters.
            public int tmAscent;           //The ascent (units above the base line) of characters.
            public int tmDescent;          //The descent(units below the base line) of characters.
            public int tmInternalLeading;  //The amount of leading (space) inside the bounds set by the tmHeight member. Accent marks and other diacritical characters may occur in this area.
            public int tmExternalLeading;  //The amount of extra leading (space) that the application adds between rows.
            public int tmAveCharWidth;     //The average width of characters in the font (generally defined as the width of the letter x ). This value does not include the overhang required for bold or italic characters.
            public int tmMaxCharWidth;     //The width of the widest character in the font.
            public int tmWeight;           //The weight of the font.
            public int tmOverhang;         //The extra width per string that may be added to some synthesized fonts such as Bold or Italic.
            public int tmDigitizedAspectX; //The horizontal aspect of the device for which the font was designed.
            public int tmDigitizedAspectY; //The vertical aspect of the device for which the font was designed.
            public ushort tmFirstChar;     //The value of the first character defined in the font.
            public ushort tmLastChar;      //The value of the last character defined in the font.
            public ushort tmDefaultChar;   //The value of the character to be substituted for characters not in the font.
            public ushort tmBreakChar;     //The value of the character that will be used to define word breaks for text justification.
            public byte tmItalic;          //Specifies an italic font if it is nonzero.
            public byte tmUnderlined;      //Specifies an underlined font if it is nonzero.
            public byte tmStruckOut;       //A strikeout font if it is nonzero.
            public byte tmPitchAndFamily;  //Specifies information about the pitch, the technology, and the family of a physical font.
            public byte tmCharSet;         //The character set of the font.
        }
    }

    ///  @image html FontMetrics.png
    /// <summary>
    /// Gets font metrics in pixels. This actually measures the bounding rectangle of the pixels in a drawn text image.
    /// This is great for vertically centering the visible area of the text.
    /// </summary>
    /// <remarks>
    /// This is appropriately scaled to the current DPI. See Win32.SetDpiAware()
    /// </remarks>
    public class ImageFontMetrics
    {
        //For ToString(): just keep private copy of font properties just in case font gets disposed.
        private readonly string _name;
        private readonly float _size;
        private readonly FontStyle _style;
        private readonly GraphicsUnit _unit;

        // https://docs.microsoft.com/en-us/windows/win32/gdi/string-widths-and-heights
        // http://csharphelper.com/blog/2014/08/get-font-metrics-in-c/
        //http://csharpexamples.com/fast-image-processing-c/

        public readonly int EmHeightPixels;
        //public readonly int AscentPixels;
        //public readonly int DescentPixels;
        public readonly int CellHeightPixels;
        public readonly int InternalLeadingPixels;
        public readonly int LineSpacingPixels;
        //public readonly int ExternalLeadingPixels;

        /// <summary>
        /// Initializes a new instance of the ImageFontMetrics class.
        /// </summary>
        /// <param name="font">Font to retrieve the font metric details from.</param>
        public ImageFontMetrics(Font font)
        {
            _name = font.Name; //save properties for ToString() just in case font gets disposed by caller.
            _size = font.Size;
            _style = font.Style;
            _unit = font.Unit;

            SizeF sz;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
                sz = g.MeasureString("Hy", font);

            Bitmap bmp = new Bitmap((int)(sz.Width+0.5f), (int)(sz.Height + 0.5f), PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
                g.DrawString("Hy", font, Brushes.Black, 0, 0);

            //var fn = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), "MetricsTest.png");
            //bmp.Save(fn, ImageFormat.Png);

            var rc = GetVisibleRectangle(bmp);
            bmp.Dispose(); //we're done with the bitmap

            EmHeightPixels = rc.Height;
            //AscentPixels = 0;
            //DescentPixels = 0;
            CellHeightPixels = rc.Height;
            InternalLeadingPixels = rc.Y;
            LineSpacingPixels = (int)(sz.Height + 0.5f) - rc.Bottom;
            //ExternalLeadingPixels = 0;
        }

        public override string ToString() => $"\"{_name}\", {_size}{UnitsString(_unit)}, {_style}";

        private static string UnitsString(GraphicsUnit unit)
        {
            switch (unit)
            {
                case GraphicsUnit.World: return "world";
                case GraphicsUnit.Display: return "display";
                case GraphicsUnit.Pixel: return "px";
                case GraphicsUnit.Point: return "pt";
                case GraphicsUnit.Inch: return "in";
                case GraphicsUnit.Document: return "doc";
                case GraphicsUnit.Millimeter: return "mm";
            }
            return string.Empty;
        }

        private Rectangle GetVisibleRectangle(Bitmap processedBitmap)
        {
            BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadOnly, processedBitmap.PixelFormat);

            int bytesPerPixel = Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
            int byteCount = bitmapData.Stride * processedBitmap.Height;
            byte[] pixels = new byte[byteCount];
            IntPtr ptrFirstPixel = bitmapData.Scan0;
            Marshal.Copy(ptrFirstPixel, pixels, 0, pixels.Length);
            int heightInPixels = bitmapData.Height;
            int widthInBytes = bitmapData.Width * bytesPerPixel;
            processedBitmap.UnlockBits(bitmapData);

            //var sb = new StringBuilder();
            //for(int i=3,w=0; i<pixels.Length; i += 4,w++)
            //{
            //    if ((w % bitmapData.Width) == 0) sb.AppendLine();
            //    sb.Append(pixels[i] == 0 ? "0" : "1");
            //    w++;
            //}
            //var result1 = sb.ToString();
            //sb.Length = 0;
            //for (int i = 0, w = 0; i < pixels.Length; i += 4, w++)
            //{
            //    if ((w % bitmapData.Width) == 0) sb.AppendLine();
            //    var m = MathEx.Max(pixels[i+0], pixels[i+1], pixels[i + 2], pixels[i + 3]);
            //    sb.Append(m == 0 ? "0" : "1");
            //    w++;
            //}
            //var result2 = sb.ToString();

            int top = -1, bottom = -1, left = -1, right = -1;

            for (int y = 0; y < heightInPixels; y++)
            {
                int currentLine = y * bitmapData.Stride;
                for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                {
                    //This assumes 4 bytes per pixel BGRA with a transparent background.
                    var a = pixels[currentLine + x + 3];

                    if (a != 0 && top == -1) top = y;
                    if (a != 0 && left == -1) left = x / bytesPerPixel;

                    if (a != 0 && right < x / bytesPerPixel + 1) right = x / bytesPerPixel + 1;
                    if (a != 0 && bottom < y + 1) bottom = y + 1;
                }
            }

            if (top == -1) top = 0;
            if (left == -1) left = 0;
            if (right == -1) right = processedBitmap.Width;
            if (bottom == -1) bottom = processedBitmap.Height;

            return Rectangle.FromLTRB(left, top, right, bottom);
        }
    }
}
