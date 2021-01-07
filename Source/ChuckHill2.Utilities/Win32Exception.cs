using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Exception to throw when a p/Invoked API fails. This is better than 
    /// System.ComponentModel.Win32Exception because it also translates the 
    /// Win32 exception code into a formatted string.
    /// </summary>
    public class Win32Exception : System.ComponentModel.Win32Exception
    {
        #region [Win32]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern int FormatMessage(FormatMsg dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, out IntPtr pMsg, int nSize, IntPtr Arguments);
        [Flags] private enum FormatMsg     //WINBASE.H
        {
            AllocateBuffer = 0x0100,
            IgnoreInserts = 0x0200,
            FromHModule = 0x0800,
            FromSystem = 0x1000
        }
        [DllImport("kernel32.dll")] private static extern void SetLastError(int dwErrCode);
        #endregion

        #region Private Static Methods
        //Extracted from: System.Diagnostics.CommonExtensions.cs: ExceptionUtils.AppendMessage()
        //This source file is linked to many projects so we cannot use any other utility functions.
        private static readonly FieldInfo _message = typeof(Exception).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);
        private static Exception SetExceptionMessage(Exception ex, string msg) { _message.SetValue(ex, msg); return ex; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the ChuckHill2.Utilities.Win32Exception class
        /// with the last Win32 error that occurred.
        /// </summary>
        public Win32Exception() : this(string.Empty, Marshal.GetLastWin32Error(), string.Empty) { }
        /// <summary>
        /// Initializes a new instance of the ChuckHill2.Utilities.Win32Exception class
        /// with the last Win32 error that occurred with a specified detailed description.
        /// </summary>
        /// <param name="msg">A detailed description of the error.</param>
        public Win32Exception(string msg) : this(string.Empty, Marshal.GetLastWin32Error(), msg) { }
        /// <summary>
        /// Initializes a new instance of the System.ComponentModel.Win32Exception class
        /// with the specified error and the specified detailed description.
        /// </summary>
        /// <param name="hResult">The Win32 error code associated with this exception.</param>
        /// <param name="msg">A detailed description of the error.</param>
        public Win32Exception(int hResult, string msg) : this(string.Empty, hResult, msg) { }
        /// <summary>
        /// Initializes a new instance of the ChuckHill2.Utilities.Win32Exception class
        /// with the last Win32 error that occurred, the name of the Win32 function that 
        /// caused this error, and a specified detailed description.
        /// </summary>
        /// <param name="pInvokeEntryPoint">Name of the Win32 function that caused this error</param>
        /// <param name="msg">A detailed description of the error.</param>
        public Win32Exception(string pInvokeEntryPoint, string msg) : this(pInvokeEntryPoint, Marshal.GetLastWin32Error(), msg) { }
        /// <summary>
        /// Initializes a new instance of the ChuckHill2.Utilities.Win32Exception class
        /// with the name of the Win32 function that caused this error, the specified 
        /// error, and the specified detailed description.
        /// </summary>
        /// <param name="pInvokeEntryPoint">Name of the Win32 function that caused this error</param>
        /// <param name="hResult">The Win32 error code associated with this exception.</param>
        /// <param name="msg">A detailed description of the error.</param>
        public Win32Exception(string pInvokeEntryPoint, int hResult, string msg) : base(msg)
        {
            base.HResult = hResult;
            base.Source = pInvokeEntryPoint;
            SetExceptionMessage(this, this.Message + Environment.NewLine + GetLastErrorMessage(base.HResult));
        }

        /// <summary>
        /// Translate the Win32 Error code into the eqivalant error message string.
        /// </summary>
        /// <param name="hResult">Last error code to translate. If undefined, use current last error code.</param>
        /// <returns>Error message string. If unable to translate, returns "Unknown error [errorcode]"</returns>
        public static string GetLastErrorMessage(int hResult=-1)
        {
            string sMsg = string.Empty;
            IntPtr pMsg = IntPtr.Zero;
            if (hResult == -1) hResult = Marshal.GetLastWin32Error();

            FormatMsg flags = FormatMsg.AllocateBuffer | FormatMsg.IgnoreInserts | FormatMsg.FromSystem;
            try
            {
                int dwBufferLength = FormatMessage(flags, IntPtr.Zero, hResult, 0, out pMsg, 0, IntPtr.Zero);
                if (dwBufferLength != 0) sMsg = Marshal.PtrToStringUni(pMsg).TrimEnd('\r', '\n');
            }
            catch { }
            finally
            {
                if (pMsg != IntPtr.Zero) Marshal.FreeHGlobal(pMsg);
            }
            return (sMsg.Length == 0 ? "Unknown error " + hResult.ToString() : sMsg);
        }

        /// <summary>
        /// Clears the last Win32 error code.
        /// Win32 API do not reset the last error code. This utility will reset the 
        /// last error code to 0 (aka success), so if a Win32 error does occur, the 
        /// lastError code will be set due to the Win32 API we are using.
        /// </summary>
        public static void ClearLastError() { SetLastError(0); }

        /// <summary>
        /// Retrieve the last Win32 error code. Equivalant to Marshal.GetLastWin32Error().
        /// </summary>
        /// <returns>Win32 Error code</returns>
        public static int GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }
    }
}
