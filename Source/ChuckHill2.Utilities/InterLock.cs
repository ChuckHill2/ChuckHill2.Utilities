//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Interlock.cs" company="Chuck Hill">
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
using System.Threading;

namespace ChuckHill2
{
    /// <summary>
    /// Provides a mechanism that synchronizes access to objects across all threads, processes, and appdomains.
    /// This may be manually disposed or it will be automatically disposed at process (or appdomain) exit.
    /// When all instances across all processes are disposed, it is disposed by the OS.
    /// </summary>
    public class InterLock : IDisposable
    {
        Mutex mutex = null;

        /// <summary>
        /// Initialize this new locking object.
        /// </summary>
        /// <param name="name">Key name to lock upon. Must not be null or empty.</param>
        public InterLock(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            mutex = new Mutex(false, name);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e) => Dispose();
        private void CurrentDomain_DomainUnload(object sender, EventArgs e) => Dispose();
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => Dispose();

        /// <summary>
        /// Acquires an exclusive lock.
        /// </summary>
        /// <param name="millisecondsTimeout">Amount of time to wait before timeout. or -1 to wait forever.</param>
        /// <returns>True if successful or false if timed out.</returns>
        public bool Lock(int millisecondsTimeout = -1)
        {
            //if (mutex == null) return true; //let it throw a null exception if it is already disposed.
            try
            {
                // acquire the mutex (or timeout after 60 seconds)
                // will return false if it timed out
                return mutex.WaitOne(millisecondsTimeout);
            }
            catch (AbandonedMutexException)
            {
                // abandoned mutexes are still acquired, we just need
                // to handle the exception and treat it as acquisition
                return true;
            }
        }

        /// <summary>
        /// Releases the exclusive lock.
        /// </summary>
        public void Unlock()
        {
            mutex?.ReleaseMutex();
        }

        public void Dispose()
        {
            if (mutex == null) return;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.DomainUnload -= CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit -= CurrentDomain_ProcessExit;
            mutex.Dispose();
            mutex = null;
        }
    }
}
