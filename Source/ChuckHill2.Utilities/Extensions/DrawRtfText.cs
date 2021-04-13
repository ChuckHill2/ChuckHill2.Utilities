//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="DrawRtfText.cs" company="Chuck Hill">
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
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ChuckHill2.Extensions
{
    public static class GraphicsExtension
    {
        [DllImport("USER32.dll")]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        private const int WM_USER = 0x400;
        private const int EM_FORMATRANGE = WM_USER + 57;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHARRANGE
        {
            public int cpMin;
            public int cpMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FORMATRANGE
        {
            public IntPtr hdc;
            public IntPtr hdcTarget;
            public RECT rc;
            public RECT rcPage;
            public CHARRANGE chrg;
        }

        private const double inch = 14.4;

        //https://www.codeproject.com/Articles/13197/Alpha-Blended-Transparent-Capable-TextBox-and-Rich
        //https://social.msdn.microsoft.com/Forums/en-US/1454d078-c312-4741-88df-aa7eb306fe51/how-to-save-the-content-of-richtextbox-as-jpg-file?forum=winforms
        private static Bitmap DrawRtfTextToImage(RichTextBox rtb, Rectangle rectangle)
        {
            Bitmap bmp = new Bitmap(rectangle.Width, rectangle.Height);

            using (Graphics gr = Graphics.FromImage(bmp))
            {
                IntPtr hDC = gr.GetHdc();
                FORMATRANGE fmtRange;
                RECT rect;
                int fromAPI;
                rect.Top = 0; rect.Left = 0;
                rect.Bottom = (int)(bmp.Height + (bmp.Height * (bmp.HorizontalResolution / 100)) * inch);
                rect.Right = (int)(bmp.Width + (bmp.Width * (bmp.VerticalResolution / 100)) * inch);
                fmtRange.chrg.cpMin = 0;
                fmtRange.chrg.cpMax = -1;
                fmtRange.hdc = hDC;
                fmtRange.hdcTarget = hDC;
                fmtRange.rc = rect;
                fmtRange.rcPage = rect;
                int wParam = 1;
                IntPtr lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fmtRange));
                Marshal.StructureToPtr(fmtRange, lParam, false);
                fromAPI = SendMessage(rtb.Handle, EM_FORMATRANGE, wParam, lParam);
                Marshal.FreeCoTaskMem(lParam);
                fromAPI = SendMessage(rtb.Handle, EM_FORMATRANGE, wParam, new IntPtr(0));
                gr.ReleaseHdc(hDC);
            }

            return bmp;
        }

        /// <summary>
        /// Draw RTF string content at a specified location and size.
        /// </summary>
        /// <param name="graphics">Graphics to draw RTF content into.</param>
        /// <param name="rtf">RTF string content</param>
        /// <param name="backColor">Background color for the RTF content. This cannot be transparent or translucent.</param>
        /// <param name="layoutArea">The bounding rectangle the RTF will be drawn into. It will autowrap as necessary.</param>
        /// <exception cref="System.ArgumentException">This does not support transparent background colors.</exception>
        /// <remarks>
        /// Control.DrawToBitmap() does not work with the RichTextBox() control so we support it here.<br />
        /// This method removes all the background color from the image, however edge effects occur around the characters where antialiasing 
        /// makes the edges not strictly the background color. This background removal is useful when the layout area partially overlaps other 
        /// features in the image selected into the Graphics object. Also, images embedded in the RTF always have a transparent color of White.<br />
        /// The default background color is White, so this works best.
        /// </remarks>
        public static void DrawRtf(this Graphics graphics, string rtf, Color backColor, Rectangle layoutArea)
        {
            // There are better ways to do this but the code grows exponentially!!
            var c = new RichTextBox();
            c.BorderStyle = BorderStyle.None;
            c.ScrollBars = RichTextBoxScrollBars.None;
            c.Size = new Size((int)layoutArea.Width, (int)layoutArea.Height);
            c.Rtf = rtf;

            //Re: Transparency: We cannot simply fake it out by using CreateParams:WS_EX_TRANSPARENT because the resulting drawn text looks really terrible.
            if (!backColor.IsEmpty) c.BackColor = backColor;

            var bmp = DrawRtfTextToImage(c, layoutArea);

            bmp.MakeTransparent(c.BackColor);
            //bmp.MakeTransparent(Color.White);  //for transparent embedded images

            //Now, place the generated image at the correct offset in the parent graphics image.
            graphics.DrawImage(bmp, layoutArea); 
            bmp.Dispose();
            c.Dispose();
        }
    }
}
