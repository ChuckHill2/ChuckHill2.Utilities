//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="DateTimeEx.cs" company="Chuck Hill">
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
using System.Runtime.InteropServices;

namespace ChuckHill2
{
    public class DateTimeEx
    {
        /// <summary>
        /// Unix beginning of time.
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

        #region public static long UtcTimerTicks
        [DllImport("Kernel32.dll")]
        private static extern void GetSystemTimeAsFileTime(out long SystemTimeAsFileTime);
        /// <summary>
        /// UTC time in ticks. Lowest overhead. Great for determining duration.
        /// </summary>
        public static long UtcTimerTicks
        {
            get
            {
                GetSystemTimeAsFileTime(out var ticks);
                return ticks + 0x0701ce1722770000; //offset from 1/1/1601 to 1/1/0001
            }
        }
        #endregion public static long UtcTimerTicks

        #region public static long LocalTimerTicks
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        [DllImport("Kernel32.dll")]
        private static extern bool SystemTimeToTzSpecificLocalTime(IntPtr lpTimeZoneInformation, ref SYSTEMTIME utcTime, out SYSTEMTIME localTime);
        [DllImport("Kernel32.dll")]
        private static extern bool FileTimeToSystemTime(ref long ticks, out SYSTEMTIME localTime);
        [DllImport("Kernel32.dll")]
        private static extern bool SystemTimeToFileTime(ref SYSTEMTIME systime, out long lpFileTime);

        /// <summary>
        /// Local time in ticks. Takes a little more time to compute than UtcTimerTicks.
        /// </summary>
        public static long LocalTimerTicks
        {
            get
            {
                GetSystemTimeAsFileTime(out var utcTicks);
                utcTicks += 0x0701ce1722770000; //offset from 1/1/1601 to 1/1/0001
                FileTimeToSystemTime(ref utcTicks, out var utcSystemTime);
                SystemTimeToTzSpecificLocalTime(IntPtr.Zero, ref utcSystemTime, out var localSystemTme);
                SystemTimeToFileTime(ref localSystemTme, out long localTicks);
                return localTicks;
            }
        }
        #endregion public static long LocalTimerTicks
    }
}
