#define TRACE //since we are using System.Diagnostics Trace logging, make sure it is enabled!
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ChuckHill2.Extensions;

// This namespace name is arbitrary. It can be changed to anything without adverse effects.
namespace ChuckHill2
{
    #region public Log interface
    /// <summary>
    /// Thin? wrapper for logging to System.Diagnostics logging interface.
    /// The purpose of this wrapper is to greatly simplify usage as well as not hinder 
    /// performance of the application.  Use of the built-in Trace or TraceSource API is 
    /// transparent, however the provided log messaging API are more efficient. Since this 
    /// is a wrapper over the .NET logging, it also captures trace messages from .NET 
    /// tracing internals such as WCF, WPF, System.Net, ASP.NET, etc.  
    /// System.Diagnostics.Trace API that do not support sources, output is redirected to 
    /// the built-in "TRACE" source when severity is set to "Verbose".  Console.WriteXXX 
    /// messages are copied to the built-in "CONSOLE" source when severity is set to 
    /// "Information".  First-chance exceptions normally only visible within the Visual 
    /// Studio debugger are copied to the built-in "FIRSTCHANCE" source when severity is 
    /// set to "Error".  All message events may also be captured and written to a .NET event 
    /// handler. Useful for copying event/messages to an application status window. Events 
    /// are lazily written to their output destination by default. The queued events are 
    /// flushed to their output destinations automatically upon application exit.  Logging 
    /// message/events is safe across thread, domain, and process boundries.  New trace 
    /// sources not defined in the app.config are automatically assigned the [Trace] 
    /// listener/output destinations.  New trace sources not defined in the app.config are 
    /// automatically assigned severity "Off" if not defined during trace source 
    /// creation.  If the app/web.config [system.diagnostics] section is modified during 
    /// runtime, the changes are detected and updated immediately. The application does not
    /// have to be restarted.  Computing certain logging parameters may be a severe performance 
    /// hit. As such, the provided log messaging API include the properties 'Is(severity)
    /// Enabled' to put the logging in an if() statement.  As an alternative, LinQ 
    /// delegates may be used instead to compute/generate the message string and requisite 
    /// parameters only upon demand.  Severity levels for a single source or all sources 
    /// may be changed programmatically.
    /// </summary>
    public class Log
    {
        #region public static methods
        /// <summary>
        /// Initialize static components of Log within the context of the main thread.
        /// Log will automatically dispose all its resources upon main thread exit.
        /// This should be called as one of the first lines in the startup thread.
        /// Failure to do so will not initialize the basic functionality until the first log instance is created.
        /// </summary>
        public static void LogInitialize()
        {
            //Nothing really to do. We just need to run within the context of the 
            //main thread so the thread watchdog will capture the main thread as 
            //Thread.CurrentThread.
        }

        /// <summary>
        /// Optionally flush and close all tracesources and associated listeners.
        /// Not really necessary as this routine will be called automatically upon main thread exit.
        /// This may take awhile to return if there are a lot of pending log messages queued.
        /// If additional logging is performed AFTER this method is called, logging is 
        /// automatically re-initialized.
        /// </summary>
        public static void Close()
        {
            AppDomainUnload(null, null);
        }

        /// <summary>
        /// Add/Remove a delegate to capture ALL log events enabled by severity for 
        /// custom handling. May be used to write events to a gui status window or other alternate destination.
        /// This will capture ALL event category/sources. It is the responsibility of the 
        /// delegate to handle the event category/sources as it sees fit. Since the output is 
        /// lazily written to the event handler, care must be taken 
        /// since we never know which thread we are running on. Use 
        /// Control.InvokeRequired flag and Control.BeginInvoke() as necessary.
        /// The event interface consists of: 
        ///    void Writer(FormatTraceEventCache);
        /// </summary>
        public static event Action<FormatTraceEventCache> RedirectorWriter
        {
            add
            {
                #region Append Redirector listener to all sources
                if (!TraceRedirectorListener.HasEventHandler)
                {
                    var redirectorListener = new TraceRedirectorListener();
                    if (Trace.Listeners[TraceRedirectorListener.DEFAULTNAME] == null) Trace.Listeners.Add(redirectorListener);
                    foreach (ConfigurationElement ce in ConfigurationSources)
                    {
                        //string sourcename = ce.ElementInformation.Properties["name"].Value as string;
                        //string severity = ce.ElementInformation.Properties["switchValue"].Value as string;
                        ConfigurationElementCollection listeners = ce.ElementInformation.Properties["listeners"].Value as ConfigurationElementCollection;
                        bool exists = false;
                        foreach (ConfigurationElement listener in listeners)
                        {
                            string lname = listener.ElementInformation.Properties["name"].Value as string;
                            //object ltype = listener.ElementInformation.Properties["type"].Value;
                            if (!lname.Equals(TraceRedirectorListener.DEFAULTNAME, StringComparison.OrdinalIgnoreCase)) continue;
                            exists = true;
                            break;
                        }
                        if (exists) continue;
                        Type tle = Type.GetType("System.Diagnostics.ListenerElement, " + typeof(Trace).Assembly.FullName, false, false);
                        ConfigurationElement newListener = Activator.CreateInstance(tle, new object[] { true }) as ConfigurationElement;
                        tle.GetProperty("Name").SetValue(newListener, TraceRedirectorListener.DEFAULTNAME, null);
                        tle.GetProperty("TypeName").SetValue(newListener, typeof(TraceRedirectorListener).AssemblyQualifiedName, null);
                        Type tlec = Type.GetType("System.Diagnostics.ListenerElementsCollection, " + typeof(Trace).Assembly.FullName, false, false);
                        var fi = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                        fi.SetValue(listeners, false); //hack: the in-memory config is read-only, so we temporarily disable the read-only property
                        tlec.GetMethod("BaseAdd", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(ConfigurationElement) }, null).Invoke(listeners, new object[] { newListener });
                        fi.SetValue(listeners, true);
                    }
                    lock (RawTraceSources)
                    {
                        foreach (WeakReference wr in RawTraceSources)
                        {
                            if (wr == null || wr.Target == null || !wr.IsAlive) continue;
                            var ts = (TraceSource)wr.Target;
                            if (ts.Listeners.Cast<TraceListener>().Any(m => m is TraceRedirectorListener)) continue;
                            ts.Listeners.Add(redirectorListener);
                        }
                    }
                }
                #endregion
                TraceRedirectorListener.RedirectorWriter += value;
            }
            remove
            {
                TraceRedirectorListener.RedirectorWriter -= value;
                #region Remove Redirector listener from all sources
                if (!TraceRedirectorListener.HasEventHandler)
                {
                    if (Trace.Listeners[TraceRedirectorListener.DEFAULTNAME] != null)
                    {
                        TraceRedirectorListener listener = Trace.Listeners[TraceRedirectorListener.DEFAULTNAME] as TraceRedirectorListener;
                        listener.Close();
                        Trace.Listeners.Remove(listener);
                    }
                    foreach (ConfigurationElement ce in ConfigurationSources)
                    {
                        //string sourcename = ce.ElementInformation.Properties["name"].Value as string;
                        //string severity = ce.ElementInformation.Properties["switchValue"].Value as string;
                        ConfigurationElementCollection listeners = ce.ElementInformation.Properties["listeners"].Value as ConfigurationElementCollection;
                        bool exists = false;
                        int index = -1;
                        foreach (ConfigurationElement listener in listeners)
                        {
                            index++;
                            string lname = listener.ElementInformation.Properties["name"].Value as string;
                            //object ltype = listener.ElementInformation.Properties["type"].Value;
                            if (!lname.Equals(TraceRedirectorListener.DEFAULTNAME, StringComparison.OrdinalIgnoreCase)) continue;
                            exists = true;
                            break;
                        }
                        if (!exists) continue;
                        Type tlec = Type.GetType("System.Diagnostics.ListenerElementsCollection, " + typeof(Trace).Assembly.FullName, false, false);
                        var fi = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
                        fi.SetValue(listeners, false); //hack: the in-memory config is read-only, so we temporarily disable the read-only property
                        tlec.GetMethod("BaseRemoveAt", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int) }, null).Invoke(listeners, new object[] { index });
                        fi.SetValue(listeners, true);
                    }

                    lock (RawTraceSources)
                    {
                        foreach (WeakReference wr in RawTraceSources)
                        {
                            if (wr == null || wr.Target == null || !wr.IsAlive) continue;
                            var ts = (TraceSource)wr.Target;
                            int index = ts.Listeners.Cast<TraceListener>().IndexOf(m => m is TraceRedirectorListener);
                            if (index == -1) continue;
                            ts.Listeners.RemoveAt(index);
                        }
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// Set the severity level for a specific source or switch
        /// </summary>
        /// <param name="sourceName">Name of source or switch.</param>
        /// <param name="severity">New severity level to set.</param>
        /// <returns>Previous severity level</returns>
        public static SourceLevels SetSeverity(string sourceName, SourceLevels severity)
        {
            var fi = typeof(System.Configuration.ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            SourceLevels prevSeverity = SourceLevels.Off;
            foreach (ConfigurationElement ce in ConfigurationSwitches)
            {
                string srcname = ce.ElementInformation.Properties["name"].Value as string;
                if (!srcname.Equals(sourceName, StringComparison.OrdinalIgnoreCase)) continue;
                var pi = ce.ElementInformation.Properties["value"];
                if (pi.Value == null) break;
                if (prevSeverity == SourceLevels.Off) Enum.TryParse<SourceLevels>(pi.Value as string, true, out prevSeverity);
                fi.SetValue(ce, false);
                pi.Value = severity.ToString();
                fi.SetValue(ce, true);
            }

            foreach (ConfigurationElement ce in ConfigurationSources)
            {
                string srcname = ce.ElementInformation.Properties["name"].Value as string;
                if (!srcname.Equals(sourceName, StringComparison.OrdinalIgnoreCase)) continue;
                var pi = ce.ElementInformation.Properties["switchValue"];
                if (pi.Value == null) break;
                if (prevSeverity == SourceLevels.Off) Enum.TryParse<SourceLevels>(pi.Value as string, true, out prevSeverity);
                fi.SetValue(ce, false);
                pi.Value = severity.ToString();
                fi.SetValue(ce, true);
            }

            lock (RawTraceSources)
            {
                foreach(WeakReference wr in RawTraceSources)
                {
                    if (wr.Target == null || !wr.IsAlive ||  !(((TraceSource)wr.Target).Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase) || ((TraceSource)wr.Target).Switch.DisplayName.Equals(sourceName, StringComparison.OrdinalIgnoreCase))) continue;
                    var ts = (TraceSource)wr.Target;
                    if (prevSeverity == SourceLevels.Off) prevSeverity = ts.Switch.Level;
                    ts.Switch.Level = severity;
                }
            }

            return prevSeverity;
        }

        /// <summary>
        /// Set ALL sources and switches to the specified severity level.
        /// Handy way to just disable all logging.
        /// </summary>
        /// <param name="severity">New severity level to set.</param>
        public static void SetAllSeverities(SourceLevels severity)
        {
            var fi = typeof(System.Configuration.ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (ConfigurationElement ce in ConfigurationSwitches)
            {
                var pi = ce.ElementInformation.Properties["value"];
                if (pi.Value == null) break;
                SourceLevels sev;
                if (!Enum.TryParse<SourceLevels>(pi.Value as string, true, out sev)) continue; //switch is not a severity
                fi.SetValue(ce, false);
                pi.Value = severity.ToString();
                fi.SetValue(ce, true);
            }

            foreach (ConfigurationElement ce in ConfigurationSources)
            {
                var pi = ce.ElementInformation.Properties["switchValue"];
                if (pi.Value == null) continue; //source severity is set by a switch value in <switches>
                fi.SetValue(ce, false);
                pi.Value = severity.ToString();
                fi.SetValue(ce, true);
            }

            lock (RawTraceSources)
            {
                foreach (WeakReference wr in RawTraceSources)
                {
                    if (wr == null || wr.Target == null || !wr.IsAlive) continue;
                    var ts = (TraceSource)wr.Target;
                    ts.Switch.Level = severity;
                }
            }
        }

        #region public static void Debug(string format, params object[] args)
        private static Action<string> debuglogger = DebugWriteInit();
        private static Action<string> DebugWriteInit()
        {
            if (Debugger.IsAttached && Debugger.IsLogging())
                return delegate(string msg) { System.Diagnostics.Debugger.Log(0, null, msg); };
            return OutputDebugString;
        }
        /// <summary>
        /// Handy SIMPLE developer debugging utility for writing a message to an external debug 
        /// viewer such as Microsoft's Dbgview.exe or VisualStudio debugger output window (not both).
        /// This method is NOT disabled in release mode, you must remove these calls from the code.
        /// </summary>
        /// <see cref="https://technet.microsoft.com/en-us/sysinternals/bb896647"/>
        /// <param name="format">string format specifier as used in string.Format()</param>
        /// <param name="args">format args</param>
        public static void Debug(string format, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(format)) return;
            if (format[format.Length - 1] != '\n') format += Environment.NewLine;
            debuglogger("Debug: " + SafeFormat(format, args));
        }
        #endregion
        #endregion

        #region public Instance Logging
        private TraceSource m_traceSource;
        private int m_traceSourceIndex;

        /// <summary>
        /// Get the name of this trace source.
        /// </summary>
        public string SourceName
        {
            get 
            {
                if (m_traceSource == null) return string.Empty;
                return m_traceSource.Name;
            }
        }

        /// <summary>
        /// Get the current severity level for this trace source.
        /// </summary>
        public SourceLevels SeverityLevel
        {
            get
            {
                if (m_traceSource == null) return SourceLevels.Off;
                return m_traceSource.Switch.Level;
            }
        }

        /// <summary>
        /// Comma-delimited list of listener names.
        /// </summary>
        public string ListenerNames
        {
            get
            {
                if (m_traceSource == null) return string.Empty;
                var sb = new StringBuilder();
                string comma = string.Empty;
                foreach(TraceListener tl in m_traceSource.Listeners)
                {
                    sb.Append(comma);
                    sb.Append(tl.Name);
                    comma = ", ";
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Constructor: Create log instance to write to the specified source.
        /// </summary>
        /// <param name="sourceName">Source category name. If the source does not exist in app.config, this new source uses the listener(s) as defined by the app.config Trace node.</param>
        /// <param name="severity">The value to use if category source is not defined in App.config</param>
        public Log(string sourceName, SourceLevels severity = SourceLevels.Off)
        {
            LogInit(); //in case logging has already been closed
            //If string source does not exist in ListenerCache, add it and use default Trace.Listeners.
            m_traceSource = GetTraceSource(sourceName, severity);
            m_traceSourceIndex = GetTraceSourceIndex(sourceName);
        }
        /// <summary>
        /// Constructor: Create log instance to write to the specified source.
        /// </summary>
        /// <param name="sourceName">Source category name. If the source does not exist in app.config, this new source uses the listener(s) as defined by the app.config Trace node.</param>
        /// <param name="severity">The value to use if category source is not defined in App.config. If this is an invalid name, the default is SourceLevels.Off</param>
        public Log(string sourceName, string severity) : this(sourceName, ToSeverity(severity)) { }

        #region IsSeverityEnabled?
        /// <summary>
        /// Determine if this severity level is allowed to be written.
        /// </summary>
        public bool IsCriticalEnabled => (((int)m_traceSource.Switch.Level & (int)TraceEventType.Critical) != 0);
        /// <summary>
        /// Determine if this severity level is allowed to be written.
        /// </summary>
        public bool IsErrorEnabled => (((int)m_traceSource.Switch.Level & (int)TraceEventType.Error) != 0);
        /// <summary>
        /// Determine if this severity level is allowed to be written.
        /// </summary>
        public bool IsWarningEnabled => (((int)m_traceSource.Switch.Level & (int)TraceEventType.Warning) != 0);
        /// <summary>
        /// Determine if this severity level is allowed to be written.
        /// </summary>
        public bool IsInformationEnabled => (((int)m_traceSource.Switch.Level & (int)TraceEventType.Information) != 0);
        /// <summary>
        /// Determine if this severity level is allowed to be written.
        /// </summary>
        public bool IsVerboseEnabled => (((int)m_traceSource.Switch.Level & (int)TraceEventType.Verbose) != 0);
        /// <summary>
        /// Determine if this severity level is allowed to be written.
        /// </summary>
        public bool IsActivityTracingEnabled => (((int)m_traceSource.Switch.Level & (int)SourceLevels.ActivityTracing) != 0);
        // Note: SourceLevels.ActivityTracing == 0xFF00 == TraceEventType.Start + Stop + Suspend + Resume + Transfer
        #endregion // Is serverity level enabled?

        #region Log write methods
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Critical(string format, params object[] args) => Write(null, TraceEventType.Critical, format, args);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Error(string format, params object[] args) => Write(null, TraceEventType.Error, format, args);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Warning(string format, params object[] args) => Write(null, TraceEventType.Warning, format, args);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Information(string format, params object[] args) => Write(null, TraceEventType.Information, format, args);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Verbose(string format, params object[] args) => Write(null, TraceEventType.Verbose, format, args);

        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Critical(Exception ex, string format, params object[] args) => Write(ex, TraceEventType.Critical, format, args);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Error(Exception ex, string format, params object[] args) => Write(ex, TraceEventType.Error, format, args);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Warning(Exception ex, string format, params object[] args) => Write(ex, TraceEventType.Warning, format, args);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Information(Exception ex, string format, params object[] args) => Write(ex, TraceEventType.Information, format, args);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Verbose(Exception ex, string format, params object[] args) => Write(ex, TraceEventType.Verbose, format, args);

        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Critical(Func<string> formatter) => Write(null, TraceEventType.Critical, formatter);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Error(Func<string> formatter) => Write(null, TraceEventType.Error, formatter);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Warning(Func<string> formatter) => Write(null, TraceEventType.Warning, formatter);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Information(Func<string> formatter) => Write(null, TraceEventType.Information, formatter);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Verbose(Func<string> formatter) => Write(null, TraceEventType.Verbose, formatter);

        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Critical(Exception ex, Func<string> formatter) => Write(ex, TraceEventType.Critical, formatter);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Error(Exception ex, Func<string> formatter) => Write(ex, TraceEventType.Error, formatter);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Warning(Exception ex, Func<string> formatter) => Write(ex, TraceEventType.Warning, formatter);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Information(Exception ex, Func<string> formatter) => Write(ex, TraceEventType.Information, formatter);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Verbose(Exception ex, Func<string> formatter) => Write(ex, TraceEventType.Verbose, formatter);

        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Write(TraceLevel severity, Func<string> formatter) => Write(null, ConvertToTraceEventType(severity), formatter);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Write(Exception ex, TraceLevel severity, Func<string> formatter) => Write(ex, ConvertToTraceEventType(severity), formatter);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Write(TraceEventType severity, Func<string> formatter) => Write(null, severity, formatter);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="formatter">A method that returns a formatted message string</param>
        public void Write(Exception ex, TraceEventType severity, Func<string> formatter)
        {
            if (m_traceSource == null) return;
            if (((int)m_traceSource.Switch.Level & (int)severity) == 0) return;
            try  //be safe. logging should not crash the app. ever!
            {
                string emsg;
                try { emsg = formatter(); }
                catch (Exception ex2) { emsg = "Internal Error: Log formatter delegate threw an exception"; ex = ex2; }
                EventCache = new FormatTraceEventCache(null, severity, m_traceSource.Name, (ushort)m_traceSourceIndex, emsg, ex);
                m_traceSource.TraceEvent(severity, m_traceSourceIndex, emsg);
                EventCache = null;
            }
            catch (Exception ex2) { Log.InternalError("log.Write({0},{1},delegate): {2}", ex == null ? "null" : "exception", severity, ex2.Message); }
        }

        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Write(TraceLevel severity, string format, params object[] args) => Write(null, ConvertToTraceEventType(severity), format, args);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Write(Exception ex, TraceLevel severity, string format, params object[] args) => Write(ex, ConvertToTraceEventType(severity), format, args);
        /// <summary>
        /// Log an event with the specified severity.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Write(TraceEventType severity, string format, params object[] args) => Write(null, severity, format, args);
        /// <summary>
        /// Log an event with the specified severity with an exception.
        /// The message string is not formatted until AFTER it is determined that the message will be written.
        /// </summary>
        /// <param name="ex">The exception to include in the logging message.</param>
        /// <param name="severity">The severity level to associate with this log event.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Write(Exception ex, TraceEventType severity, string format, params object[] args)
        {
            if (m_traceSource == null) return;
            if (((int)m_traceSource.Switch.Level & (int)severity) == 0) return;
            string emsg = "format";
            try  //be safe. logging should not crash the app. ever!
            {
                emsg = SafeFormat(format, args); //never throws an exception
                EventCache = new FormatTraceEventCache(null, severity, m_traceSource.Name, (ushort)m_traceSourceIndex, emsg, ex);
                m_traceSource.TraceEvent(severity, m_traceSourceIndex, emsg);
                EventCache = null;
            }
            catch (Exception ex2) { Log.InternalError("log.Write({0},{1},\"{2}\"): {3}", ex == null ? "null" : "exception", severity, emsg, ex2.Message); }
        }
        #endregion //Log write methods
        #endregion //public Instance Logging

        #region private helper functions
        //Maybe need to share the same locking object to avoid deadlocks.
        private static readonly object critSec = Type.GetType("System.Diagnostics.TraceInternal, " + typeof(Trace).Assembly.FullName, false, false).GetField("critSec", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        private static readonly ConfigurationElementCollection ConfigurationSources = Type.GetType("System.Diagnostics.DiagnosticsConfiguration, " + typeof(Trace).Assembly.FullName, false, false).GetProperty("Sources", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null) as ConfigurationElementCollection;
        private static readonly ConfigurationElementCollection ConfigurationSwitches = Type.GetType("System.Diagnostics.DiagnosticsConfiguration, " + typeof(Trace).Assembly.FullName, false, false).GetProperty("SwitchSettings", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null, null) as ConfigurationElementCollection;

        //The following is initialized only for the lifetime of a single event IN THIS THREAD. 
        //The purpose is to efficiently support multiple listeners for a given trace source.
        //Initialized/disposed in Log.Write() or initialized and disposed lazily within TraceListenerBase.
        [ThreadStatic] internal static FormatTraceEventCache EventCache = null;

        private TraceEventType ConvertToTraceEventType(TraceLevel severity)
        {
            switch (severity)
            {
                case TraceLevel.Off: return 0;
                case TraceLevel.Error: return TraceEventType.Error;
                case TraceLevel.Warning: return TraceEventType.Warning;
                case TraceLevel.Info: return TraceEventType.Information;
                case TraceLevel.Verbose: return TraceEventType.Verbose;
                default: return 0;
            }
        }

        private SourceLevels ConvertToSourceLevels(TraceLevel severity)
        {
            switch (severity)
            {
                case TraceLevel.Off: return SourceLevels.Off;
                case TraceLevel.Error: return SourceLevels.Error;
                case TraceLevel.Warning: return SourceLevels.Warning;
                case TraceLevel.Info: return SourceLevels.Information;
                case TraceLevel.Verbose: return SourceLevels.Verbose;
                default: return SourceLevels.Off;
            }
        }

        internal static string SafeFormat(string format, params object[] args)
        {
            //This is needed if the caller screws up the format string.
            try
            {
                //nothing to do!
                if (string.IsNullOrWhiteSpace(format)) return format;
                //if string contains "{0}", but arg list is empty, just return the string.
                if (args == null || args.Length == 0) return format;
                //If the format arg is null, say so!
                //for (int i = 0; i < args.Length; i++) { if (args[i] == null) args[i] = "null"; } -- Performance Hit?
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("(string format error) \"{0}\"", format);
                for (int i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat(", \"{0}\"", args[i].ToString());
                }
                return sb.ToString();
            }
        }

        private static SourceLevels ToSeverity(string severity)
        {
            SourceLevels severityLevel;
            if (Enum.TryParse<SourceLevels>(severity, true, out severityLevel)) return severityLevel;
            Log.InternalError("\"{0}\" is not a valid severity. Valid Severity levels are: {1}", severity, string.Join(", ", Enum.GetNames(typeof(SourceLevels))));
            return SourceLevels.Off; 
        }

        private static readonly List<WeakReference> RawTraceSources = ((List<WeakReference>)typeof(TraceSource).GetField("tracesources", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null));
        internal static TraceSource GetTraceSource(string sourceName, SourceLevels defaultSeverity = SourceLevels.Off)
        {
            lock (RawTraceSources)
            {
                //Efficiency: just re-use tracesource, if it exists.
                WeakReference wr = RawTraceSources.FindLast(x => (x.Target != null && x.IsAlive && ((TraceSource)x.Target).Name.Equals(sourceName, StringComparison.InvariantCultureIgnoreCase)));
                if (wr != null) return (TraceSource)wr.Target;
            }
            TraceSource ts =  new TraceSource(sourceName, defaultSeverity); //Tracesource does not exist, so create a new one.
            try
            {
                //Add default listeners to new trace source not listed in app.config.
                if (ts.Listeners.Count == 1 && ts.Listeners[0].Name == "Default") //must be a new source not in the app.config
                {
                    TraceListenerCollection tlc;
                    ts.Listeners.Clear();
                    try
                    {
                        //This will throw an exception if the trace listeners in the appconfig do not exist or improperly configured!
                        //If it does throw an exception, we fix it by clearing the bad listeners and adding our own DebugTraceListener()
                        tlc = Trace.Listeners; 
                    }
                    catch (Exception ex)
                    {
                        //handle missing or corrupted listener assembly type names
                        Log.InternalError("Error initializing Default Trace listeners: {0}", ex.Message);
                        var listeners = (TraceListenerCollection)typeof(TraceListenerCollection).GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null).Invoke(new object[] { });
                        listeners.Add(new DebugTraceListener());
                        Type t = Type.GetType(typeof(TraceListenerCollection).AssemblyQualifiedName.Replace("TraceListenerCollection", "TraceInternal"));
                        FieldInfo fi = t.GetField("listeners", BindingFlags.Static | BindingFlags.NonPublic);
                        fi.SetValue(null, listeners);
                        tlc = Trace.Listeners;
                    }
                    ts.Listeners.AddRange(tlc);
                }
            }
            catch (Exception ex)
            {
                //Handle improperly configured or missing listeners for the given trace source. Defaults to System.Diagnostics.DefaultTraceListener
                Log.InternalError("Error initializing {0} listeners: {1}", ts.Name, ex.Message);
                typeof(TraceSource).GetField("_initCalled", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ts, true);
                typeof(TraceSource).GetMethod("NoConfigInit",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ts,new object[]{});
            }
            return ts;
        }
        internal static int GetTraceSourceIndex(string sourceName)
        {
            lock (KnownSources)
            {
                for (int i = 0; i < KnownSources.Count; i++)
                    if (KnownSources[i].Equals(sourceName, StringComparison.InvariantCultureIgnoreCase)) return i;
                KnownSources.Add(sourceName);
                return KnownSources.Count - 1;
            }
        }

        private static List<string> KnownSources = ConfigurationSources.Cast<ConfigurationElement>().Select(m => m.ElementInformation.Properties["name"].Value as string).ToList();
        private static ConWriter m_ConWriter;
        private static FileSystemWatcher m_AppConfigWatcher;
        private static Guid m_AppConfigChangeKey;
        private static Thread m_watchDogThread=null;

        static Log()
        {
            LogInit();
        }

        internal static void LogInit()
        {
            try
            {
                if (m_watchDogThread != null) return;
                m_watchDogThread = AtThreadExit(AppDomainUnload);

                //good AppDomain explanation: http://www.codeproject.com/Articles/16164/Managed-Application-Shutdown
                AppDomain dom = AppDomain.CurrentDomain;
                if (dom.IsDefaultAppDomain()) dom.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) { AppDomainUnload(sender, (EventArgs)e); };
                else dom.DomainUnload += AppDomainUnload;
                dom.ProcessExit += AppDomainUnload;

                //Pre-execute the built-ins just in case they are not in the app.config so their source index is in the correct order.
                //Also, if CONSOLE or FIRSTCHANCE are enabled, be sure the handlers are installed.
                var trace = new Log("TRACE", SourceLevels.Off);
                if (new Log("CONSOLE", SourceLevels.Off).IsInformationEnabled) m_ConWriter = new ConWriter();  //Enable Log("CONSOLE")
                if (new Log("FIRSTCHANCE", SourceLevels.Off).IsErrorEnabled) AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;  //Enable Log("FIRSTCHANCE")
                KnownSources.MoveToIndex("TRACE", 0); //TRACE must always be zero in order to detect listener TraceEvent() id==0.
                KnownSources.MoveToIndex("CONSOLE", 1);
                KnownSources.MoveToIndex("FIRSTCHANCE", 2);

                m_AppConfigWatcher = AppConfigWatcher();
            }
            catch (Exception ex)
            {
                Log.InternalError("LogInit(): {0}", ex.Message);
            }
        }

        // *** Detecting Application Exit ***
        // Execution context consists of (1)Process, (2)AppDomains, and (3)Threads.
        // Because of the global (e.g. static) nature of logging, when the appdomain shuts down,
        // so must the logging. This logging needs to be closed gracefully because queued 
        // asynchronous writes may not all yet be written to disk. Well, if the writer threads 
        // are background threads and when the AppDomain.DomainUnload event fires, the writer 
        // threads are already dead. All pending writes are lost.  If the writer threads are NOT
        // background threads, AppDomain.DomainUnload event will never fire because .NET will 
        // not automatically terminate the writer threads. In addition, the process will never 
        // exit because all the appdomains have not yet been unloaded. Catch-22. How to detect 
        // when the appdomain is really done? Well, the only other way is to detect when the 
        // appdomain startup thread exits. This is not the ideal solution because there may be 
        // other foreground threads running, but it is the only one. At least continued use of 
        // the logging will not cause any problems, In fact, logging will automatically restart!
        // good explanation: http://www.codeproject.com/Articles/16164/Managed-Application-Shutdown

        private static void AppDomainUnload(object sender, EventArgs ev)
        {
            try
            {
                if (sender is Thread)
                {
                    AppDomain dom = AppDomain.CurrentDomain;
                    if (dom.IsDefaultAppDomain()) dom.UnhandledException -= delegate(object sender2, UnhandledExceptionEventArgs e) { AppDomainUnload(sender2, (EventArgs)e); };
                    else dom.DomainUnload -= AppDomainUnload;
                    dom.ProcessExit -= AppDomainUnload;
                }
                else if (sender is AppDomain)
                {
                    m_watchDogThread.Abort();
                }
                else //User called Close()
                {
                    AppDomain dom = AppDomain.CurrentDomain;
                    if (dom.IsDefaultAppDomain()) dom.UnhandledException -= delegate(object sender2, UnhandledExceptionEventArgs e) { AppDomainUnload(sender2, (EventArgs)e); };
                    else dom.DomainUnload -= AppDomainUnload;
                    dom.ProcessExit -= AppDomainUnload;
                    m_watchDogThread.Abort();
                }

                m_watchDogThread = null;
                if (m_AppConfigWatcher != null) { m_AppConfigWatcher.Dispose(); m_AppConfigWatcher = null; }
                if (m_ConWriter != null) { m_ConWriter.Dispose(); m_ConWriter = null; }
                AppDomain.CurrentDomain.FirstChanceException -= FirstChanceExceptionHandler;
                lock (RawTraceSources)
                {
                    foreach (TraceListener t in Trace.Listeners) t.Dispose();
                    foreach (WeakReference wr in RawTraceSources.ToArray())
                    {
                        if (wr.Target == null || !wr.IsAlive) continue;
                        TraceSource ts = wr.Target as TraceSource;
                        ts.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.InternalError("AppDomainUnload({0},EventArgs): {1}", sender==null?"null":sender.GetType().Name, ex.Message);
            }
        }

        private static Thread AtThreadExit(Action<object, EventArgs> handler)
        {
            Thread th = new Thread(delegate(object obj)
            {
                Thread mainTh = (Thread)obj;
                //mainTh.Join();  //This never returns!!!
                while ((mainTh.ThreadState & System.Threading.ThreadState.Stopped) != System.Threading.ThreadState.Stopped)
                {
                    Thread.Sleep(1000);
                }
                handler(mainTh, EventArgs.Empty);
            });
            th.Name = "Main Thread Watchdog";
            th.Priority = ThreadPriority.Lowest;
            th.IsBackground = false;
            th.Start(Thread.CurrentThread);
            return th;
        }

        #region internal static void InternalError(string format, params object[] args)
        #region Raw Win32 Console Write
        [DllImport("Kernel32.dll")] private static extern void OutputDebugString(string errmsg);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)] private static extern bool WriteFile(IntPtr hFile, string lpBuffer, int nNumberOfBytesToWrite, out int lpNumberOfBytesWritten, IntPtr Overlapped);
        [DllImport("Kernel32.dll", SetLastError = true)] private static extern bool CloseHandle(IntPtr hFile);
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)] private static extern IntPtr CreateFile(string name, int DesiredAccess, int ShareMode, IntPtr SecurityAttributes, int CreationDisposition, int FlagsAndAttributes, IntPtr hTemplateFile);
        /// <summary>
        /// Write raw message to console window. Needed when Console.Write has been redirected.
        /// We want to avoid using ANYTHING within this logging utility
        /// </summary>
        /// <param name="s"></param>
        private static bool WriteRawConsole(string s)
        {
            IntPtr INVALID_HANDLE = new IntPtr(-1);
            int OPEN_EXISTING = 3;
            int GENERIC_WRITE = 0x40000000;
            int FILE_SHARE_WRITE = 0x00000002;
            int nbytes;
            bool ok;

            IntPtr hFile = CreateFile("CON", GENERIC_WRITE, FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (hFile == IntPtr.Zero || hFile == INVALID_HANDLE) return false;
            ok = WriteFile(hFile, s, s.Length, out nbytes, IntPtr.Zero);
            CloseHandle(hFile);
            return ok;
        }
        #endregion

        /// <summary>
        /// Forceably write error to: 
        /// (1) application event log (if the event log source exists), 
        /// (2) console window (if it exists), and 
        /// (3) DbgView or VisualStudio output window (if it is listening), 
        /// that occured within this logger and TraceListeners!
        /// Any and all exceptions within this method are swallowed.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        internal static void InternalError(string format, params object[] args)
        {
            string emsg = string.Format(CultureInfo.InvariantCulture, format, args);
            string emsg2 = string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm:ss.fff} : {1} : {2} : {3}", DateTime.Now, TraceEventType.Error, "LoggerInternal", emsg);
            try { EventLog.WriteEntry("Analytics", "Category: LoggerInternal\n" + emsg, EventLogEntryType.Error, (int)EventLogEntryType.Error, 0); } catch { }
            try { if (!WriteRawConsole(emsg2)) Console.WriteLine(emsg2); } catch { }
            try 
            {
                if (Debugger.IsAttached && Debugger.IsLogging())
                    System.Diagnostics.Debugger.Log((int)TraceEventType.Error, "LoggerInternal", emsg2);
                else OutputDebugString(emsg2); 
            } catch{}
        }
        #endregion

        /// <summary>
        /// Reload system.diagnostics trace logging info from the app.config if the app.config file has changed.
        /// </summary>
        /// <returns></returns>
        private static FileSystemWatcher AppConfigWatcher()
        {
            FileSystemWatcher fsw = null;
            try
            {
                string appConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                //string appConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
                if (!File.Exists(appConfig)) return null;

                m_AppConfigChangeKey = GetAppConfigKey(appConfig);

                fsw = new FileSystemWatcher(Path.GetDirectoryName(appConfig), Path.GetFileName(appConfig));
                fsw.NotifyFilter = NotifyFilters.LastWrite;
                //fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                fsw.Changed += delegate(object sender, FileSystemEventArgs e)
                {
                    fsw.EnableRaisingEvents = false;
                    if (e.ChangeType != WatcherChangeTypes.Changed)
                    {
                        fsw.EnableRaisingEvents = true;
                        return;
                    }
                    Guid key = GetAppConfigKey(appConfig);
                    if (m_AppConfigChangeKey != key)
                    {
                        m_AppConfigChangeKey = key;
                        Trace.Refresh();  //App.config has changed. Refresh the logging info.

                        if (new Log("CONSOLE").IsInformationEnabled)
                        {
                            if (m_ConWriter == null) m_ConWriter = new ConWriter();  //Enable Log("CONSOLE")
                        }
                        else if (m_ConWriter != null) m_ConWriter.Dispose();

                        AppDomain.CurrentDomain.FirstChanceException -= FirstChanceExceptionHandler;
                        if (new Log("FIRSTCHANCE").IsErrorEnabled)
                            AppDomain.CurrentDomain.FirstChanceException += FirstChanceExceptionHandler;  //Enable Log("FirstChance")
                    }
                    fsw.EnableRaisingEvents = true;
                };
                fsw.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Log.InternalError("AppConfigWatcher(): {0}", ex.Message);
            }
            return fsw;
        }

        /// <summary>
        /// Create unique hash key for the app.config system.diagnostics xml element.
        /// The purpose is to be able to detect changes to *only* the config section 
        /// we are interested in.
        /// </summary>
        /// <param name="appConfig"></param>
        /// <returns></returns>
        private static Guid GetAppConfigKey(string appConfig)
        {
            try
            {
                if (!File.Exists(appConfig)) return Guid.Empty;
                string xml = File.ReadAllText(appConfig);
                if (string.IsNullOrWhiteSpace(xml)) return Guid.Empty;
                int startIndex = xml.IndexOf("<system.diagnostics>");
                if (startIndex == -1) return Guid.Empty;
                int endIndex = xml.IndexOf("</system.diagnostics>", startIndex);
                if (endIndex == -1) return Guid.Empty;
                string s = xml.Substring(startIndex, endIndex - startIndex + 22);

                StringBuilder sb = new StringBuilder(s.Length);
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c > 0 && c < 32) c = ' ';
                    if (c == ' ') continue;
                    sb.Append(char.ToLowerInvariant(c));
                }

                //MD5 is not FIPS compliant, but this is not used for security, fits very nicely in a guid, and is FAST.
                using (var provider = MD5.Create())
                {
                    bool compliant = FIPSCompliant;
                    if (compliant) FIPSCompliant = false;
                    Guid key = new Guid(provider.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
                    if (compliant) FIPSCompliant = true;
                    return key;
                }
            }
            catch (Exception ex)
            {
                Log.InternalError("GetAppConfigKey(\"{0}\"): {1}", appConfig, ex.Message);
            }
            return Guid.Empty;
        }

        /// <summary>
        /// Get or set FIPS compliance flag.
        /// A hacky way to allow non-FIPS compliant algorthms to run no matter what.
        /// Non-FIPS compliant algorthims are:
        ///     MD5CryptoServiceProvider,
        ///     RC2CryptoServiceProvider,
        ///     RijndaelManaged,
        ///     RIPEMD160Managed,
        ///     SHA1Managed,
        ///     SHA256Managed,
        ///     SHA384Managed,
        ///     SHA512Managed,
        ///     AesManaged,
        ///     MD5Cng. 
        /// In particular, enables use of fast MD5 hash to create unique identifiers for internal use.
        /// </summary>
        private static bool FIPSCompliant
        {
            get { return CryptoConfig.AllowOnlyFipsAlgorithms; }
            set
            {
                FieldInfo fi;
                fi = typeof(CryptoConfig).GetField("s_fipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, value);
                fi = typeof(CryptoConfig).GetField("s_haveFipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, true);
            }
        }
        #endregion

        #region ConWriter - Clone Console.WriteXXXX() messages to Log("CONSOLE")
        /// <summary>
        /// Capture Console.WriteXXXX() overloaded methods and copy to TraceSource("CONSOLE").
        /// Note: Console.WriteLine() will ALWAYS has a serverity of "Information"
        /// </summary>
        private class ConWriter : TextWriter
        {
            private TextWriter oldOut;
            Log log;

            internal ConWriter() : base()
            {
                log = new Log("CONSOLE");
                oldOut = System.Console.Out;
                System.Console.SetOut(this);
            }

            public override Encoding Encoding { get { return oldOut.Encoding; } }

            public override void Close()
            {
                if (oldOut == null) return;
                System.Console.SetOut(oldOut);
                oldOut = null;
            }
            protected override void Dispose(bool disposing)
            {
                this.Close();
                base.Dispose(disposing);
            }

            #region public void Write(value) [overloaded]
            public override void Write(bool value) { WriteInternal(value.ToString()); }
            public override void Write(char value) { WriteInternal(value.ToString()); }
            public override void Write(char[] buffer) { WriteInternal(new string(buffer)); }
            public override void Write(char[] buffer, int index, int count) { WriteInternal(new string(buffer, index, count)); }
            public override void Write(decimal value) { WriteInternal(value.ToString()); }
            public override void Write(double value) { WriteInternal(value.ToString()); }
            public override void Write(float value) { WriteInternal(value.ToString()); }
            public override void Write(int value) { WriteInternal(value.ToString()); }
            public override void Write(long value) { WriteInternal(value.ToString()); }
            public override void Write(object value) { WriteInternal(value.ToString()); }
            public override void Write(string format, object arg0) { WriteInternal(format, arg0); }
            public override void Write(string format, object arg0, object arg1) { WriteInternal(format, arg0, arg1); }
            public override void Write(string format, object arg0, object arg1, object arg2) { WriteInternal(format, arg0, arg1, arg2); }
            public override void Write(string format, params object[] arg) { WriteInternal(format, arg); }
            public override void Write(string value) { WriteInternal(value); }
            public override void Write(uint value) { WriteInternal(value.ToString()); }
            public override void Write(ulong value) { WriteInternal(value.ToString()); }
            public override void WriteLine() { WriteInternal(this.NewLine); }
            public override void WriteLine(bool value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(char value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(char[] buffer) { WriteInternal(new string(buffer) + this.NewLine); }
            public override void WriteLine(char[] buffer, int index, int count) { WriteInternal(new string(buffer, index, count) + this.NewLine); }
            public override void WriteLine(decimal value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(double value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(float value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(int value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(long value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(object value) { WriteInternal(value.ToString() + this.NewLine); }
            public override void WriteLine(string format, object arg0) { WriteInternal(format + this.NewLine, arg0); }
            public override void WriteLine(string format, object arg0, object arg1) { WriteInternal(format + this.NewLine, arg0, arg1); }
            public override void WriteLine(string format, object arg0, object arg1, object arg2) { WriteInternal(format + this.NewLine, arg0, arg1, arg2); }
            public override void WriteLine(string format, params object[] arg) { WriteInternal(format + this.NewLine, arg); }
            public override void WriteLine(string value) { WriteInternal(value.ToString(CultureInfo.InvariantCulture) + this.NewLine); }
            public override void WriteLine(uint value) { WriteInternal(value.ToString(CultureInfo.InvariantCulture) + this.NewLine); }
            public override void WriteLine(ulong value) { WriteInternal(value.ToString(CultureInfo.InvariantCulture) + this.NewLine); }
            #endregion

            private void WriteInternal(string format, params object[] args)
            {
                string message = Log.SafeFormat(format, args);
                if (oldOut!=null)
                {
                    try 
                    { 
                        oldOut.Write(message); 
                    }
                    catch 
                    {
                        System.Console.SetOut(oldOut);
                        oldOut = null;
                    }
                }
                try
                {
                    if (string.IsNullOrWhiteSpace(message)) return; //don't log empty strings.
                    log.Information(message);
                }
                catch { }
            }
        }
        #endregion

        #region FirstChanceExceptionHandler() - Log("FIRSTCHANCE")
        private static string _previousMsg = string.Empty; //used to avoid duplicates during try/catch/throw stack unwinding.
        private static Log _firstChanceLog = null;
        private static readonly PropertyInfo _FullName = typeof(MethodBase).GetProperty("FullName", BindingFlags.Instance | BindingFlags.NonPublic); //This is a protected property. Go figure...
        private static void FirstChanceExceptionHandler(object source, FirstChanceExceptionEventArgs e)
        {
            try
            {
                if (e.Exception is System.Configuration.ConfigurationErrorsException) return; //avoid recursion errors
                if (_firstChanceLog == null)
                {
                    _firstChanceLog = new Log("FIRSTCHANCE");
                }

                //Only report when "FIRSTCHANCE" severity >= Error
                if (!_firstChanceLog.IsErrorEnabled) return;

                //Ignore the following exceptions. They happen way too often.

                Exception ex = e.Exception; //for brevity
                if (ex is System.TimeoutException) return;
                if (ex is System.IO.IOException) return;
                string tname = ex.GetType().FullName;
                string[] exclusions = { "System.ServiceModel.", "System.Net.", "System.Net.Sockets.", "System.Deployment.Application." };
                foreach (string exclusion in exclusions)
                {
                    if (tname.StartsWith(exclusion) && tname.IndexOf('.', exclusion.Length) == -1) return;
                }

                //Find out which of our product assemblies the error occured in.

                var thisAssemblyName = (new AssemblyName(Assembly.GetExecutingAssembly().FullName)).Name;
                string assemblyName = null;
                string methodName = null;
                int lineNumber = 0;
                string fileName = null;
                //The following is time-consuming, but we can't do it in another thread.
                var stackFrames = new StackTrace(true).GetFrames();
                if (stackFrames != null) foreach (var frame in stackFrames)
                    {
                        var method = frame.GetMethod();
                        string name = method.DeclaringType.Assembly.FullName;
                        if (name.StartsWith(thisAssemblyName, StringComparison.InvariantCultureIgnoreCase)) continue;
                        assemblyName = (new AssemblyName(name)).Name;
                        methodName = _FullName.GetValue(method, null) as string;
                        lineNumber = frame.GetFileLineNumber();
                        fileName = frame.GetFileName();  //valid only when the PDB's exist
                        break;
                    }

                //If it didn't occur anywhere within our product assemblies, 
                //don't display it. We can't do anything about it anyway.
                if (assemblyName == null) return;

                //Every parent caller that catches and rethrows the exception will execute 
                //this handler. We check here to see if it has already been reported. This 
                //is necessary so we won't see duplicates in the log.
                string curr = string.Concat(methodName, lineNumber, fileName, tname, ex.Message);
                if (_previousMsg.Equals(curr)) return;
                _previousMsg = curr;

                string emsg = "First-Chance exception raised in:\r\n    Assembly: {0}\r\n    Method: {1}";
                if (fileName != null) emsg += "\r\n    File: {2}({3})";  //PDB's apparently exist in order to get this.
                _firstChanceLog.Error(ex, emsg, assemblyName, methodName, fileName, lineNumber);
            }
            catch (Exception ex)
            {
                Log.InternalError("FirstChanceExceptionHandler(): {0}", ex.Message);
            }
        }
        #endregion
    }
    #endregion

    #region Custom Trace Listeners
    //Built-in .NET TraceListeners:
    //  System.Diagnostics.TextWriterTraceListener
    //  System.Diagnostics.ConsoleTraceListener
    //  System.Diagnostics.DefaultTraceListener
    //  System.Diagnostics.DelimitedListTraceListener
    //  System.Diagnostics.XmlWriterTraceListener
    //  System.Diagnostics.EventLogTraceListener
    //  System.Diagnostics.EventSchemaTraceListener
    //  System.Diagnostics.Eventing.EventProviderTraceListener
    //  System.Web.IisTraceListener
    //  System.Web.WebPageTraceListener
    //The above trace listeners are NOT asynchronous. The file-based listeners do not handle writes across multiple processes/threads. They are NOT thread-safe.
    //Trace and TraceSource are independent and do not refer to the same output destinations and severity levels.

    #region public interface IWriterBase<MSG>
    /// <summary>
    /// TraceListenerBase log message writer interface
    /// </summary>
    /// <typeparam name="MSG">type of message to write</typeparam>
    public interface IWriterBase<MSG>
    {
        string Name { get; }
        void Write(MSG msg);
        void Close();
        bool IsClosed { get; }
    }
    #endregion

    #region public abstract class QWriterBase<MSG> : IWriterBase<MSG>, IDisposable
    /// <summary>
    /// Base class for writing asynchronous OR synchronous log messages. For asynchronous 
    /// logging, one must call Close/Dispose upon application exit or application will NEVER 
    /// exit as messages are lazily written by a low-priority foreground thread. The 
    /// foreground thread is necessary to be able to allow time to flush all queued messages
    /// before the close operation completes. Upon application exit, a background thread 
    /// will simply terminate and outstanding queued messages will be lost.
    /// </summary>
    /// <typeparam name="MSG">type of message to write</typeparam>
    public abstract class QWriterBase<MSG> : IWriterBase<MSG>, IDisposable
    {
        private bool m_async = false;
        protected string m_name = string.Empty; //name of parent listener. Needed to assign a nice name to the thread.
        private Queue<MSG> m_queue;
        private AutoResetEvent m_writeEvent;
        private ManualResetEvent m_exitEvent; //triggered only once.
        private Thread m_thread;
        private bool m_isClosed = true;

        public string Name { get { return m_name; } }

        public QWriterBase(string listenerName, bool async, params object[] args)
        {
            m_name = listenerName;
            m_async = async;

            if (m_async)
            {
                try
                {
                    LoggerInit(args);
                    m_queue = new Queue<MSG>();
                    m_writeEvent = new AutoResetEvent(false);
                    m_exitEvent = new ManualResetEvent(false);
                    m_thread = new Thread(LogWriterThread);
                    m_thread.Priority = ThreadPriority.BelowNormal;
                    m_thread.IsBackground = false;
                    m_thread.Name = m_name + " Listener Queue";
                    m_thread.Start();
                }
                catch
                {
                    Close();
                    throw;
                }
            }
            m_isClosed = false;
        }
        
        private void LogWriterThread()
        {
            try
            {
                WaitHandle[] handles = new WaitHandle[] { m_exitEvent, m_writeEvent };
                int index = -1;
                while (true)
                {
                    index = WaitHandle.WaitAny(handles);
                    if (index==0) break;
                    MsgWriterLoop(); //write all msgs in the queue
                }
            }
            catch (Exception ex)
            {
                Log.InternalError("Failure Writing {0} TraceEvent message. Terminating further writes.\nException={1}", this.Name, ex);
            }
            finally
            {
                if (m_exitEvent != null) { m_exitEvent.Close(); m_exitEvent = null; }
                if (m_writeEvent != null) { m_writeEvent.Close(); m_writeEvent = null; }
            }
        }

        private void MsgWriterLoop()
        {
            if (m_queue == null) return;
            lock (m_queue) { if (m_queue.Count == 0) return; }
            try
            {
                Lock();
                while (true)
                {
                    MSG msg;
                    lock (m_queue)
                    {
                        if (m_queue.Count == 0) break;
                        msg = m_queue.Dequeue();
                    }
                    try { LoggerWrite(msg); }
                    catch (Exception ex) { Log.InternalError("Exception writing {0} log message\r\n{1}", this.Name, ex.ToString()); }
                }
            }
            catch { }
            finally { Unlock(); }
        }

        public void Write(MSG msg)
        {
            if (m_async)
            {
                if (m_thread == null || m_queue == null) return;
                lock (m_queue) m_queue.Enqueue(msg);
                if (m_writeEvent != null) m_writeEvent.Set();
            }
            else
            {
                try { LoggerWrite(msg); }
                catch (Exception ex) { Log.InternalError("Exception writing {0} log message\r\n{1}", this.Name, ex.ToString()); }
            }
        }

        public void Close()
        {
            m_isClosed = true;
            if (m_async)
            {
                if (m_thread != null)
                {
                    m_thread.Priority = ThreadPriority.Highest;
                    if (m_exitEvent != null) m_exitEvent.Set();
                    if (m_thread.IsAlive) m_thread.Join(10 * 60 * 1000); //wait 10 minutes to finish writing messages.
                    if (m_thread.IsAlive) m_thread.Abort();
                    m_thread = null;
                }
                if (m_queue != null)
                {
                    m_queue.Clear();
                    m_queue = null;
                }
            }
            LoggerClose();
        }
        public void Dispose() { Close(); }
        public bool IsClosed { get { return m_isClosed; } }

        protected virtual void Lock() { } 
        protected virtual void Unlock() { }
        protected abstract void LoggerInit(object[] args);
        protected abstract void LoggerWrite(MSG msg);
        protected abstract void LoggerClose();
    }
    #endregion

    #region public abstract class TraceListenerBase<MSG> : TraceListener
    /// <summary>
    /// Our base trace listener class that handles 99% of the work.
    /// Listeners derived from this class: 
    /// (1) Optionally handle custom message formatting.
    /// (2) Handle redirection of Trace/Debug messages that do not contain a traceSource (some do!) to the "TRACE" traceSource.
    /// (3) Safely cleanup upon close/dispose.
    /// (4) Efficiently re-use pre-existing trace sources. System.Diagnostics by default does not!
    /// (5) Thread-safe.
    /// (6) Initialized only upon first log message/event.
    /// The app.config listener attribute 'initializeData', contains a stringized, case-insensitive, dictionary of name/value (delimited by '=') pairs delimited by ';'
    /// These dictionary keys and default values are listed below:
    ///    SqueezeMsg=false - squeeze multi-line FullMessage and duplicate whitespace into a single line.
    ///    Async=true - lazily write messages to output destination. False may incur performance penalties as messages are written immediately.
    ///    IndentSize=Trace.IndentSize (typically 4) - How many spaces to indent succeeding lines in a multi-line FullMessage.
    ///    Format=(default determined by derived class) - same as string.Format format specifier with arguments. See string.Format().
    ///    Possible 'Format' argument values are (case-insensitive): 
    ///       string UserMessage - message provided by user to logging api. If null or empty, ExceptionMessage is returned.
    ///       string Exception - full exception provided by user to logging api. If exception not available, CallStack is returned.
    ///       string ExceptionOrCallStack - full exception provided by user to logging api. If exception not available, CallStack is returned.
    ///       string ExceptionMessage	- The message part of exception or "" if exception not provided.
    ///       string CallStack - call stack for this logging api (excluding the internal logging calls). May incur a logging performance penality.
    ///       TraceEventType Severity - the severity assigned to this logging event as a native enum.
    ///       string SeverityString - the severity assigned to this logging event converted to string.
    ///       ushort SourceId	- the source index as defined by the order of sources in app.config
    ///       string SourceName - the name of the source for this logging event.
    ///       string DomainName - friendly name of the AppDomain that these logging api are running under or "" if AppDomain.FriendlyName not set.
    ///       string EntryAssemblyName - namepart of the assembly that started this AppDomain
    ///       Guid ActivityId - Unique id in order to group events across AppDomains/Processes.
    ///       string LogicalOperationStack - comma-delimited list of logical operations.
    ///       DateTime LocalDateTime - local time when this logging API was called.
    ///       DateTime DateTime - UTC time when this logging API was called. 
    ///       int ProcessId - process ID for this instance of the application.
    ///       string ProcessName - process name for this application.
    ///       int ThreadId - managed thread id for the current thread.
    ///       string ThreadName - thread name for the current thread.
    ///       long Timestamp - high-resolution time that this logging api was called.
    ///       string UserData - user-defined object. Must have had overridden ToString() to get more than the class name.
    ///   App.Config 'Format' example:
    ///   "FORMAT=&quot;{0:yyyy-MM-ddTHH:mm:ss.fff} Severity: {1}, Source: {2}\r\nMessage: {3}\r\nException: {4}&quot;,DateTime,Severity,SourceName,UserMessage,ExceptionMessage"
    ///   or with implicit newlines....
    ///   "FORMAT=&quot;
    ///   {0:yyyy-MM-ddTHH:mm:ss.fff} Severity: {1}, Source: {2}Message: {3}
    ///   Exception: {4}&quot;, DateTime, Severity, SourceName, UserMessage, ExceptionMessage"
    /// Note: trace source 'traceOutputOptions' attribute is ignored as the initializeData 'Format' property handles this much better.
    /// Additional dictionary items are handled by the derived class.
    /// </summary>
    /// <typeparam name="MSG">type of message to write</typeparam>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public abstract class TraceListenerBase<MSG> : TraceListener
    {
        protected IWriterBase<MSG> m_writer; //object that actually performs the write to the output destination.
        private bool m_initialized = false; //initialize listener properties only upon demand.
        private string m_initializeData = string.Empty; //string from app.config: configuration/system.diagnostics/sharedListeners/add[name='listenername']/@initializeData
        //Log message formatter delegate that is initialized when there is a custom format string in 'initializeData'. Used by MsgFormatter();
        private Func<FormatTraceEventCache,string> m_formatter = null;

        /// <summary>
        /// True to squeeze string message arguments with duplicate 
        /// whitespace (including newlines) into a single line.
        /// </summary>
        protected bool SqueezeMsg { get; private set; }

        /// <summary>
        /// True to lazily write messages to the output destination (the default)
        /// False to write messages immediately (may be a performance hit).
        /// </summary>
        protected bool AsyncWrite { get; private set; }

        /// <summary>
        /// Indent padding string for subsequent lines in a multiline field. Will begin with '\n' 
        /// for easy search and replace. This value is computed from app.config Trace indentsize 
        /// attribute or overridden by the listener initializeData IndentSize property.
        /// </summary>
        protected string IndentPadding { get; private set; }

        /// <summary>
        /// Only true if derived type is FileTraceListener and 'Format' property == "{0},\"{1}\",{2}, ..."
        /// </summary>
        protected bool IsCSV { get; set; }

        protected bool FormatDeclared { get; set; }

        public TraceListenerBase() : base() { }
        /// <summary>
        /// Constructor with data defined in App.Config "sharedListeners/add" node 
        /// "initializeData" attribute.
        /// This constructor gets called multiple times. Therefore we initialize 
        /// only upon demand of first log message.
        /// </summary>
        /// <param name="initializeData"></param>
        public TraceListenerBase(string initializeData)
        {
            m_initializeData = initializeData;
        }

        private void InitializeBase()
        {
            if (m_initialized) return;
            m_initialized = true;
            try
            {
                Log.LogInit(); //Restart, in case logging has been closed.

                var data = m_initializeData.ToDictionary(';', '=');
                SqueezeMsg = data.GetValue("SqueezeMsg").CastTo<bool>();
                AsyncWrite = data.GetValue("Async").CastTo(true);
                var format = data.GetValue("Format");
                int indentSize = data.GetValue("IndentSize").CastTo(base.IndentSize < 1 ? 0 : base.IndentSize);
                IndentPadding = indentSize < 1 ? string.Empty : "\n".PadRight(indentSize + 1); //for string.Replace("\n",m_indentPadding);

                data.Remove("SqueezeMsg");   //This is relevant to this base class only.
                data.Remove("Async");
                data.Remove("Format");
                data.Remove("IndentSize");

                if (!format.IsNullOrEmpty())
                {
                    FormatDeclared = true;
                    IsCSV = this is FileTraceListener && IsFormatCsv(format);
                    m_formatter = Tool.GetFormatter<FormatTraceEventCache>(format);
                }
                else FormatDeclared = false;

                Initialize(data); //call the parent's Initialize() abstract method.
            }
            catch (Exception ex)
            {
                Log.InternalError("Failure Initializing {0} message writer. initializeData=\"{1}\"\nException={2}", this.Name, m_initializeData, ex);
            }
        }

        private bool IsFormatCsv(string format)
        {
            int commas = 0;
            char prevC = '\0';
            foreach(var c in format)
            {
                if (prevC=='\\' && (c == 'r' || c == 'n')) return false; //handle newline substrings
                if (c == '\r' || c == '\n') return false;
                if (c == ',') commas++;
                prevC = c;
            }
            return (commas > 1);
        }

        public override void Close()
        {
            if (m_writer == null) return;
            m_writer.Close();
            m_writer = null;
            m_initialized = false; //reset because if this listener is called again after closure, we can reinitialize
        }
        protected override void Dispose(bool disposing) { if (disposing) this.Close(); }

        /// <summary>
        /// System.Diagnostics.Trace.Writexxxx(msg) methods are used exclusively by all basic System.Diagnostics.Trace logging.
        /// By default, configuration for these API are handled via the App.Config "trace" node.
        /// Trace Source and Severity are NOT supported. Therefore by default source=="TRACE" and severity="Verbose".
        /// If TraceSource "TRACE" is defined in the App.Config "sources" node, the severity may be modified via "switchValue" attribute.
        /// Trace.Fail(msg);
        /// Trace.Write(msg);
        /// Trace.WriteLine(msg);
        /// Debug.Print(msg);
        /// Debug.Write(msg);
        /// Debug.WriteLine(msg);
        /// </summary>
        /// <param name="o"></param>
        public override void Write(object o) { WriteLine(o, null); }
        public override void WriteLine(object o) { WriteLine(o, null); }
        public override void Write(string message) { WriteLine(message, null); }
        public override void WriteLine(string message) { WriteLine(message, null); }

        /// <summary>
        /// System.Diagnostics.Trace.Writexxxx(msg,source) methods are used exclusively by all basic System.Diagnostics.Trace logging.
        /// By default, configuration for these API are handled via the App.Config "sources" node specified by the source argument.
        /// If the source is not found in the App.Config "sources" node, this message will have the default severity="Verbose" with 
        /// the listener as defined by the App.Config "trace" node.
        /// Trace.Write(msg,source);
        /// Trace.WriteLine(msg,source);
        /// Debug.Write(msg,source);
        /// Debug.WriteLine(msg,source);
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        public override void Write(object o, string source) { WriteLine(o, source); }
        public override void WriteLine(object o, string source) { WriteLine(o.ToString(), source); }
        public override void Write(string message, string source) { WriteLine(message, source); }
        public override void WriteLine(string message, string source)
        {
            try
            {
                InitializeBase();
                if (m_writer == null) return;
                if (string.IsNullOrEmpty(message)) message = "";
                if (string.IsNullOrEmpty(source)) source = "TRACE";

                TraceSource ts = Log.GetTraceSource(source);
                if (((int)ts.Switch.Level & (int)TraceEventType.Verbose) == 0) return;
                //message forwarded to the appropriate TraceSource
                ts.TraceEvent(TraceEventType.Verbose, Log.GetTraceSourceIndex(source), message);
            }
            catch (Exception ex)
            {
                Log.InternalError("Failure Writing {0} WriteLine message. initializeData=\"{1}\"\nException={2}", this.Name, m_initializeData, ex);
            }
        }

        /// <summary>
        /// System.Diagnostics.TraceSource.TraceEvent() is principally used by System.Diagnostics.TraceSource logging.
        /// Trace.TraceError(msg) --> TraceSource.TraceEvent(TraceEventType.Error,0,msg), source=app filename
        /// Trace.TraceWarning(msg) --> TraceSource.TraceEvent(TraceEventType.Warning,0,msg), source=app filename
        /// Trace.TraceInformation(msg) --> TraceSource.TraceEvent(TraceEventType.Information,0,msg), source=app filename
        /// TraceSource.TraceInformation(msg) --> TraceSource.TraceEvent(TraceEventType.Information,0,msg), source=app filename
        /// TraceSource.TraceEvent(severity,id,msg)
        /// TraceSource.TraceData() --> TraceSource.TraceEvent()
        /// TraceSource.TraceTransfer() --> TraceSource.TraceEvent()
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="severity"></param>
        /// <param name="id"></param>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id)
        {
            TraceEvent(eventCache, source, severity, id, string.Empty);
        }
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format)) return;
            TraceEvent(eventCache, source, severity, id, Log.SafeFormat(format,args));
        }
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType severity, int id, string message)
        {
            try
            {
                if (id == 0 && !source.Equals("TRACE", StringComparison.OrdinalIgnoreCase)) //Occurs when msg is coming directly from Trace API
                {
                    Log.GetTraceSource("TRACE").TraceEvent(severity, 0, message);
                    return;
                }

                InitializeBase();
                if (m_writer == null) return;
                if (base.Filter != null && !base.Filter.ShouldTrace(eventCache, source, severity, id, message, null, null, null)) return;

                if (Log.EventCache != null) //Log.EventCache is thread-local, so this is safe.
                {
                    //If the System.Diagnostics builtin api post the identical severity/sourceId/usermessage multiple times in a
                    //row within this thread, then the formatted output will show the same information, including the datetime stamp!
                    if (Log.EventCache.SourceId != (ushort)id || Log.EventCache.Severity != severity || Log.EventCache.UserMessage.GetHashCode() != message.GetHashCode())
                        Log.EventCache = new FormatTraceEventCache(eventCache, severity, source, (ushort)id, message);
                }
                else Log.EventCache = new FormatTraceEventCache(eventCache, severity, source, (ushort)id, message);

                m_writer.Write(TraceMsgFormatter(eventCache, severity, source, (ushort)id, message));
            }
            catch (Exception ex)
            {
                Log.InternalError("Failure Writing {0} TraceEvent message. initializeData=\"{1}\"\nException={2}", this.Name, m_initializeData, ex);
            }
        }

        /// <summary>
        /// System.Diagnostics.TraceSource.TraceData() calls .ToString() on all the 
        /// object data and then forwards the result to TraceEvent() for processing.
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="severity"></param>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, params object[] data)
        {
            base.TraceData(eventCache, source, severity, id, data);
        }
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType severity, int id, object data)
        {
            base.TraceData(eventCache, source, severity, id, data);
        }

        /// <summary>
        /// This is used internally by WCF and WPF Activity tracing. The formatted result is forwarded to TraceEvent();
        /// This executes the following single statement: 
        /// TraceEvent(eventCache, source, TraceEventType.Transfer, id, message + ", relatedActivityId=" + relatedActivityId.ToString());
        /// </summary>
        /// <param name="eventCache"></param>
        /// <param name="source"></param>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <param name="relatedActivityId"></param>
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
        }

        /// <summary>
        /// Utility to build custom message from format string specified in 'initializeData'.
        /// Available for use by derived TraceMsgFormatter().
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="severity"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceId"></param>
        /// <param name="message"></param>
        /// <returns>formatted message or null if "Format" element in initializeData is undefined.</returns>
        protected string MsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            if (m_formatter == null) return null;
            Log.EventCache.SqueezeMsg = this.SqueezeMsg;
            Log.EventCache.IndentPadding = this.IndentPadding;
            string result = m_formatter(Log.EventCache);
            Log.EventCache.SqueezeMsg = false;
            Log.EventCache.IndentPadding = string.Empty;
            return result;
        }
         
        /// <summary>
        /// Message builder that creates the message object that is written out by the logger/writer.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="severity"></param>
        /// <param name="sourceName"></param>
        /// <param name="sourceId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract MSG TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message);

        /// <summary>
        /// Initialize the objects that the derived class will be needing. This is performed 
        /// here instead of a constructor because the System.Diagnostics will instaniate
        /// this many times without actually logging anything. This is called just 
        /// before the first log message is to be written.
        /// </summary>
        /// <param name="initializeData">value found in the app.config listener attribute "initializeData"</param>
        protected abstract void Initialize(Dictionary<string,string> initializeData);
    }
    #endregion TraceListener base class

    #region TraceRedirectorListener - clone log messages to user-specified delegate (for internal use only)
    /// <summary>
    /// For internal use only. Used for posting messages to the static event handlers of 
    /// Log.RedirectorWriter. 'initializeData' is a dictionary of name/value pairs, however 
    /// these are all handled by the base class. Note: All 'initializeData' properties 
    /// (except 'Async') are ignored as message formatting is the responsibility of the 
    /// subscribed handlers.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    internal sealed class TraceRedirectorListener : TraceListenerBase<FormatTraceEventCache>
    {
        public const string DEFAULTNAME = "_Redirector"; //default listener name.

        /// <summary>
        /// Written to exclusively by Log.RedirectorWriter add/remove methods
        /// </summary>
        public static event Action<FormatTraceEventCache> RedirectorWriter;

        /// <summary>
        /// Log.RedirectorWriter add/remove methods need to know if there are any subscribed handlers.
        /// </summary>
        public static bool HasEventHandler { get { return RedirectorWriter != null; } }

        //There can only be one instance of the message writer handler. Really required when posting
        //messages thru the asynchronous thread message queue as we want only ONE message queue.
        static IWriterBase<FormatTraceEventCache> m_globalWriter = null;

        public TraceRedirectorListener() : base() { if (string.IsNullOrWhiteSpace(Name)) Name = DEFAULTNAME; }
        public TraceRedirectorListener(string initializeData) : base(initializeData) { if (string.IsNullOrWhiteSpace(Name)) Name = DEFAULTNAME; }

        protected override FormatTraceEventCache TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            //We don't perform any custom message formatting here. It is the responsibility of the event subscriber.
            return Log.EventCache;
        }

        protected override void Initialize(Dictionary<string, string> initializeData)
        {
            if (m_globalWriter == null || m_globalWriter.IsClosed)
            {
                m_globalWriter = new QWriter(base.Name, base.AsyncWrite);
            }
            base.m_writer = m_globalWriter;
        }
        public override void Close()
        {
            base.Close();
            m_globalWriter = null;
        }

        #region class QWriter
        private class QWriter : QWriterBase<FormatTraceEventCache>
        {
            public QWriter(string listenerName, bool async) : base(listenerName, async) { }
            protected override void LoggerInit(object[] args) { }
            protected override void LoggerWrite(FormatTraceEventCache cache)
            {
                try
                {
                    if (RedirectorWriter != null)
                        RedirectorWriter(cache);
                }
                catch { }
            }
            protected override void LoggerClose() { }
        }
        #endregion
    }
    #endregion

    #region EventLogTraceListener
    /// <summary>
    /// Write log messages to the Windows Event Log.
    /// Creates Event log and/or source if it does not already exist.
    /// 'initializeData' is a dictionary of name/value pairs.
    /// These are: 
    ///   Machine="." - computer whos event log to write to. Requires write access.
    ///   Log=(no default). EventLog log to write to. If undefined EventLog logging disabled.
    ///   Source=(no default). EventLog source to write to. If undefined EventLog logging disabled.
    /// If 'Format' is undefined, the default is: 
    ///   string.Format("Category: {0}\r\n{1}{2}", SourceName, UserMessage, Exception);
    /// </summary>
    [EventLogPermission(SecurityAction.Assert, PermissionAccess = EventLogPermissionAccess.Administer)]
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public class EventLogTraceListener : TraceListenerBase<EventLogTraceListener.QMsg>
    {
        public EventLogTraceListener() : base() { }
        public EventLogTraceListener(string initializeData) : base(initializeData) { }

        #region  public class QMsg
        /// <summary>
        /// For passing complex message to EventLogTraceListener.QWriter.Write().
        /// For internal use only by EventLogTraceListener and it's private IWriterBase class
        /// </summary>
        public class QMsg
        {
            public string message;
            public EventLogEntryType type;
            public int eventID;
            public ushort source;
            public QMsg(string message, EventLogEntryType type, int eventID, ushort source)
            {
                this.message = message;
                this.type = type;
                this.eventID = eventID;
                this.source = source;
            }
        }
        #endregion

        protected override void Initialize(Dictionary<string,string> initializeData)
        {
            string machine = initializeData.GetValue("Machine").CastTo(".");
            string log = initializeData.GetValue("Log").CastTo("Application");
            string source = initializeData.GetValue("Source") ?? "";
            if (source.Length == 0)
            {
                Log.InternalError("AppConfig missing EventLog listener initialization data. 'Source' is undefined. EventLog logging disabled.");
                return;
            }
            if (CreateEventLog(ref machine, ref log, ref source))
                Log.InternalError("New EventLog Created. OS needs to be rebooted.");

            base.m_writer = new QWriter(base.Name, base.AsyncWrite, machine, log, source);
        }

        /// <summary>
        /// Create an event log and/or event source if it doesn't exist. The event log system appears
        /// to have namespaces/scope, but it really doesn't. All event names (log and source) must be
        /// unique on a given machine. Due to permission and reboot issues, this method may need to be
        /// called from the context of the product installer.
        /// </summary>
        /// <param name="machine">The machine to create the eventlog/source on. Upon error, it reverts to "." (local machine).</param>
        /// <param name="log">Event log to use or create. Upon error, it reverts to "Application".</param>
        /// <param name="source">Event source to use or create. Upon error, it reverts to executable name</param>
        /// <returns>
        /// There can only be one Event Source in the entire Event Service. It does not recognize Logs as distinctive namespaces.
        /// Thus when moving an event source from one log to another, The event service needs to be restarted to re-read its configuration.
        /// However the the event service cannot be stopped. The entire OS must be rebooted for the event source move to be recognized.
        /// Unless dependent services are stopped first...
        /// <see cref="https://stackoverflow.com/questions/19169529/reinitialize-windows-event-log-service-without-reboot"/>
        /// Therefore this method returns true if reboot is required.
        /// </returns>
        public static bool CreateEventLog(ref string machine, ref string log, ref string source)
        {
            bool reboot = false; //True if created. Also means 'needs to reboot OS'
            try
            {
                if (EventLog.SourceExists(log) && !EventLog.LogNameFromSourceName(log, machine).Equals(log, StringComparison.OrdinalIgnoreCase))
                {
                    string oldLog = EventLog.LogNameFromSourceName(log, machine);
                    int kount = 9999;  //assume we can't delete the log.
                    //do not clean up official Windows logs
                    if (!oldLog.Equals("Application", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("Security", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("System", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("Setup", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("", StringComparison.OrdinalIgnoreCase))
                    {
                        EventLog logger = null;
                        try
                        {
                            logger = new EventLog(oldLog, machine, log);
                            kount = logger.Entries.Count;
                        }
                        catch { }
                        finally
                        {
                            if (logger != null) logger.Dispose();
                        }
                    }
                    EventLog.DeleteEventSource(log, machine);
                    if (kount == 0) EventLog.Delete(oldLog, machine);  //clean up if empty
                    reboot = true;
                }

                if (EventLog.SourceExists(source) && !EventLog.LogNameFromSourceName(source, machine).Equals(log, StringComparison.OrdinalIgnoreCase))
                {
                    string oldLog = EventLog.LogNameFromSourceName(source, machine);
                    int kount = 9999;  //assume we can't delete the log.
                    //do not clean up official Windows logs
                    if (!oldLog.Equals("Application", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("Security", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("System", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("Setup", StringComparison.OrdinalIgnoreCase)
                        && !oldLog.Equals("", StringComparison.OrdinalIgnoreCase))
                    {
                        EventLog logger = null;
                        try
                        {
                            logger = new EventLog(oldLog, machine, source);
                            kount = logger.Entries.Count;
                        }
                        catch { }
                        finally
                        {
                            if (logger != null) logger.Dispose();
                        }
                    }
                    if (!source.Equals(oldLog, StringComparison.OrdinalIgnoreCase)) EventLog.DeleteEventSource(source, machine);
                    if (kount == 0 || source.Equals(oldLog, StringComparison.OrdinalIgnoreCase)) EventLog.Delete(oldLog, machine);  //clean up if empty
                    reboot = true;
                }

                //Event log source does not exist, so create it.
                if (!EventLog.SourceExists(source))
                {
                    EventSourceCreationData ed = new EventSourceCreationData(source, log);
                    ed.MachineName = machine;
                    //ed.CategoryCount = Log.Categories.Count;
                    //ed.CategoryResourceFile = CreateCategoryResourceDll(Log.Categories);
                    EventLog.CreateEventSource(ed);
                    var logger = new EventLog(log, machine, source);
                    logger.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 0);
                    if (logger.MaximumKilobytes < 10240) logger.MaximumKilobytes = 10240;
                    logger.Dispose();
                    //reboot = true;  //Reboot is only necessary when MOVING an event source.
                }
            }
            catch (Exception ex)
            {
               var newLog = "Application";
               var newSource = Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                Log.InternalError("Failure creating new EventLog:Source {0}:{1}.\nDefaulting to {2}:{3}.\nException={4}", log, source, newLog, newSource, ex);
                //Hack: revert back to original pre-existing names.
                machine = ".";
                log = newLog;
                source = newSource;
            }

            return reboot;
        }

        protected override QMsg TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            string result = base.MsgFormatter(cache, severity, sourceName, sourceId, message);
            if (result == null)
            {
                Log.EventCache.SqueezeMsg = this.SqueezeMsg;
                Log.EventCache.IndentPadding = this.IndentPadding;
                result = string.Format("Category: {0}\r\n{1}{2}", sourceName, Log.EventCache.UserMessage, 
                    (Log.EventCache.HasException ? Environment.NewLine+Log.EventCache.Exception : 
                    (((int)Log.EventCache.Severity&(int)SourceLevels.Warning)!=0 ? Environment.NewLine+Log.EventCache.CallStack : string.Empty)));
                Log.EventCache.SqueezeMsg = false;
                Log.EventCache.IndentPadding = string.Empty;
            }

            EventLogEntryType eventType;
            switch (severity)
            {
                case TraceEventType.Critical: eventType = EventLogEntryType.Error; break;
                case TraceEventType.Error: eventType = EventLogEntryType.Error; break;
                case TraceEventType.Warning: eventType = EventLogEntryType.Warning; break;
                case TraceEventType.Information: eventType = EventLogEntryType.Information; break;
                case TraceEventType.Verbose: eventType = EventLogEntryType.SuccessAudit; break;
                default: eventType = EventLogEntryType.Information; break;
            }

            return new QMsg(result, eventType, (int)severity, sourceId);
        }

        #region private class QWriter : QWriterBase<QMsg>
        /// <summary>
        /// Asynchronous write to event log. 
        /// Synchronizes multiple writes to the log file.
        /// </summary>
        private class QWriter : QWriterBase<QMsg>
        {
            private EventLog m_logger = null;

            public QWriter(string listenerName, bool async, string machine, string log, string source) : base(listenerName, async, machine, log, source) { }

            protected override void LoggerInit(object[] args)
            {
                string machine = args[0].ToString();
                string log = args[1].ToString();
                string source = args[2].ToString();
                m_logger = new EventLog(log, machine, source);
            }

            protected override void LoggerWrite(QMsg msg)
            {
                while (msg.message.Length > 30016) //The official longest event log message is 32766, however that seems still too long...
                {
                    string m = msg.message.Substring(0, 30000) + "\r\n[CONTINUED...]\r\n";
                    m_logger.WriteEntry(m, msg.type, msg.eventID, (short)msg.source);
                    msg.message = "[...CONTINUED]\r\n" + msg.message.Substring(30000); //"[...CONTINUED]\r\n".Length == 16
                }
                m_logger.WriteEntry(msg.message, msg.type, msg.eventID, (short)msg.source);
            }

            protected override void LoggerClose()
            {
                if (m_logger == null) return;
                m_logger.Close();
                m_logger = null;
            }
        }
        #endregion
    }
    #endregion

    #region FileTraceListener
    /// <summary>
    /// Write log messages to the specified file.
    /// 'initializeData' is a dictionary of name/value pairs
    /// These are 
    ///   Filename=Same as appname with a ".log" extension - Relative or full filepath which 
    ///       may contain environment variables including pseudo-environment variables: ProcessName, 
    ///       ProcessId(as 4 hex digits)), AppDomainName, and BaseDir. DateTime in filename is not supported.
    ///   MaxSize=104857600 (100MB) - max file size before starting over with a new file.
    ///   MaxFiles=-1 (infinite) - Maximum number of log files before deleting the oldest.
    ///   FileHeader=(no default) - String literal to insert as the first line(s) in a new file.
    ///   FileFooter=(no default) - String literal to append as the last line(s) in a file being closed.
    /// If 'Format' is undefined, the default is (CSV): 
    ///   string.Format("{0:yyyy-MM-dd HH:mm:ss.fff},{1},{2},\"{3}\"", LocalDateTime, Severity, SourceName, UserMessage);
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public sealed class FileTraceListener : TraceListenerBase<string>
    {
        private const string m_extension = ".log";
        private string m_filename;
        private StreamWriter m_logger = null;
        private int m_maxsize;
        private int m_maxfiles;
        Mutex m_mutex = null;
        private string m_fileheader;
        private string m_filefooter;

        public FileTraceListener() : base() { }
        public FileTraceListener(string initializeData) : base(initializeData) { }

        protected override void Initialize(Dictionary<string, string> initializeData)
        {
            m_filename = ExpandFileName(initializeData.GetValue("Filename"));
            m_maxsize = initializeData.GetValue("MaxSize").CastTo(100 * 1048576); //100MB is the default
            m_maxfiles = initializeData.GetValue("MaxFiles").CastTo<int>(); //-1 == infinity
            m_fileheader = initializeData.GetValue("FileHeader")?.Trim() ?? "";
            m_filefooter = initializeData.GetValue("FileFooter")?.Trim() ?? "";
            if (m_fileheader.Length == 0 && !base.FormatDeclared) m_fileheader = "DateTime,Severity,SourceName,Message"; //default header if no format

            bool createdNew;
            try { m_mutex = new Mutex(false, m_filename.ToIdentifier(), out createdNew); } catch { }
            if (m_mutex != null) m_mutex.WaitOne(3000);
            m_logger = OpenFile(ref m_filename, out createdNew);
            if (m_mutex != null) m_mutex.ReleaseMutex();

            base.m_writer = new QWriter(base.Name, base.AsyncWrite, m_logger, m_maxsize, m_maxsize, m_mutex, m_fileheader, m_filefooter);
        }

        private string ExpandFileName(string basename)
        {
            string appName = Process.GetCurrentProcess().MainModule.FileName;

            if (string.IsNullOrEmpty(basename))
            {
                 return Path.Combine(Path.GetDirectoryName(appName), Path.GetFileNameWithoutExtension(appName)).Replace(".vshost", "") + m_extension;
            }

            if (basename.Contains('%'))
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    Environment.SetEnvironmentVariable("ProcessName", p.ProcessName);
                    Environment.SetEnvironmentVariable("ProcessId", p.Id.ToString("X4"));
                }
                Environment.SetEnvironmentVariable("AppDomainName", AppDomain.CurrentDomain.FriendlyName);
                Environment.SetEnvironmentVariable("BaseDir", AppDomain.CurrentDomain.BaseDirectory);
                basename = Environment.ExpandEnvironmentVariables(basename);
            }
            if (Path.IsPathRooted(basename)) basename = Path.GetFullPath(basename);
            else basename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(appName), basename));
            if (!Directory.Exists(Path.GetDirectoryName(basename)))
            {
                try { Directory.CreateDirectory(Path.GetDirectoryName(basename)); }
                catch { }
            }
            if (!Directory.Exists(Path.GetDirectoryName(basename)))
            {
                basename = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(appName), Path.GetFileName(basename)));
            }
            return basename;
        }

        /// <summary>
        /// Open file ready to begin writing to.
        /// </summary>
        /// <param name="outFile">full path of file to write to. Path may be changed to LocalApplicationData/appname if there is no write access.</param>
        /// <param name="maxsize"></param>
        /// <param name="maxfiles"></param>
        /// <param name="createdNew"></param>
        /// <returns></returns>
        private StreamWriter OpenFile(ref string outFile, out bool createdNew)
        {
            StreamWriter sw = null;
            createdNew = false;

            while (true)
            {
                string dir = Path.GetDirectoryName(outFile);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                createdNew = !File.Exists(outFile);
                if (!createdNew && new FileInfo(outFile).Length > m_maxsize && !IsFileLocked(outFile)) //move over if exceeds maxsize
                {
                    try
                    {
                        string s = Path.Combine(Path.GetDirectoryName(outFile), Path.GetFileNameWithoutExtension(outFile)) + File.GetCreationTime(outFile).ToString(".yyyyMMddHHmmssfff") + Path.GetExtension(outFile);
                        s = UnusedFilename(s);
                        File.Move(outFile, s);
                        PurgeLogs(outFile, m_maxfiles);
                        createdNew = true;
                    }
                    catch { }
                }
                try 
                { 
                    sw = new StreamWriter(File.Open(outFile, FileMode.Append, createdNew||string.IsNullOrEmpty(m_filefooter)?FileAccess.Write:FileAccess.ReadWrite, FileShare.Read | FileShare.Write | FileShare.Delete), Encoding.UTF8);
                    sw.AutoFlush = true;
                    if (createdNew)
                    {
                        File.SetCreationTime(outFile, DateTime.Now);
                        if (!string.IsNullOrEmpty(m_fileheader)) WriteLineFlush(sw, m_fileheader);
                    }
                    else if (!string.IsNullOrEmpty(m_filefooter)) //need to strip off the footer before log writing commences.
                    {
                        FileStream fs = (FileStream)sw.BaseStream;
                        byte[] bFooter = new byte[m_filefooter.Length*2];
                        fs.Seek(-bFooter.Length, SeekOrigin.End);
                        int kountread = fs.Read(bFooter, 0, bFooter.Length);
                        string szFooter = UTF8Encoding.UTF8.GetString(bFooter);
                        int index = szFooter.IndexOf(m_filefooter);
                        if (index == -1) index = 0;
                        else index -= kountread;
                        fs.Seek(index, SeekOrigin.End);
                        fs.SetLength(fs.Position);
                    }
                }
                catch { }
                if (sw == null) //try again in a directory guarenteed to be writeable.
                {
                    string name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
                    string s = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + name;
                    if (!Directory.Exists(s)) Directory.CreateDirectory(s);
                    outFile = Path.GetFullPath(Path.Combine(s, Path.GetFileName(outFile)));
                    continue;
                }
                break;
            }
            return sw;
        }

        private static void PurgeLogs(string outFile, int maxfiles)
        {
            if (maxfiles < 1) return;
            var files = Directory.EnumerateFiles(Path.GetDirectoryName(outFile), Path.GetFileNameWithoutExtension(outFile) + ".*").OrderByDescending<string, DateTime>(m=>
            {
                string file = m;
                //The creation time is explicitly set when creating a new log file, but we don't trust it as people can touch 
                //the files and potentially change it because CreationTime is a property of the directory element, not the file.
                DateTime defalt = new FileInfo(file).CreationTime; 
                int index = file.LastIndexOf('.'); //=='.ext'
                if (index == -1) return defalt;
                index = file.LastIndexOf('.'); //=='.yyyyMMddHHmmssfff(00).ext'. may have trailing incremental version in the form of '(00)'. see UnusedFilename()
                if (index == -1) return defalt;
                string szDt = file.Substring(index + 1);
                int index2 = szDt.IndexOf<char>(x => x < '0' || x > '9');
                if (index2 != -1) szDt = szDt.Substring(0, index2);
                DateTime dt = defalt;
                DateTime.TryParseExact(szDt, "yyyyMMddHHmmssfff".Substring(0, szDt.Length), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                return dt;
            });
            int i = 0;
            foreach (var f in files)
            {
                i++;
                if (i <= maxfiles) continue; //Keep only 'maxfiles' files. Delete the rest.
                File.Delete(f);
            }
        }

        private static string UnusedFilename(string s)
        {
            if (File.Exists(s))
            {
                string fmt = Path.Combine(Path.GetDirectoryName(s), Path.GetFileNameWithoutExtension(s)) + "({0:00})" + Path.GetExtension(s);
                int i = 1;
                while (File.Exists(s)) s = string.Format(fmt, i++);
            }
            return s;
        }

        private bool IsFileLocked(string fn)
        {
            try
            {
                using (FileStream stream = new FileStream(fn, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //   still being written to
                //   or being processed by another thread
                //   or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        //Did another instance (not thread) change the filename? This feature is not available in .NET
        [DllImport("Kernel32.dll")] private static extern int GetFinalPathNameByHandle(IntPtr hFile, StringBuilder filePath, int maxPathLength, int flags);
        private static string GetPathName(FileStream fs) //get the CURRENT path name to this filestream
        {
            StringBuilder sb = new StringBuilder(260);
            int length = GetFinalPathNameByHandle(fs.SafeFileHandle.DangerousGetHandle(), sb, sb.Capacity, 0x08);
            return sb.Length == 0 ? fs.Name : sb.ToString().Substring(4);  //remove the "\\?\" prefix
        }

        private static void WriteLineFlush(StreamWriter sw, string msg)
        {
            sw.WriteLine(msg);
            sw.Flush(); //flush to FileStream
            ((FileStream)sw.BaseStream).Flush(true); //flush to disk
        }

        protected override string TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            string result = base.MsgFormatter(cache, severity, sourceName, sourceId, message);
            if (result != null) return result;

            IsCSV = true;
            Log.EventCache.SqueezeMsg = this.SqueezeMsg;
            Log.EventCache.IndentPadding = this.IndentPadding;
            result = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff},{1},{2},\"{3}\"", Log.EventCache.LocalDateTime, Log.EventCache.Severity, Log.EventCache.SourceName, Log.EventCache.UserMessage.Replace("\"","\"\""));
            Log.EventCache.SqueezeMsg = false;
            Log.EventCache.IndentPadding = string.Empty;
            return result;
        }

        #region class QWriter
        private class QWriter : QWriterBase<string>
        {
            private string m_filename;
            private StreamWriter m_logger;
            private Mutex m_mutex;
            private int m_maxsize;
            private int m_maxfiles;
            private string m_fileheader;
            private string m_filefooter;

            public QWriter(string listenerName, bool async, StreamWriter logger, int maxsize, int maxfiles, Mutex mutex, string fileheader, string filefooter) : base(listenerName, async, logger, maxsize, maxfiles, mutex, fileheader, filefooter) { }

            protected override void LoggerInit(object[] args)
            {
                m_logger = (StreamWriter)args[0];
                m_maxsize = (int)args[1];
                m_maxfiles = (int)args[2];
                m_mutex = (Mutex)args[3];
                m_mutex = (Mutex)args[3];
                m_fileheader = (string)args[4];
                m_filefooter = (string)args[5];

                m_filename = GetPathName((FileStream)m_logger.BaseStream);
            }

            protected override void Lock()
            {
                if (m_mutex != null) m_mutex.WaitOne(60000);
                m_logger.BaseStream.Seek(0, SeekOrigin.End);
                if (!GetPathName((FileStream)m_logger.BaseStream).Equals(m_filename, StringComparison.OrdinalIgnoreCase)) //file renamed by some other instance?
                {
                    m_logger.Dispose();
                    m_logger = new StreamWriter(File.Open(m_filename, FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Write | FileShare.Delete));
                }
                else if (m_logger.BaseStream.Position > m_maxsize)  //file too large? rename it and start over.
                {
                    if (!string.IsNullOrEmpty(m_filefooter)) WriteLineFlush(m_logger, m_filefooter);
                    m_logger.Dispose();
                    string s = Path.Combine(Path.GetDirectoryName(m_filename), Path.GetFileNameWithoutExtension(m_filename)) + File.GetCreationTime(m_filename).ToString(".yyyyMMddHHmmssfff") + Path.GetExtension(m_filename);
                    s = UnusedFilename(s);
                    File.Move(m_filename, s);
                    PurgeLogs(m_filename, m_maxfiles);
                    m_logger = new StreamWriter(File.Open(m_filename, FileMode.Append, FileAccess.Write, FileShare.Read | FileShare.Write | FileShare.Delete));
                    if (!string.IsNullOrEmpty(m_fileheader)) WriteLineFlush(m_logger, m_fileheader);
                    File.SetCreationTime(m_filename, DateTime.Now);
                }
            }

            protected override void LoggerWrite(string msg)
            {
                m_logger.WriteLine(msg);
            }

            protected override void Unlock()
            {
                if (m_logger != null)
                {
                    m_logger.Flush(); //flush to FileStream
                    ((FileStream)m_logger.BaseStream).Flush(true); //flush to disk
                }
                if (m_mutex != null) m_mutex.ReleaseMutex();
            }

            protected override void LoggerClose()
            {
                if (m_logger != null)
                {
                    if (m_mutex != null) m_mutex.WaitOne(60000);
                    if (!string.IsNullOrEmpty(m_filefooter)) WriteLineFlush(m_logger, m_filefooter);
                    m_logger.Close(); 
                    m_logger = null;
                    if (m_mutex != null) m_mutex.ReleaseMutex();
                }
                if (m_mutex != null) { m_mutex.Close(); m_mutex = null; }
            }
        }
        #endregion

    }
    #endregion FileTraceListener

    #region DebugTraceListener
    /// <summary>
    /// Write log messages to the debugger output.
    /// Output is available to an external debug viewer such as Microsoft's Dbgview.exe or 
    /// the VisualStudio debugger output window, but not both.
    /// See: https://technet.microsoft.com/en-us/sysinternals/bb896647
    /// Note: There are no 'initializeData' properties unique to this derived class.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public sealed class DebugTraceListener : TraceListenerBase<string>
    {
        public DebugTraceListener() : base() { }
        public DebugTraceListener(string initializeData) : base(initializeData) { }

        protected override void Initialize(Dictionary<string,string> initializeData)
        {
            base.m_writer = new QWriter(base.Name, base.AsyncWrite);
        }

        protected override string TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            string result = base.MsgFormatter(cache, severity, sourceName, sourceId, message);
            if (result != null) return result + Environment.NewLine;

            Log.EventCache.SqueezeMsg = this.SqueezeMsg;
            Log.EventCache.IndentPadding = this.IndentPadding;
            result = string.Format("Debug: {0:yyyy/MM/dd HH:mm:ss.fff} : {1} : {2} : {3}", Log.EventCache.LocalDateTime, Log.EventCache.Severity, Log.EventCache.SourceName, Log.EventCache.UserMessage);
            Log.EventCache.SqueezeMsg = false;
            Log.EventCache.IndentPadding = string.Empty;
            return result + Environment.NewLine;
        }

        #region class QWriter
        private class QWriter : QWriterBase<string>
        {
            [DllImport("Kernel32.dll")] private static extern void OutputDebugString(string errmsg);
            private Action<string> m_logger;

            public QWriter(string listenerName, bool async) : base(listenerName, async) { }

            protected override void LoggerInit(object[] args)
            {
                if (Debugger.IsAttached && Debugger.IsLogging())
                    m_logger = delegate(string msg) { System.Diagnostics.Debugger.Log(0, null, msg); };
                else
                    m_logger = OutputDebugString;
            }

            protected override void LoggerWrite(string msg) { m_logger(msg); }

            protected override void LoggerClose() { }
        }
        #endregion
    }
    #endregion DebugTraceListener

    #region EmailTraceListener
    /// <summary>
    /// Write log messages as email messages to the mail server.
    /// 'initializeData' is a dictionary of name/value pairs.
    /// These are: 
    ///   Subject="Log: "+SourceName - email subject line.
    ///   SendTo=(no default) - comma-delimited list of email addresses to send to. Whitespace is ignored. Addresses may be in the form of "username@domain.com" or "UserName &lt;username@domain.com&gt;". If undefined, email logging is disabled.
    ///   The following are explicitly defined here or defaulted from app.config configuration/system.net/mailSettings/smtp;
    ///   SentFrom=system.net/mailSettings/smtp/@from - the 'from' email address. Whitespace is ignored. Addresses may be in the form of "username@domain.com" or "UserName &lt;username@domain.com&gt;".
    ///   ClientDomain=LocalHost - aka "www.gmail.com"
    ///   DefaultCredentials=true - true to use windows authentication, false to use UserName and Password.
    ///   UserName=(no default)
    ///   Password=(no default)
    ///   EnableSsl=false - 
    ///   MailServer=(no default) - aka "smtp.gmail.com"
    ///   Port=25 - mail server listener port to send messages to.
    /// If 'Format' is undefined, the default is: 
    ///   string.Format("DateTime : {0:yyyy/MM/dd HH:mm:ss.fff}\r\nSeverity : {1}\r\nSource   : {2}\r\nMessage  : {3}", LocalDateTime, Severity, SourceName, UserMessage);
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public sealed class EmailTraceListener : TraceListenerBase<MailMessage>
    {
        private string m_subject;
        private MailAddress m_sentFrom;
        private MailAddressCollection m_sendTo;

        public EmailTraceListener() : base() { }
        public EmailTraceListener(string initializeData) : base(initializeData) { }

        protected override void Initialize(Dictionary<string, string> initializeData)
        {
            System.Net.Configuration.SmtpSection smtp = null;
            try { smtp = System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp") as System.Net.Configuration.SmtpSection; }
            catch { }

            m_subject = initializeData.GetValue("Subject") ?? "";

            string sentFrom = initializeData.GetValue("SentFrom").CastTo(smtp != null ? smtp.From : string.Empty);
            if (sentFrom.Length == 0) { Log.InternalError("AppConfig missing MailTraceListener data. Both 'system.net/mailSettings/smtp/@from' and initializeData 'SentFrom' undefined. Mail logging disabled."); return; }
            m_sentFrom = new MailAddress(sentFrom);

            string sendTo = initializeData.GetValue("SendTo").CastTo(string.Empty);
            if (sendTo.Length == 0) { Log.InternalError("AppConfig missing MailTraceListener data. initializeData 'SendTo' undefined. Mail logging disabled."); return; }
            m_sendTo = new MailAddressCollection();
            m_sendTo.Add(sendTo); //parses comma-delimited string

            string clientDomain = initializeData.GetValue("ClientDomain").CastTo(smtp != null && smtp.Network != null ? smtp.Network.ClientDomain ?? string.Empty : string.Empty);
            string mailServer = initializeData.GetValue("MailServer").CastTo(smtp != null && smtp.Network != null ? smtp.Network.Host ?? string.Empty : string.Empty);
            if (mailServer.Length == 0) { Log.InternalError("AppConfig missing MailTraceListener data. Both 'system.net/mailSettings/smtp/network/@host' and initializeData 'MailServer' undefined. Mail logging disabled."); return; }
            int port = initializeData.GetValue("Port").CastTo(smtp != null ? smtp.Network.Port : 25);
            bool enableSsl = initializeData.GetValue("EnableSsl").CastTo(smtp != null && smtp.Network != null ? smtp.Network.EnableSsl : false);
            bool useDefaultCredentials = initializeData.GetValue("DefaultCredentials").CastTo(smtp != null && smtp.Network != null ? smtp.Network.DefaultCredentials : true);
            string username = initializeData.GetValue("UserName").CastTo(smtp != null && smtp.Network != null ? smtp.Network.UserName ?? string.Empty : string.Empty);
            string password = initializeData.GetValue("Password").CastTo(smtp != null && smtp.Network != null ? smtp.Network.Password ?? string.Empty : string.Empty);

            base.m_writer = new QWriter(base.Name, base.AsyncWrite, clientDomain, mailServer, port, enableSsl, useDefaultCredentials, username, password);
        }

        protected override MailMessage TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            string result = base.MsgFormatter(cache, severity, sourceName, sourceId, message);
            if (result == null)
            {
                Log.EventCache.SqueezeMsg = this.SqueezeMsg;
                Log.EventCache.IndentPadding = this.IndentPadding;
                result = string.Format(@"
DateTime : {0:yyyy/MM/dd HH:mm:ss.fff}
Severity : {1}
Source   : {2}
Message  : {3}", Log.EventCache.LocalDateTime, Log.EventCache.Severity, Log.EventCache.SourceName, Log.EventCache.UserMessage, Log.EventCache.HasException ? Environment.NewLine+Log.EventCache.Exception: string.Empty);
                Log.EventCache.SqueezeMsg = false;
                Log.EventCache.IndentPadding = string.Empty;
            }

            var mm = new MailMessage();
            mm.From = m_sentFrom;
            foreach (var sendTo in m_sendTo) mm.To.Add(sendTo);
            mm.Subject = m_subject.Length == 0 ? "Log: "+sourceName : m_subject;
            mm.Body = result;

            return mm;
        }

        #region class QWriter
        private class QWriter : QWriterBase<MailMessage>
        {
            SmtpClient m_logger = null;

            public QWriter(string listenerName, bool async, string clientDomain, string mailServer, int port, bool enableSsl, bool useDefaultCredentials, string username, string password) : base(listenerName, async, clientDomain, mailServer, port, enableSsl, useDefaultCredentials, username, password) { }

            protected override void LoggerInit(object[] args)
            {
                string clientDomain = (string)args[0];
                string mailServer   = (string)args[1]; //aka host
                int    port         =    (int)args[2];
                bool   enableSsl    =   (bool)args[3];
                bool useDefaultCredentials = (bool)args[4];
                string username     = (string)args[5];
                string password     = (string)args[6];

                m_logger = new SmtpClient();
                m_logger.Host = mailServer;
                m_logger.Port = port;
                m_logger.EnableSsl = enableSsl;
                m_logger.UseDefaultCredentials = useDefaultCredentials; //this must be set BEFORE setting the credentials.
                //m_logger.Credentials = username.Length == 0 || password.Length == 0 ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(username, password, clientDomain.Length == 0 ? mailServer : clientDomain);
                m_logger.Credentials = username.Length == 0 || password.Length == 0 ? CredentialCache.DefaultNetworkCredentials : new NetworkCredential(username, password);

                if (clientDomain.Length > 0)
                {
                    //Bug: Unlike all the other properties, ClientDomain field can only be set from value in app.config!
                    typeof(SmtpClient).GetField("clientDomain", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(m_logger, clientDomain);
                }
            }

            protected override void LoggerWrite(MailMessage msg)
            {
                if (m_logger != null) m_logger.Send(msg); 
            }

            protected override void LoggerClose() 
            {
                if (m_logger == null) return;
                m_logger.Dispose();
                m_logger = null;
            }
        }
        #endregion
    }
    #endregion EmailTraceListener

    #region DataBaseTraceListener
    /// <summary>
    /// Write log messages to database table. Database table and relevant columns must already 
    /// exist and have a large enough length to support all possible message strings.
    /// 'initializeData' is a dictionary of name/value pairs.
    /// These are: 
    ///   ConnectionString=(no default) - a string key representing AppConfig ConfigurationManager.ConnectionStrings[] dictionary entry OR literal full SQL connection string.
    ///   SqlStatement=(no default) - SQL statement to insert logging values into the database table.
    ///   Examples:
    ///      "spStoredProcedure @LocalDateTime, @Severity, @SourceName, @UserMessage"
    ///      "spStoredProcedure @Date=@LocalDateTime, @Severity=@Severity, @Source=@SourceName, @Message=@UserMessage"
    ///      "spStoredProcedure @Date={0}, @Severity={1}, @Source={2}, @Message={3}", LocalDateTime, Severity, SourceName,@UserMessage
    ///      "INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES (@LocalDateTime, @Severity, @SourceName, @UserMessage)"
    ///      "INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES ({0}, {1}, {2}, {3})", LocalDateTime, Severity, SourceName, UserMessage
    /// The 'Format' and 'IndentSize' properties are not used.
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, Synchronization = true)]
    public sealed class DatabaseTraceListener : TraceListenerBase<object[]>
    {
        #region Sample Create DB Table SQL Statement
#if FALSE
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'LogApplication') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
DROP TABLE LogApplication
GO

CREATE TABLE LogApplication (
    [LogApplicationKey] INT IDENTITY(1,1) NOT NULL, --PK
    [DateTime] DATETIME NOT NULL,
    [TimeStamp] BIGINT NULL,
    [Severity] VARCHAR(16) NULL,
    [Source] VARCHAR(64) NULL,
    [Domain] VARCHAR(64) NULL,
    [EntryAssembly] VARCHAR(256) NULL,
    [ActivityId] UNIQUEIDENTIFIER NULL,
    [LogicalOperationStack] VARCHAR(MAX) NULL,
    [ProcessName] VARCHAR(256) NULL,
    [ProcessId] INT NULL,
    [ThreadName] VARCHAR(256) NULL,
    [ThreadId] INT NULL,
    [UserMessage] VARCHAR(MAX) NULL,
    [ExceptionMessage] VARCHAR(MAX) NULL,
    [Exception] VARCHAR(MAX) NULL,
    [UserData] VARCHAR(MAX) NULL,
)
GO
#endif
        #endregion

        private List<Func<FormatTraceEventCache, object[], object>> m_argFuncs = null;

        public DatabaseTraceListener() : base() { }
        public DatabaseTraceListener(string initializeData) : base(initializeData) { }

        protected override void Initialize(Dictionary<string, string> initializeData)
        {
            string connectionString = initializeData.GetValue("ConnectionString").CastTo(string.Empty);
            if (connectionString.Length == 0) { Log.InternalError("AppConfig missing SQL connection string. Database logging disabled."); return; }
            if (connectionString.All(m => (m >= 'A' && m <= 'Z') || (m >= 'a' && m <= 'z') || (m >= '0' && m <= '9')))
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionString];
                if (settings == null) { Log.InternalError("AppConfig ConfigurationManager.ConnectionStrings[\"{0}\"] is missing. Database logging disabled.", connectionString); return; }
                connectionString = settings.ConnectionString;
            }
            if (!PingConnection(connectionString)) { Log.InternalError("AppConfig connection string is invalid. Database logging disabled."); return; }

            string sqlStatement = initializeData.GetValue("SqlStatement").CastTo(string.Empty);
            if (sqlStatement.Length == 0) { Log.InternalError("AppConfig missing SQL statement. Database logging disabled."); return; }

            List<string> keys = new List<string>();
            List<string> args = new List<string>();
            sqlStatement = ParseQuery(sqlStatement, keys, args);
            SqlParameter[] parms = new SqlParameter[keys.Count];
            for (int i = 0; i < parms.Length; i++ )
            {
                parms[i] = new SqlParameter(keys[i].Length == 0 ? args[i] : keys[i], null);
            }

            m_argFuncs = new List<Func<FormatTraceEventCache, object[], object>>(parms.Length);
            foreach (var arg in args)
            {
                var v = FormatTraceEventCache.Properties.GetValue(arg);
                if (v == null)
                {
                    Log.InternalError("Warning: \"{0}\" Unknown event property. Replacing with empty string.", arg);
                    m_argFuncs.Add(FormatTraceEventCache.Properties["Null"]);
                    continue;
                }
                m_argFuncs.Add(v);
            }

            base.m_writer = new QWriter(base.Name, base.AsyncWrite, connectionString, sqlStatement, parms);
        }

        /// <summary>
        /// Simply tests the connection string against the server.
        /// </summary>
        /// <param name="cs">Connection string to validate</param>
        /// <returns>True if successful</returns>
        private static bool PingConnection(string cs)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;
            try
            {
                conn = new SqlConnection(cs);
                cmd = conn.CreateCommand();
                if (conn.State == ConnectionState.Closed) conn.Open();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "select system_user";
                string result = (cmd.ExecuteScalar() as string) ?? string.Empty;
                return result.Length > 0;
            }
            catch { return false; }
            finally
            {
                if (conn != null) { conn.Close(); conn.Dispose(); }
                if (cmd != null) { cmd.Dispose(); }
            }
        }

        /// <summary>
        /// Rewrite query to use SQL parameter variables.
        /// Examples:
        ///    "INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES ({0}, {1}, {2}, {3})", LocalDateTime, Severity, SourceName, UserMessage
        ///    "spStoredProcedure @Date={0}, @Severity={1}, @Source={2}, @Message={3}", LocalDateTime, Severity, SourceName,UserMessage
        ///    spStoredProcedure @LocalDateTime, @Severity, @SourceName, @UserMessage
        ///    spStoredProcedure @Date=@LocalDateTime, @Severity=@Severity, @Source=@SourceName, @Message=@UserMessage
        ///    INSERT INTO MyTable ([Date],Severity,Source,Message) VALUES (@LocalDateTime, @Severity, @SourceName, @UserMessage)
        /// </summary>
        /// <param name="query">source query format string</param>
        /// <param name="keys">populate with stored procedure parameter names ("" if not found)</param>
        /// <param name="args">populate with parameter variables</param>
        /// <returns>rewritten query</returns>
        private static string ParseQuery(string query, List<string> keys, List<string> args)
        {
            string newQuery = query;
            keys.Clear();
            args.Clear();

            //Replace string.Format() replacement field with SQL variable.
            if (newQuery.Contains("{0"))
            {
                var items = newQuery.Split(new char[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                var args2 = items[1].Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                newQuery = Regex.Replace(items[0], @"\{(?<INDEX>[0-9]+)[^\}]*\}", delegate(Match match)
                {
                    int index = -1;
                    int.TryParse(match.Groups["INDEX"].Value, out index);
                    return "@" + args2[index];
                }, RegexOptions.IgnoreCase);
            }

            //Extract SQL variables matching FormattedTraceEventCache variables.
            foreach (Match m in Regex.Matches(newQuery, @"(?:@(?<KEY>[a-z]+)\s*=\s*)?@(?<NAME>[a-z]+)", RegexOptions.IgnoreCase))
            {
                keys.Add(m.Groups["KEY"].Value);
                args.Add(m.Groups["NAME"].Value);
            }

            //Extract stored procedure, if it exists
            int i = newQuery.IndexOf(" ");
            int i1 = newQuery.IndexOf(" @");
            if (i != -1 && i == i1) newQuery = newQuery.Substring(0, i);

            return newQuery;
        }

        protected override object[] TraceMsgFormatter(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string message)
        {
            var args = new object[m_argFuncs.Count];
            Log.EventCache.SqueezeMsg = this.SqueezeMsg;
            for (int i = 0; i < m_argFuncs.Count; i++) args[i] = m_argFuncs[i](Log.EventCache, null);
            Log.EventCache.SqueezeMsg = false;
            return args;
        }

        #region class QWriter
        private class QWriter : QWriterBase<object[]>
        {
            SqlCommand m_logger = null;

            public QWriter(string listenerName, bool async, string connectionString, string sqlStatement, SqlParameter[] args) : base(listenerName, async, connectionString, sqlStatement, args) { }

            protected override void LoggerInit(object[] args)
            {
                string connectionString = (string)args[0];
                string sqlStatement = (string)args[1];
                SqlParameter[] parms = (SqlParameter[])args[2];

                SqlConnection conn = new SqlConnection(connectionString);
                m_logger = conn.CreateCommand();
                m_logger.CommandText = sqlStatement;
                m_logger.CommandType = sqlStatement.Contains(' ') ? CommandType.Text : CommandType.StoredProcedure;
                //m_logger.CommandTimeout = 30;
                m_logger.Parameters.AddRange(parms);
            }

            protected override void LoggerWrite(object[] args)
            {
                if (m_logger != null)
                {
                    try
                    {
                        if (m_logger.Connection.State == ConnectionState.Closed) m_logger.Connection.Open();
                        for (int i = 0; i < m_logger.Parameters.Count; i++)
                        {
                            m_logger.Parameters[i].Value = args[i];
                        }
                        m_logger.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Log.InternalError("Error writing {0} message. Terminating further writes.\r\n{1}", this.Name, ex);
                        LoggerClose();
                    }
                }
            }

            protected override void LoggerClose()
            {
                if (m_logger == null) return;
                m_logger.Connection.Dispose();
                m_logger.Dispose();
                m_logger = null;
            }
        }
        #endregion
    }
    #endregion DataBaseTraceListener

    #region TBD - ColoredConsoleTraceListener
    // TBD - Verbose=light grey, Info=green, Warning=yellow, Error=red, Critical=red bkgnd, white text
    #endregion ColoredConsoleTraceListener
    #region TBD - MsmqTraceListener
    // TBD
    #endregion MsmqTraceListener

    #region Custom Listener Filters
    //Built-in System.Diagnostics listener filters are
    //  System.Diagnostics.EventTypeFilter - TraceEvents >= SourceLevel
    //  System.Diagnostics.SourceFilter - Allow a single source

    /// <summary>
    /// Listener filter to block writing messages from this list of sources.
    /// The app.config attribute 'initializeData', contains a comma-delimited list of sources to ignore.
    /// </summary>
    public class MultiSourceFilter : TraceFilter
    {
        string[] Sources;

        /// <summary>
        /// Listener filter to block writing messages from this list of sources.
        /// </summary>
        /// <param name="initializeData">Comma-delimited list of sources to ignore</param>
        public MultiSourceFilter(string initializeData)
        {
            Sources = initializeData.Split(new char[]{',',' '},StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>Determines whether the trace listener should trace the event. </summary>
        /// <param name="cache">A <see cref="T:System.Diagnostics.TraceEventCache" /> that represents the information cache for the trace event. (unused)</param>
        /// <param name="source">The name of the source.</param>
        /// <param name="eventType">One of the <see cref="T:System.Diagnostics.TraceEventType" /> values. (unused)</param>
        /// <param name="id">A trace identifier number. (unused)</param>
        /// <param name="formatOrMessage">The format to use for writing an array of arguments, or a message to write. (unused)</param>
        /// <param name="args">An array of argument objects. (unused)</param>
        /// <param name="data1">A trace data object. (unused)</param>
        /// <param name="data">An array of trace data objects. (unused)</param>
        /// <returns>
        /// True if the trace should be produced; otherwise, False />.
        /// </returns>
        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType severity, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            return !Sources.Contains(source, StringComparer.InvariantCultureIgnoreCase);
        }
    }
    #endregion

    #endregion Custom Trace Listeners

    #region class FormatTraceEventCache
    /// <summary>
    /// Extended version of System.Diagnostics.TraceEventCache
    /// Properties are load-on-demand just as TraceEventCache, however, there are MANY 
    ///   more properties to pick from.
    /// Mainly used by TraceListenerBase custom string formatting.
    /// Really useful only when there are multiple listeners on a given event because the 
    ///   properties are initialized only once.
    /// The lifetime of this object is for a single event across multiple listeners.
    /// This object MUST be used immediately in the context of the logging event and not lazily 
    ///   in another thread as many of the properties are timing and context sensitive.
    /// </summary>
    public class FormatTraceEventCache
    {
        //Dictionary of property reader methods - Used only by DataBaseTraceListener
        public static readonly Dictionary<string, Func<FormatTraceEventCache, object[], object>> Properties = GetProperties();
        private static Dictionary<string, Func<FormatTraceEventCache, object[], object>> GetProperties()
        {
            var properties = new Dictionary<string, Func<FormatTraceEventCache, object[], object>>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var pi in typeof(FormatTraceEventCache).GetProperties())
            {
                properties.Add(pi.Name, pi.GetValue);
            }
            return properties;
        }

        private static readonly string FullLogTypeName = typeof(Log).FullName; //required for stripping our internals from the callstack string;

        // List all string properties that may possibly contain newlines or doublequote chars;
        // internal static readonly string[] CSVFormattingRequired = new string[]{ "UserMessage", "Exception", "ExceptionMessage", "CallStack", "LogicalOperationStack", "UserData" };

        #region Constructor Values
        private TraceEventCache cache = null;
        private readonly TraceEventType severity;
        private readonly string sourceName;
        private readonly ushort sourceId;
        private readonly string userMessage;
        private readonly Exception exception;
        private readonly object userData;
        #endregion

        private FormatTraceEventCache() { } //not used!
        internal FormatTraceEventCache(TraceEventCache cache, TraceEventType severity, string sourceName, ushort sourceId, string userMessage, Exception exception = null, object userData = null)
        {
            this.cache = cache; //may be null. If so it will be load-upon-demand.
            this.severity = severity;
            this.sourceName = sourceName ?? "Unknown";
            this.sourceId = sourceId;
            this.userMessage = userMessage ?? string.Empty;
            if (string.IsNullOrWhiteSpace(this.userMessage) && exception != null) this.userMessage = exception.Message;
            this.exception = exception==null && InternalHttpContext!=null ? InternalHttpContext.Error : exception;
            this.userData = userData;
            IndentPadding = string.Empty;
        }

        #region Internal Properties
        /// <summary>
        /// Internally used in case of invalid format argument in TraceListenerBase constructor.
        /// </summary>
        public string Null { get { return string.Empty; } }
        /// <summary>
        /// Get/Set bool to squeeze all whitespace (including newlines) from string properties.
        /// In particular, UserMessage, Exception, ExceptionMessage, CallStack, and UserData.
        /// All the other string properties consist of a squeezed single line by default.
        /// SqueezeMsg and IndentPadding are unique to each listener, thus must be set prior to using these properties.
        /// </summary>
        public bool SqueezeMsg { get; set; }
        /// <summary>
        /// Get/Set padded whitespace string prefixed with '\n' to indent multi-line string properties.
        /// In particular, UserMessage, Exception, ExceptionMessage, CallStack, and UserData.
        /// All the other string properties consist of a single line by default.
        /// SqueezeMsg and IndentPadding are unique to each listener, thus must be set prior to using these properties.
        /// </summary>
        public string IndentPadding { get; set; }
        /// <summary>
        /// Determines if the 'Exception' property has data.
        /// Used when cache properties are called explicitly and not thru the static 'Properties' dictionary. 
        /// </summary>
        public bool HasException { get { return this.exception != null; } }
        /// <summary>
        /// Provided to return the core TraceEventCache, because System.Diagnostics.TraceSource API do not know about FormatTraceEventCache.
        /// May also have to initialize this late within a listener as we may not yet have the TraceEventCache object initialized by System.Diagnostics.TraceSource.
        /// This will work fine even when late bound as properties are back-filled into the TraceEventCache core.
        /// </summary>
        public TraceEventCache Cache
        {
            //May have strip down because System.Diagnostics.TraceSource does not know about FormatTraceEventCache.
            get 
            {
                if (cache == null) cache = new TraceEventCache();
                return cache; 
            }
            set
            {
                 if (value == null) return; //do not allow to be null, ever.
                cache = value;
                //Backfill TraceEventCache to refer to *our* preexisting values.
                if (this.localdatetime != DateTime.MinValue)
                   typeof(TraceEventCache).GetField("dateTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cache,this.localdatetime.ToUniversalTime());
                if (exception == null) this.callStack = null;
                if (timeStamp != -1)
                    typeof(TraceEventCache).GetField("timeStamp", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(cache, this.timeStamp);
                if (processName != null)
                    typeof(TraceEventCache).GetField("processName", BindingFlags.NonPublic | BindingFlags.Static).SetValue(cache, processName);
                if (processId != -1)
                    typeof(TraceEventCache).GetField("processId", BindingFlags.NonPublic | BindingFlags.Static).SetValue(cache, processId);
            }
        }
        #endregion

        #region Public Properties used by TraceListenerBase string.Format()
        //Note: The following properties MUST be first executed in the context of the thread that they are being used.
        //   ActivityId, CallStack (if Exception object not provided), LogicalOperationStack, ThreadName, and ThreadId
        //All others will work fine on other threads as they do not need the current thread state.

        /// <summary>
        /// The formatted user log message for this event.
        /// </summary>
        public string UserMessage { get { return FormatMessage(userMessage); } }

        private string exceptionString = null;
        /// <summary>
        /// Gets the current exception or empty if there is no exception.
        /// </summary>
        public string Exception
        {
            get
            {
                if (exception == null) { exceptionString = string.Empty; return string.Empty; }
                if (exceptionString == null) exceptionString = "Exception:\r\n" + exception.ToString();
                return FormatMessage(exceptionString);
            }
        }

        /// <summary>
        /// Get the call stack or exception (if it exists) for Verbose log messages only. Returns empty if not verbose.
        /// </summary>
        public string ExceptionOrCallStack { get { return (severity < TraceEventType.Information ? (exception == null ? this.CallStack : this.Exception) : string.Empty); } }

        /// <summary>
        ///  Gets the message part of exception or empty if there is no exception.
        /// </summary>
        public string ExceptionMessage { get { return (exception == null ? string.Empty : FormatMessage(exception.Message)); } }

        /// <summary>
        /// Gets the severity level for this log event.
        /// </summary>
        public TraceEventType Severity { get { return severity; } }

        /// <summary>
        /// Gets the severity level for this log event.
        /// </summary>
        public string SeverityString { get { return severity.ToString(); } } //needed when NOT using string.Format!

        /// <summary>
        /// Gets the integer source ID
        /// </summary>
        public ushort SourceId { get { return sourceId; } }

        /// <summary>
        /// Gets the source name.
        /// </summary>
        public string SourceName { get { return sourceName; } }

        private DateTime localdatetime = DateTime.MinValue;
        /// <summary>
        /// Gets the local datetime this event was posted.
        /// </summary>
        public DateTime LocalDateTime 
        { 
            get 
            {
                if (localdatetime==DateTime.MinValue) localdatetime = Cache.DateTime.ToLocalTime();
                return localdatetime; 
            } 
        }
        /// <summary>
        /// Gets the UTC datetime this event was posted.
        /// </summary>
        public DateTime DateTime { get { return Cache.DateTime; } } //TraceEventCache DateTime is UTC.

        private static string domainName = null;
        /// <summary>
        /// Gets the AppDomain friendly name for this event.
        /// </summary>
        public string DomainName
        {
            get
            {
                if (domainName == null) domainName = AppDomain.CurrentDomain.FriendlyName;
                return domainName;
            }
        }

        private static string entryAssemblyName = null;
        /// <summary>
        /// Gets the assembly name for this event.
        /// </summary>
        public string EntryAssemblyName
        {
            get
            {
                if (entryAssemblyName == null) entryAssemblyName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                return entryAssemblyName;
            }
        }

        private Guid activityId = Guid.Empty;
        /// <summary>
        /// Gets the correlation activity id. 
        /// </summary>
        public Guid ActivityId 
        { 
            get 
            { 
                if (activityId==Guid.Empty) activityId = Trace.CorrelationManager.ActivityId;
                return activityId; 
            } 
        }

        private string callStack = null;
        /// <summary>
        /// Gets the call stack at the point of this event.
        /// </summary>
        public string CallStack
        {
            get
            {
                if (callStack == null)
                {
                    if (exception==null)
                    {
                        callStack = Cache.Callstack;
                        try
                        {
                            //Need to unwind stack trace to remove our private internal calls:
                            int i = callStack.LastIndexOf(FullLogTypeName);
                            i = callStack.IndexOf("   at ", i+1);
                            if (i > 0) callStack = callStack.Substring(i);
                        }
                        catch { }
                    }
                    else
                    {
                        callStack = exception.StackTrace;
                    }
                    callStack = "Stack Trace:\r\n" + callStack;
                }
                return FormatMessage(callStack);
            }
        }

        private string logicalOperationStack = null;
        /// <summary>
        /// Gets the entire correlated logical call stack form the call context.
        /// </summary>
        public string LogicalOperationStack
        {
            get
            {
                //Note: TraceEventCache returns this as type Stack. ToString() does not reformat this correctly when using string.Format().
                if (logicalOperationStack == null)
                {
                    StringBuilder sb = new StringBuilder();
                    string comma = string.Empty;
                    foreach (object obj in Trace.CorrelationManager.LogicalOperationStack)
                    {
                        sb.Append(comma);
                        sb.Append(obj);
                        comma = ", ";
                    }
                    logicalOperationStack = sb.ToString();
                }
                return logicalOperationStack;
            }
        }

        private static int processId = -1;
        /// <summary>
        /// Gets the unique identiier of the current process (PID)
        /// </summary>
        public int ProcessId 
        { 
            get 
            { 
                if (processId==-1) processId = Cache.ProcessId;
                return processId;
            } 
        }

        private static string processName = null;
        /// <summary>
        /// Gets the name of this process.
        /// </summary>
        public string ProcessName
        {
            get
            {
                if (processName == null) 
                {
                    //ProcessName exists in TraceEventCache but is not public. Go figure...
                    processName = typeof(TraceEventCache).GetMethod("GetProcessName", BindingFlags.NonPublic | BindingFlags.Static).Invoke(Cache, new object[0]) as string;
                }
                return processName;
            }
        }

        [ThreadStatic] private static int threadId = -1;
        /// <summary>
        /// Gets the current managed thread ID.
        /// </summary>
        public int ThreadId 
        {
            //Note: In TraceEventCache, this is a string! We leave it native to let the user's string format determine how to display it.
            get 
            { 
                if (threadId == -1) threadId = Thread.CurrentThread.ManagedThreadId;
                return threadId; 
            } 
        }

        [ThreadStatic] private static string threadName = null;
        /// <summary>
        /// Gets the current thread name or 'Thread ' + ThreadId if no thread name has been assigned.
        /// </summary>
        public string ThreadName
        {
            get
            {
                if (threadName == null) threadName = Thread.CurrentThread.Name ?? "Thread " + ThreadId;
                return threadName;
            }
        }

        private long timeStamp = -1;
        /// <summary>
        /// Gets the current number of ticks in the timer mechanism.
        /// </summary>
        public long Timestamp 
        { 
            get 
            {
                if (timeStamp == -1) timeStamp = Cache.Timestamp;
                return timeStamp;
            } 
        }

        private string userDataString = null;
        /// <summary>
        /// Custom data provided by the user. Object must have overridden ToString() else the string output will be just the class name.
        /// </summary>
        public string UserData 
        {
            get 
            {
                if (userDataString == null) userDataString = userData.ToString();
                return FormatMessage(userDataString);
            } 
        }

        public string version = null;
        /// <summary>
        /// Gets the version of the assembly that called this event.
        /// </summary>
        public string Version
        {
            get
            {
                if (version == null)
                {
                    version = Assembly.GetEntryAssembly().GetName().Version.ToString();
                }
                return version;
            }
        }

        #region static HttpContext.Current properties
        private static bool isHosted = System.Web.Hosting.HostingEnvironment.IsHosted;  //Official method for indicating whether an appdomain is configured to run under ASP.NET, but it requires that System.Web be loaded.
        //private static bool isHosted = (System.Web.HttpRuntime.AppDomainAppId != null); //Indiciate that the appdomain is currently running under ASP.NET. Requires that System.Web be loaded.
        //private static bool isHosted = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName).Equals("w3wp", StringComparison.InvariantCultureIgnoreCase); //"w3wp" for IIS 6.0, 7.0. "aspnet_wp" for IIS < 6.0. "?" for IIS >7.0 
        //private static bool isHosted = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile).Equals("web.config", StringComparison.InvariantCultureIgnoreCase); //hack, but usually true.
        private bool gotHttpContext = !isHosted;
        private HttpContext internalHttpContext = null;

        /// <summary>
        /// Gets the System.Web.HttpContext instance for the current HTTP request.
        /// </summary>
        private HttpContext InternalHttpContext
        {
            get
            {
                if (!gotHttpContext)
                {
                    internalHttpContext = HttpContext.Current; //HttpContext.Current may return null.
                    gotHttpContext = true;
                }
                return internalHttpContext;
            }
        }

        private string userName = null;
        /// <summary>
        /// Gets the current user name associated with this HttpContext.
        /// </summary>
        public string UserName
        {
            get
            {
                if (userName==null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.User != null && InternalHttpContext.User.Identity != null)
                        userName = InternalHttpContext.User.Identity.Name;
                    if (string.IsNullOrEmpty(userName)) userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    else userName = string.Empty;
                }
                return userName;
            }
        }

        private string userHostAddress = null;
        /// <summary>
        /// Gets IP address of remote client.
        /// </summary>
        public string UserHostAddress
        {
            get
            {
                if (userHostAddress == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null)
                        userHostAddress = InternalHttpContext.Request.UserHostAddress;
                    if (string.IsNullOrEmpty(userHostAddress))
                    {
                        userHostAddress = "127.0.0.1";
                        foreach(var hostIP in Dns.GetHostAddresses("127.0.0.1"))
                        {
                            if (hostIP.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) continue;
                            userHostAddress = hostIP.ToString();
                        }
                    }
                    else userHostAddress = string.Empty;
                }
                return userHostAddress;
            }
        }

        private string requestUrl = null;
        /// <summary>
        /// Gets the request Url.
        /// </summary>
        public string RequestUrl
        {
            get
            {
                if (requestUrl == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null)
                        requestUrl = InternalHttpContext.Request.Url.ToString();
                    else requestUrl = string.Empty;
                }
                return requestUrl;
            }
        }

        private string requestUserAgent = null;
        /// <summary>
        /// Gets the raw user agent string of the client browser.
        /// </summary>
        public string RequestUserAgent
        {
            get
            {
                if (requestUserAgent == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null)
                        requestUserAgent = InternalHttpContext.Request.UserAgent??string.Empty;
                    else requestUserAgent = string.Empty;
                }
                return requestUserAgent;
            }
        }

        private string requestUrlLocalPath = null;
        /// <summary>
        /// Gets the request url local file name.
        /// </summary>
        public string RequestUrlLocalPath
        {
            get
            {
                if (requestUrlLocalPath == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null)
                        requestUrlLocalPath = InternalHttpContext.Request.Url.LocalPath??string.Empty;
                    else requestUrlLocalPath = string.Empty;
                }
                return requestUrlLocalPath;
            }
        }

        private string requestHttpMethod = null;
        /// <summary>
        /// Gets the HTTP data transfer method (such as GET, POST, or HEAD) used by the client.
        /// </summary>
        public string RequestHttpMethod
        {
            get
            {
                if (requestHttpMethod == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null)
                        requestHttpMethod = InternalHttpContext.Request.HttpMethod??string.Empty;
                    else requestHttpMethod = string.Empty;
                }
                return requestHttpMethod;
            }
        }

        private string requestBrowserType = null;
        /// <summary>
        /// Gets the name and major (integer) version number of the browser.
        /// </summary>
        public string RequestBrowserType
        {
            get
            {
                if (requestBrowserType == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null && InternalHttpContext.Request.Browser != null)
                        requestBrowserType = InternalHttpContext.Request.Browser.Type??string.Empty;
                    else requestBrowserType = string.Empty;
                }
                return requestBrowserType;
            }
        }

        private string requestData = null;
        /// <summary>
        /// Get request data payload.
        /// </summary>
        public string RequestData
        {
            get
            {
                if (requestData == null)
                {
                    if (InternalHttpContext != null && InternalHttpContext.Request != null)
                        requestData = RequestHttpMethod.Equals("POST",StringComparison.InvariantCultureIgnoreCase) ? GetPostData(InternalHttpContext.Request) : InternalHttpContext.Request.QueryString.ToString();
                    else requestData = string.Empty;
                }
                return requestData;
            }
        }
        #endregion

        #endregion

        private static string GetPostData(HttpRequest request)
        {
            //do not log post data for login.
            if (request.Url.LocalPath.EndsWith("Account/Login")) return string.Empty;
            string postdata = string.Empty;
            request.InputStream.Position = 0;
            using (var reader = new StreamReader(request.InputStream))
            {
                postdata = reader.ReadToEnd();
            }
            return postdata;
        }

        /// <summary>
        /// Frequently used utility function to format multi-line strings according to 
        /// the dynamically set 'SqueezeMsg' and 'IndentPadding' properties.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string FormatMessage(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            if (SqueezeMsg) return s.Squeeze();
            if (IndentPadding.Length > 0) s = s.Replace("\n", IndentPadding);
            s = s.Trim();
            return s;
        }

        private string toString = null;
        public override string ToString()
        {
            if (toString==null) 
                toString = string.Format("FormatTraceEventCache({0},\"{1}\",{2},\"{3}\")",Severity,SourceName,SourceId,UserMessage.Squeeze());
            return toString;
        }
    }
    #endregion
}
