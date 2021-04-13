//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Debug.cs" company="Chuck Hill">
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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChuckHill2.LoggerEditor
{
    public static class Debug
    {
        [DllImport("Kernel32.dll")]
        private static extern void OutputDebugString(string errmsg);
        private static readonly TraceListener trace;
        private static readonly Action<string> _rawWrite;

        static Debug()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                trace = new System.Diagnostics.DefaultTraceListener();
                _rawWrite = trace.Write;
            }
            else _rawWrite = OutputDebugString;
        }
        public static void Write(string msg)
        {
            _rawWrite(msg);
        }
        public static void Write(string fmt, params Object[] args)
        {
            string msg = string.Format(fmt, args);
            _rawWrite(msg);
        }
        public static void WriteLine(string msg)
        {
            _rawWrite(msg + Environment.NewLine);
        }
        public static void WriteLine(string fmt, params Object[] args)
        {
            string msg = string.Format(fmt, args) + Environment.NewLine;
            _rawWrite(msg);
        }
    }
}
