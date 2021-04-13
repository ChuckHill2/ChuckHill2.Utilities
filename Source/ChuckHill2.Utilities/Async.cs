//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Async.cs" company="Chuck Hill">
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
using System.Collections;
using System.Threading;

namespace ChuckHill2
{
    /// <summary>
    /// Non-GUI asynchronous event executor.  It does not require a System.Windows.Forms object to operate.  
    /// It is similar in functionality to Control.BeginInvoke()/EndInvoke() however this will never conflict
    /// or be queued with other commands since it runs on its own separate, independent, and private thread.
    /// It is also similar in functionality to ThreadPool.QueueUserWorkItem() but threadpool threads belong 
    /// to MultiThreadedApartment(MTA). Any .NET API that uses OLE behind the scenes, including any using 
    /// System.Interop, will cause an exception when executed within the ThreadPool!
    /// </summary>
    public class Async
    {
        private Thread         m_Thread      = null;
        private AutoResetEvent m_TriggerEvent= null;
        private AutoResetEvent m_ExitEvent   = null;
        private Callback       m_func;
        private Queue          m_queue = System.Collections.Queue.Synchronized(new System.Collections.Queue());

        /// <summary>
        /// Async callback delegate. 
        /// </summary>
        /// <param name="userValues">Value to pass to the callback/delegate function. May be null if not used.</param>
        /// <returns>true to continue waiting for the next invocation Trigger() or false to terminate thread and cleanup. 
        /// Exit() does not need to be called (although it doesn't hurt) when returning false.</returns>
        public delegate bool Callback(object[] userValues);

        /// <summary>
        /// Handy class for calling a function asynchronously, and repeatedly. Upon completion, Exit() must be called to close the thread.
        /// Example: Async m_Async = new Async("CopyToClipboard", new Async.Callback(this.CopyToClipboard), null);
        /// </summary>
        /// <param name="threadName">Optional name for thread.</param>
        /// <param name="func">Callback function: bool Callback(Object userValue)  
        /// Return true to continue waiting for next invocation trigger or false to exit thread and cleanup. 
        /// Returning false is useful for one-shot asynchronous calls.</param>
        public Async(string threadName, Callback func)
        {
            m_func = func;
            m_TriggerEvent= new System.Threading.AutoResetEvent(false);
            m_ExitEvent   = new System.Threading.AutoResetEvent(false);
            m_Thread      = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProc));
            m_Thread.Name = threadName;
            m_Thread.IsBackground = true;  //Allow system to throw a ThreadAbortException to exit the thread upon program exit.
            m_Thread.Start();
        }

        /// <summary>
        /// Only necessary if the callback function is hung or extremly busy.
        /// </summary>
        public void Kill() { if (m_Thread!=null) m_Thread.Abort(); }

        /// <summary>
        /// Execute the registered callback function asynchronously. This function returns immediately.
        /// </summary>
        public void Trigger() { Trigger(null); }
        /// <summary>
        /// Execute the registered callback function asynchronously. This function returns immediately.
        /// </summary>
        /// <param name="objs">Args to pass to registered callback function</param>
        public void Trigger(object[] objs)
        {
            if (m_TriggerEvent==null) return;
            m_queue.Enqueue(objs); //access is thread-safe
            m_TriggerEvent.Set();
        }

        /// <summary>
        /// Remove all outstanding Trigger/jobs from queue.
        /// </summary>
        public void FlushQueue() { m_queue.Clear(); }

        /// <summary>
        /// Get current number of items in queue.
        /// </summary>
        public int QueueCount() { return m_queue.Count; }

        /// <summary>
        /// Stops thread and cleans up this Async object. 
        /// Useful particularly within Form.Dispose(). Add the line: "if (m_Async!=null) m_Async.Exit();"
        /// This is only necessary if you want to cleanup before application exit to avoid resource leaks during runtime.
        /// </summary>
        public void Exit() 
        {	
            try
            {
                if (m_ExitEvent!=null) 
                { 
                    m_ExitEvent.Set();
                    if (m_Thread!=null) m_Thread.Join(500);
                    m_Thread = null; 
                }
            }
            catch {}
        }

        /// <summary>
        /// Check to see if thread is still running.
        /// </summary>
        /// <returns>true if it still running.</returns>
        public bool IsRunning() { return (m_Thread==null?false:m_Thread.IsAlive); }

        private void ThreadProc() 
        {
            System.Threading.WaitHandle[] handles = new System.Threading.WaitHandle[]{m_ExitEvent,m_TriggerEvent};
            int handleIndex;
            try
            {
                while(true)
                {
                    handleIndex = System.Threading.WaitHandle.WaitAny(handles);
                    if (handleIndex==0) return; //Exit() was called
                    try
                    {
                        while(m_queue.Count>0)
                        {
                            object[] objs = (object[])m_queue.Dequeue();
                            if (m_func(objs)==false) Exit();
                        }
                    }
                    catch(Exception e) //never allow an m_func() exception terminate this thread.
                    {
                        //synchronous Log uses Async class so we can't use it here
                        DBG.WriteLine("Thread: {0}\r\n{1}",(m_Thread.Name==null?"<unknown>":m_Thread.Name),e);
                    }
                }
            }
            finally
            {
                if (m_ExitEvent!=null) { m_ExitEvent.Close(); m_ExitEvent = null; }
                if (m_TriggerEvent!=null) { m_TriggerEvent.Close(); m_TriggerEvent = null; }
                m_queue.Clear();
                m_queue = null;
            }
        }
    }
}
