//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="InactivityTimer.cs" company="Chuck Hill">
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
//#define INACTIVITYDEBUG
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable 618 //warning CS0618: 'System.Threading.Thread.Suspend()' is obsolete: 'Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202'

namespace ChuckHill2.Forms
#pragma warning restore CS1030 // #warning directive
{
    /// <summary>
    /// Static logon session-wide inactivity timer. Event occurs when no mouse
    /// movement or keyboard input occurs after a specified timeout duration.
    /// </summary>
    public static class InactivityTimer //: IDisposable --but Dispose() exists anyway. This static object may be reused.
    {
        private static Int32 _timeoutDuration = 30 * 60 * 1000; //milliseconds. default == 30 minutes.
        private static Int32 _pollingPeriod = (Int32)(_timeoutDuration * 0.10); //10% of duration
        private static Thread _pollingThread = null;
        private static AutoResetEvent _exitEvent = null;
        private static volatile UInt32 IdleTime = 0;  //volatile enables sharing between threads.
        private static bool _suspended = false;

        /// <summary>
        /// Get time OS has been running. Due to the maximum size of a unsigned 32-bit int, 
        /// this value will rollover to zero after 50 days. Property Environment.TickCount, 
        /// uses this same underlying Win32 function, HOWEVER it converts the value to a signed int. 
        /// That means that the returned value will be a negative value after 25 days!
        /// </summary>
        /// <returns>System up time in milliseconds</returns>
        [DllImport("kernel32.dll")] private static extern uint GetTickCount();

        /// <summary>
        /// Useful debugging tool. Writes directly to debug output. 
        /// Will not show up in debugger output window. Must use DbgView utility.
        /// </summary>
        /// <param name="errmsg"></param>
        [DllImport("Kernel32.dll")] private static extern void OutputDebugString(string errmsg);
        [Conditional("INACTIVITYDEBUG")]
        public static void DebugWrite(string fmt, params object[] args) { OutputDebugString(string.Format(fmt,args)); }
        [Conditional("INACTIVITYDEBUG")]
        public static void DebugWrite(string msg) { OutputDebugString(msg); }

        /// <summary>
        /// Idle duration (in minutes) that is considered "inactive". Default=30 minutes.
        /// This may be set at any time and takes effect immediately.
        /// </summary>
        public static Int32 TimeoutDuration //minutes
        {
            get { return _timeoutDuration / 60000; }
            set 
            {
                _timeoutDuration = value * 60000;
                _pollingPeriod = (Int32)(_timeoutDuration * 0.10); //10% of duration
                DebugWrite("InactivityTimer.TimeoutDuration({0:00},{1:00}:{2:00})", value, _pollingPeriod / 60000, (_pollingPeriod/1000)%60);
            }
        }

        /// <summary>
        /// Checks if InactivityTimer is running. Useful for avoiding subcribing to events more than once.
        /// </summary>
        public static bool IsAlive { get { return (_pollingThread != null ? _pollingThread.IsAlive : false); } }

        /// <summary>
        /// Handler that is called periodically (e.g. every 10% of the timeout duration). 
        /// Check the handler event args for the inactivity status.
        /// Use Suspend/Resume to temporarily disable/enable the inactivity timer. 
        /// This handler is called on a separate thread, so for Forms UI, Control.Invoke()
        /// must be used in order to operate upon the form.
        /// </summary>
        public static event HeartbeatHandler Heartbeat;

        /// <summary>
        ///  Handler for the Heartbeat event.
        /// </summary>
        /// <param name="idleDuration">Amount of time (ms) since last mouse movement in any application window. Useful for impending timeout warning.</param>
        /// <param name="hasBeatActivity">True if mouse moved since last heartbeat.</param>
        /// <param name="timedOut">True if idle duration exceeds timeout.</param>
        public delegate void HeartbeatHandler(UInt32 idleDuration, bool hasBeatActivity, bool timedOut);

        /// <summary>
        /// Start the inactivity timer. Does nothing if already started.
        /// </summary>
        public static void Start()
        {
            if (_pollingThread != null) return;
            DebugWrite("InactivityTimer.Start()");
            _suspended = false;
            InitIdleReset(true); //enable low-level mouse polling
            _exitEvent = new AutoResetEvent(false);
            _pollingThread = new Thread(PollingThread);
            _pollingThread.Name = "InactivityTimer";
            _pollingThread.IsBackground = true;
            _pollingThread.Priority = ThreadPriority.BelowNormal;
            IdleTime = GetTickCount();
            _pollingThread.Start();
        }

        /// <summary>
        /// Stop the inactivity timer. Does nothing if already stopped.
        /// </summary>
        public static void Stop()
        {
            InitIdleReset(false); //disables and deallocates low-level mouse polling
            if (_pollingThread == null) return;
            DebugWrite("InactivityTimer.Stop()");
            _exitEvent.Set();
            _pollingThread.Join(3000);
            if (_pollingThread.IsAlive) _pollingThread.Abort();
            _pollingThread = null;
            _exitEvent.Close();
            _exitEvent = null;
            _suspended = false;
        }

        /// <summary>
        /// Temporarily suspend inactivity timer. Does nothing if already suspended.
        /// </summary>
        public static void Suspend()
        {
            DebugWrite("InactivityTimer.Suspend()");

            //Thread.Suspend() actually freezes the thread, including any callbacks. 
            //This is problematic if this is called within the callback because 
            //the callback execution will be frozen as well.
            _suspended = true;
        }
        /// <summary>
        /// Start and/or Resume stopped or suspended inactivity timer. Does nothing if timer is already resumed.
        /// </summary>
        public static void Resume()
        {
            DebugWrite("InactivityTimer.Resume()");
            InactivityTimer.Start();
            _suspended = false;
        }

        /// <summary>
        /// Stops the inactivity timer, deallocates all resources, and unsubscribes all event handlers.
        /// </summary>
        public static void Dispose()
        {
            DebugWrite("InactivityTimer.Dispose()");
            Stop();
            if (Heartbeat != null)
            {
                var invocationList = Heartbeat.GetInvocationList();
                if (invocationList != null) foreach (var handler in invocationList) Heartbeat -= (HeartbeatHandler)handler;
            }
        }

        private static void PollingThread()
        {
            UInt32 now;
            UInt32 idleDuration;
            UInt32 idleTime;
            bool timedOut;
            UInt32 lastIdleTime = GetTickCount();
            bool hasBeatActivity = false;
            IdleTime = lastIdleTime; //initialize to current upon startup

            try
            {
                while (!_exitEvent.WaitOne(_pollingPeriod)) //poll on wait timeout. break on event signaled.
                {
                    if (_suspended) continue; //do not execute events
                    idleTime = IdleTime;  //create our own private copy
                    now = GetTickCount();
                    hasBeatActivity = (lastIdleTime != idleTime);
                    lastIdleTime = idleTime;
                    idleDuration = (now < idleTime ? 0 : now - idleTime);  //handle 50-day rollover
                    timedOut = (idleDuration > _timeoutDuration);

                    if (Heartbeat != null)
                        try 
                        { 
                            DebugWrite("InactivityTimer.Heartbeat({0},{1},{2})", idleDuration, hasBeatActivity, timedOut);
                            Heartbeat(idleDuration, hasBeatActivity, timedOut);
                        }
                        catch(Exception)
                        {
                            DebugWrite("Warning: InactivityTimer.Heartbeat handler threw an error. Ignoring.");
                        }
                }
            }
            catch (Exception)
            {
                DebugWrite("Error: InactivityTimer polling thread terminated unexpectedly.");
            }
        }

        #region IdleTime reset by Win32 HookProc
        // Not only will this detect user activity in the app but it is also capable of 
        // detecting whether or not the App (not just a window) is active (title bar greyed out or not).
        // The idle time will NOT be reset if the app is inactive (all title bars greyed out.)

        #region Win32
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private const int WH_CALLWNDPROCRET = 12; //gets window management msgs AFTER window processed them
        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        private const int WH_CALLWNDPROC = 4;     //gets window management msgs BEFORE window processed them
        [StructLayout(LayoutKind.Sequential)]
        private struct CWPSTRUCT
        {
            public IntPtr lParam;
            public IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }

        private const int WH_GETMESSAGE = 3;      //gets keyboard, mouse, and timer msgs only
        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
        }

        [DllImport("user32.dll",SetLastError=true)] private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll",SetLastError=true)] private static extern int UnhookWindowsHookEx(IntPtr idHook);
        [DllImport("user32.dll",SetLastError=true)] private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        private const int WM_ACTIVATEAPP = 0x001C;
        private const int WM_ACTIVATE = 0x0006;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_NCMOUSEMOVE = 0x00A0;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SETFOCUS = 0x0007;
        private const int WM_KILLFOCUS = 0x0008;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)] private static extern int FormatMessage(FormatMsg dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, out IntPtr pMsg, int nSize, IntPtr Arguments);
        [Flags] private enum FormatMsg     //WINBASE.H
        {
            AllocateBuffer = 0x0100,
            IgnoreInserts = 0x0200,
            FromHModule = 0x0800,
            FromSystem = 0x1000
        }
        private static string GetHResultMessage() { return GetHResultMessage(Marshal.GetLastWin32Error()); }
        private static string GetHResultMessage(int hResult)
        {
            string sMsg = string.Empty;
            IntPtr pMsg = IntPtr.Zero;
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
        #endregion

        private static IntPtr _hActiveHook = IntPtr.Zero;
        private static IntPtr _hInputHook = IntPtr.Zero;
        private static readonly HookProc _activeHookProc = new HookProc(ActiveHookProc); //this MUST be static so it won't get garbage collected!
        private static readonly HookProc _inputHookProc = new HookProc(InputHookProc); //this MUST be static so it won't get garbage collected!
        private static bool _active = true;
        private static bool _focused = true;

        private static IntPtr ActiveHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hActiveHook, nCode, wParam, lParam);
            CWPRETSTRUCT m = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            switch (m.message)
            {
                case WM_SETFOCUS:
                    _focused = true;
                    DebugWrite("WM_SETFOCUS(0x{0:X8}) Activitated={1} Focused={2}",m.wParam, _active, _focused);
                    break;
                case WM_KILLFOCUS:
                    _focused = false;
                    DebugWrite("WM_KILLFOCUS(0x{0:X8}) Activitated={1} Focused={2}", m.wParam, _active, _focused);
                    break;
                case WM_ACTIVATEAPP:
                case WM_ACTIVATE:
                    _active = ((m.wParam.ToInt32()&0xFFFF) != 0);
                    DebugWrite("WM_ACTIVATE Activitated={0} Focused={1}", _active, _focused);
                    break;
            }
            return CallNextHookEx(_hActiveHook, nCode, wParam, lParam);
        }
        private static IntPtr InputHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hInputHook, nCode, wParam, lParam);
            MSG m = (MSG)Marshal.PtrToStructure(lParam, typeof(MSG));
            switch (m.message)
            {
                case WM_MOUSEMOVE:
                case WM_NCMOUSEMOVE:
                    if (_active && _focused)
                    {
                        IdleTime = GetTickCount();
                        DebugWrite("InactivityTimer.InputHookProc(WM_MOUSEMOVE)");
                    }
                    break;
                //Cannot use WM_KEYUP because when running in a remote RDP client, this routine recieves 
                //several WM_KEYUP messages for TAB, CONTROL, and SHIFT keys when restoring an RDP window.
                //case WM_KEYUP:
                //    if (_active && _focused)
                //    {
                //        IdleTime = GetTickCount();
                //        DebugWrite("InactivityTimer.InputHookProc(WM_KEYUP={0})", TranslateVK_KEY(m.wParam.ToInt32()));
                //    }
                //    break;
            }
            return CallNextHookEx(_hInputHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// This will initialize/deinitialize the Idle time reset by Windows hooks. 
        /// This is the only function public outside of this region.
        /// </summary>
        /// <param name="doIt">true to initialize, false to dispose</param>
        private static void InitIdleReset(bool doIt)
        {
            if (doIt)
            {
                if (_hActiveHook != IntPtr.Zero) return;
                #pragma warning disable 0618  //we need the REAL threadID for Win32, not the bogus one .NET generates
                _hActiveHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _activeHookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
                if (_hActiveHook == IntPtr.Zero) DebugWrite("Error: InactivityTimer.SetWindowsHookEx(WH_CALLWNDPROCRET) failed.\n{0}", GetHResultMessage());
                _hInputHook = SetWindowsHookEx(WH_GETMESSAGE, _inputHookProc, IntPtr.Zero, AppDomain.GetCurrentThreadId());
                if (_hInputHook == IntPtr.Zero) DebugWrite("Error: InactivityTimer.SetWindowsHookEx(WH_GETMESSAGE) failed.\n{0}", GetHResultMessage());
                #pragma warning restore 0618
            }
            else
            {
                if (_hActiveHook == IntPtr.Zero) return;
                UnhookWindowsHookEx(_hActiveHook);
                UnhookWindowsHookEx(_hInputHook);
                _hActiveHook = IntPtr.Zero;
                _hInputHook = IntPtr.Zero;
            }
        }
        #endregion

        #region IdleTime reset by Application MessageFilter
        //We cannot use the Application.Idle event because there must be NO messages 
        //left in the message queue. Over the span of our activity timeout there will 
        //always be non-user messages of some sort (e.g. WM_TIMER, etc).
        //
        //We cannot use Win32 GetLastInputInfo() because it
        //spans the entire desktop, not just our application.
        //
        //Application.AddMessageFilter() works but recieves mouse/keyboard messages  
        //even when the app is not active (e.g. title bar grayed out). There is no 
        //way to detect if the app is active or not throughout the entire application 
        //within C#. See Windows Hooks.

        private static readonly UserActivityDetector _activityDetector = new UserActivityDetector();
        private class UserActivityDetector : IMessageFilter
        {
            //Windows messages that define user activity
            private const int WM_NCMOUSEMOVE = 0x00A0;
            private const int WM_KEYUP = 0x0101;
            private const int WM_MOUSEMOVE = 0x0200;
            public bool PreFilterMessage(ref Message m)
            {
                switch (m.Msg)
                {
                    //case WM_ACTIVATE: These messages are never recieved so there is no way to detect if the app is active or not!
                    case WM_MOUSEMOVE:
                    case WM_NCMOUSEMOVE:
                    case WM_KEYUP:
                        IdleTime = GetTickCount();
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// This will initialize/deinitialize the Idle time reset by C# Application Message filter. 
        /// This is the only function public outside of this region.
        /// </summary>
        /// <param name="doIt">true to initialize, false to dispose</param>
        private static void InitIdleReset2(bool doIt)
        {
            if (doIt)
            {
                Application.AddMessageFilter(_activityDetector);
            }
            else
            {
                Application.RemoveMessageFilter(_activityDetector);
            }
        }
        #endregion
    }
}
