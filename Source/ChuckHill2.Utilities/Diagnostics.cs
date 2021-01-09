using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

#pragma warning disable 1591 //warning CS1591: Missing XML comment for publicly visible type or member

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Primitive Developer one-off debugging utilities. 
    /// For more comprehensive usage, use: var log = new ChuckHill2.Utilities.Log("General",SourceLevels.All);
    /// </summary>
    public static class DBG
    {
        /// <summary>
        /// True to enable built-in developer debug trace logging. Useful for release debugging. 
        /// "ChuckHill2.Utilities.DBG.DevLoggingEnabled" App.config entry determines the default enable
        /// state. If the entry is missing or the App.config does not exist, the default is false.
        /// This flag may be enabled or disabled at any time.
        /// </summary>
        public static bool Enabled { get; set; }

        #region RawWrite() Initialization
        /// <summary>
        /// Write string to debug output. 
        /// Uses Win32 OutputDebugString() or System.Diagnostics.Trace.Write() if running under a debugger.
        /// The reason for all this trickery is due to the fact that OutputDebugString() output DOES NOT get
        /// written to VisualStudio output window. Trace.Write() does write to the VisualStudio output window
        /// (by virtue of OutputDebugString somewhere deep inside), BUT it also is can be redirected
        /// to other destination(s) in the app config. This API Delegate is a compromise.
        /// </summary>
        private static readonly WriteDelegate _rawWrite;  //also directly used by class ChuckHill2.Utilities.FormattedDebugTraceListener.
        private delegate void WriteDelegate(string msg);
        [DllImport("Kernel32.dll")]
        private static extern void OutputDebugString(string errmsg);
        private static readonly TraceListener trace;
        public static void RawWrite(string msg)
        {
            //Prefix diagnostic message with something unique that can be filtered upon by DebugView.exe
            _rawWrite("DBG: " + msg);
        }
        #endregion

        static DBG() //Retrieve defaults upon assembly load.
        {
            DBG.Enabled = ConfigEnabled("ChuckHill2.Utilities.DBG.DevLoggingEnabled");
            string enabled = Environment.GetEnvironmentVariable("DevLoggingEnabled");
            if (!string.IsNullOrWhiteSpace(enabled))
            {
                char c = enabled[0];
                if (c == '1' || c == 't' || c == 'T' || c == 'y' || c == 'Y') DBG.Enabled = true;
            }

            #region RawWrite() Initialization
            //Initialize our raw DBG.Write() method.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //using this bypasses [Conditional("DEBUG")] on Debug.Write()
                trace = new System.Diagnostics.DefaultTraceListener();
                _rawWrite = trace.Write;
            }
            else _rawWrite = OutputDebugString;
            #endregion
        }
        //Set default startup state from app config file.
        private static bool ConfigEnabled(string appSetting)
        {
            try
            {
                object o = ConfigurationManager.AppSettings[appSetting];
                if (o == null) return false;
                string s = o.ToString().Trim();
                if (s.Length == 0) return false;
                char c = s[0];
                return (c == '1' || c == 't' || c == 'T' || c == 'y' || c == 'Y');
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Add/remove custom trace log writer. NewLine's are automatically appended.
        /// Handy for capturing this loggging output for further processing 
        /// (e.g. writing to a status window).
        /// </summary>
        public static event Action<string> LogWriter;

        /// <summary>
        /// Sends a string to the debugger output window only if DBG.Enabled==true
        /// See event DBG.LogWriter for adding custom loggers.
        /// </summary>
        /// <param name="format">string format. see String.Format()</param>
        /// <param name="args">variable argument list</param>
        public static void WriteLine(string format, params object[] args)
        {
            if (!DBG.Enabled) return;
            string s = string.Format(format, args) + Environment.NewLine;
            RawWrite(s);
            if (LogWriter != null) try { LogWriter(s); }
                catch { }
        }
        /// <summary>
        /// Sends a string to the debugger output window only if DBG.Enabled==true
        /// See event DBG.LogWriter for adding custom loggers.
        /// </summary>
        /// <param name="message">message to write</param>
        public static void WriteLine(string message)
        {
            if (!DBG.Enabled) return;
            string s = message + Environment.NewLine;
            RawWrite(s);
            if (LogWriter != null) try { LogWriter(s); }
                catch { }
        }
    }
}
