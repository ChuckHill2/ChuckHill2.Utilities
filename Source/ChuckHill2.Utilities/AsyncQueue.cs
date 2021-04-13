//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="AsyncQueue.cs" company="Chuck Hill">
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ChuckHill2
{
    /// <summary>
    /// A queue of asynchronous jobs where there is a maximum of concurrent jobs that may run.
    /// Thread/Job management is much more fine-grained than ThreadPool alone.
    /// Useful for running recursive jobs with a common action.
    /// This leverages System.Threading.ThreadPool.
    /// </summary>
    /// <typeparam name="T">Type of job data that may be operated upon.</typeparam>
    public class AsyncQueue<T> : IDisposable where T : class
    {
        private Object m_countdownObj = new Object();
        private List<Thread> myThreadPool;
        private Queue<T> m_jobQueue;
        private List<T> m_runningJobList;
        private Semaphore m_runningJobsSemaphore;
        private AutoResetEvent m_TriggerEvent;
        private AutoResetEvent m_StopEvent;
        private Thread m_Thread;
        private volatile int countdown;

        #region Properties
        private int __maxConcurrentJobs = 20;
        /// <summary>
        /// Set the maximum number of jobs that may run concurrently.
        /// Can only be set when there are no jobs in the queue.
        /// If set to 1, then this is effectively synchronous.
        /// </summary>
        public int MaxConcurrentJobs
        {
            get => __maxConcurrentJobs;
            set
            {
                if (__maxConcurrentJobs==value) return;
                lock (m_jobQueue)
                {
                    if (this.QueueCount!=0)
                    {
                        Log(TraceEventType.Warning, "Cannot change MaxConcurrentJobs until job queue is empty.");
                        return;
                    }
                    if (value < 1)
                    {
                        Log(TraceEventType.Warning, "MaxConcurrentJobs cannot be less than one.");
                        return;
                    }

                    //By default, there are a max of threadpool threads == (number of CPUs)*25 so it would be rare that we would exceed this number.
                    ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
                    if (value > workerThreads) ThreadPool.SetMaxThreads(value, completionPortThreads);

                    if (m_runningJobsSemaphore!=null) m_runningJobsSemaphore.Dispose();
                    __maxConcurrentJobs = value;
                    m_runningJobsSemaphore = new Semaphore(__maxConcurrentJobs, __maxConcurrentJobs);
                }
            }
        }

        /// <summary>
        /// The message logger to write AsyncQueue messages to as AsyncQueue does not throw exceptions. This message logger may run within multiple threads so it must be multi-thread compliant.
        /// </summary>
        public Action<TraceEventType, string, Object[]> Logger { get; set; }
        private void Log(TraceEventType severity, string format, params Object[] args) { if (Logger != null) Logger(severity, format, args); }

        /// <summary>
        /// User-defined data associated with this instance of AsyncQueue. See NotifyQueueIdle.
        /// </summary>
        public Object UserData { get; set; }

        /// <summary>
        /// Notify caller when queue is finally empty and all jobs have completed or when Abort() is called.
        /// The value of _UserData_ is passed to this user method.
        /// </summary>
        public Action<Object> NotifyQueueIdle { get; set; }

        /// <summary>
        /// Get count of running jobs at this moment
        /// </summary>
        /// <returns></returns>
        public int RunningCount { get { lock (m_countdownObj) return countdown; } }

        /// <summary>
        /// Get number of jobs currently in queue that are not yet running.
        /// </summary>
        /// <returns></returns>
        public int QueueCount { get { lock (m_jobQueue) return m_jobQueue.Count; } }

        /// <summary>
        /// Total jobs incomplete (running + queued)
        /// </summary>
        public int PendingCount => RunningCount + QueueCount;

        /// <summary>
        /// Asynchronous action to execute upon object T
        /// </summary>
        public Action<T> JobExecutor { get; set; }
        #endregion //Properties

        /// <summary>
        /// Asychronous queue constructor
        /// </summary>
        /// <param name="maxConcurrentJobs">
        ///   The maximum number of jobs that may run concurrently.
        ///   If set to 1, then this is effectively synchronous.
        /// </param>
        public AsyncQueue(int maxConcurrentJobs = 20)
        {
            m_jobQueue = new Queue<T>();
            MaxConcurrentJobs = maxConcurrentJobs;
            m_runningJobList = new List<T>(maxConcurrentJobs);
            myThreadPool = new List<Thread>(maxConcurrentJobs);

            m_TriggerEvent = new AutoResetEvent(false);
            m_StopEvent = new AutoResetEvent(false);
            m_Thread = new Thread(new ThreadStart(ThreadProc));
            m_Thread.Name = "Async Job Queue";
            m_Thread.IsBackground = true;  //Allow system to throw a ThreadAbortException to exit the thread upon program exit.
            m_Thread.Start();
        }

        private void ThreadProc()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        if (WaitHandle.WaitAny(new WaitHandle[] { m_StopEvent, m_TriggerEvent }) < 1) { return; }
                        while (this.QueueCount > 0)
                        {
                            if (WaitHandle.WaitAny(new WaitHandle[] { m_StopEvent, m_runningJobsSemaphore }) < 1) { return; }
                            T ja = Dequeue();
                            if (JobExecutor== null || ja == null)
                            {
                                if (!IsClosed(m_runningJobsSemaphore)) m_runningJobsSemaphore.Release();
                                continue;
                            }

                            ThreadPool.QueueUserWorkItem(delegate(object state)
                            {
                                #region delegate
                                bool aborted = false;
                                int runningJobIndex = -1;
                                int runningThreadIndex = -1;
                                try
                                {
                                    lock (m_runningJobList) runningJobIndex = AddIndexed(m_runningJobList, ja);
                                    lock (myThreadPool) runningThreadIndex = AddIndexed(myThreadPool, Thread.CurrentThread);
                                    lock (m_countdownObj) countdown++;
                                    //System.Diagnostics.Debug.WriteLine("Debug: countdown=" + countdown);
                                    JobExecutor(ja);
                                }
                                catch (ThreadAbortException) { aborted = true; }
                                catch (Exception) { /*handled by caller*/ }
                                finally
                                {
                                    if (!aborted)
                                    {
                                        lock (m_countdownObj) countdown--;
                                        lock (m_runningJobList) RemoveIndexed(m_runningJobList, runningJobIndex);
                                        lock (myThreadPool) RemoveIndexed(myThreadPool, runningThreadIndex);
                                        if (!IsClosed(m_runningJobsSemaphore)) m_runningJobsSemaphore.Release();
                                        if (this.QueueCount == 0 && countdown == 0 && NotifyQueueIdle != null) NotifyQueueIdle(this.UserData);
                                    }
                                }
                                #endregion
                            });
                        }
                    }
                    catch (ThreadAbortException) { throw; }
                    catch (Exception ex) { Log(TraceEventType.Error, "Queueing. Continuing...\r\n\t{0}",ex.Message); }
                }
            }
            catch (ThreadAbortException)
            {
                lock (myThreadPool) foreach (Thread th in myThreadPool)
                {
                    if (th != null && th.IsAlive)
                        try { th.Abort(); } catch { }
                }
                Log(TraceEventType.Warning, "Queueing Thread Aborted by User.");
            }
            catch (Exception ex)
            {
                Log(TraceEventType.Error, "Queueing Thread Abnormal Exit.\r\n\t{0}", ex.Message);
                return;
            }
            Log(TraceEventType.Information, "Report Queueing Thread Exited Normally.");
        }

        /// <summary>
        /// Submit job data to queue and execute
        /// </summary>
        /// <param name="ja"></param>
        public void Enqueue(T ja)
        {
            lock (m_jobQueue)
            {
                if (m_jobQueue.Contains(ja)) return; //disallow duplicate jobs
                m_jobQueue.Enqueue(ja);
                if (!IsClosed(m_TriggerEvent)) m_TriggerEvent.Set();
            }
        }

        /// <summary>
        /// Retrieve and remove job data from the queue.
        /// </summary>
        /// <returns></returns>
        public T Dequeue()
        {
            lock (m_jobQueue)
            {
                if (m_jobQueue.Count == 0) return default(T);
                T job = m_jobQueue.Dequeue();
                return job;
            }
        }

        /// <summary>
        /// Terminate all threads. The queue cannot be restarted.
        /// One can only peek at properties or enumerate outstanding jobs.
        /// </summary>
        public void Abort()
        {
            if (m_Thread.IsAlive)
            {
                m_Thread.Abort();
                m_Thread.Join(60000); //wait up to 1 min
                if (NotifyQueueIdle != null) NotifyQueueIdle(this.UserData);
            }
        }

        /// <summary>
        /// Get copy of all pending and active jobs (aka snapshot).
        /// Note: Cannot implement lazily as IEnumerable because the state may change.
        /// </summary>
        /// <returns>zero or more jobs. never null</returns>
        public List<T> GetPendingJobs()
        {
            lock (m_jobQueue)
            lock (m_runningJobList)
            {
                var jobs = new List<T>(PendingCount);
                jobs.AddRange(m_runningJobList.Where(m => m != null));
                jobs.AddRange(m_jobQueue);
                return jobs;
            }
        }

        #region IDisposable Members
        /// <summary>
        /// Dispose/cleanup all this class's resources.
        /// Nothing is left in this class to reference.
        /// Calling Dispose() again does nothing.
        /// </summary>
        public void Dispose()
        {
            if (!IsClosed(m_StopEvent)) m_StopEvent.Set();
            if (m_Thread.IsAlive) m_Thread.Join(60000);
            if (!IsClosed(m_StopEvent)) m_StopEvent.Close();
            m_jobQueue.Clear();
            m_runningJobList.Clear();
            if (!IsClosed(m_runningJobsSemaphore)) m_runningJobsSemaphore.Close();
            if (!IsClosed(m_TriggerEvent)) m_TriggerEvent.Close();
        }
        #endregion IDisposable Members

        /// <summary>
        /// Add item to first null value in list and returns its location index.
        /// List count can grow but it will never shrink.
        /// </summary>
        private static int AddIndexed<TT>(List<TT> list, TT value)
        {
            int index;
            for (index = 0; index < list.Count; index++)
            {
                if (IsNull(list[index])) { list[index] = value; return index; }
            }
            list.Add(value);
            return index;
        }

        /// <summary>
        /// Set value at index to default (e.g. null) value.
        /// List count can grow but it will never shrink.
        /// </summary>
        private static void RemoveIndexed<TT>(List<TT> list, int index)
        {
            if (index < 0 || index >= list.Count) return;
            list[index] = default(TT);
        }

        /// <summary>
        /// Test if value is equal to the default value for the given type.
        /// </summary>
        private static bool IsNull<TT>(TT value)
        {
            return EqualityComparer<TT>.Default.Equals(value, default(TT));
        }

        private static bool IsClosed(WaitHandle h)
        {
            return (h == null || h.SafeWaitHandle.IsClosed);
        }
    }
}
