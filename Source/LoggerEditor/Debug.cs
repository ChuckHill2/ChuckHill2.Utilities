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
