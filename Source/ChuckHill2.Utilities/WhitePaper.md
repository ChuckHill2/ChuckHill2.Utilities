# Diagnostics White Paper

# Problem

When attempting to diagnose a problem, standard logging and and even the debugger may not be enough. The official logging is too high-level and the debugger too low-level and is limited in scope (only a single process) and the debugger's performance may also effect the output (e.g. multi-threading).

Because of the detailed nature of this mechanism, the information provided by this diagnostic tool is usually not useful to the customer or even the field service support crew. However it may be critical to the developer to identify the root cause of a problem.

In all cases, this logging should be lightweight not hurt performance, lest it change the result.

System.Diagnostics.Debug methods perform a lot (but not all) of the same functionality but is only available in DEBUG builds. This can be a real problem when diagnosing problems with inter-process, inter-thread communications.

# Solution

Create a lightweight utility class that is capable of writing debug information like System.Diagnostics.Debug but is available in RELEASE, can be enabled/disabled at runtime, uses native Win32 output API, and has a minimal performance hit and memory footprint.

# Usage

There are 2 major parts to using this tool.

1. Using this to write the debug message
2. Use Visual Studio debugger or other 3rd-party debug listener tool to read the messages.

## Using ChuckHill2.Diagnostics

For the target project, add a reference to the ChuckHill2.Diagnostics assembly/project

For convenience in the .cs file, add a using reference (e.g. using ChuckHill2.Diagnostics;)

### static void DBG.Writeline(string msg, arg0, arg1, …);

This is the principal method of this utility. It sends a string to the debug listener for display. If there is no debug listener listening for messages, this method does nothing and does not impact performance. See below how to capture these debug message events. The message may be a formatted string. It's usage is identical to string.Format(). Like string.Format(), this method has a variable argument list.

### static string DBG.CallStack(int depth);

Returns a semi-colon delimited list of method names in reverse order from any place in the code. Does not require an exception to occur. This is useful if the method this is used in has many different callers.

The *depth* arg determines how far up the call stack to unwind. CallStack() may be used as part of the DBG.Writeline() format string.

### Property static bool DBG.Enabled;

For the entire application, globally enables/disables the above 2 methods. When disabled, DBG.Writeline() does nothing and returns immediately. When disabled, DBG.CallStack() does nothing and immediately returns string.Empty. The DBG.Enabled default value is TRUE.

### PerfTimer Class

Lightweight Performance Timer. Returns elapsed real time in milliseconds. Similar to System.Diagnostics.Stopwatch().

**Properties:**

UInt32 Count; Return the current elapsed time in milliseconds. Does not stop the timer.

**Methods:**

PerfTimer(); Constructor. Creates and starts the timer.

Restart(); Reset the timer to zero, but continue timing.

Stop(); Stop the timer and reset to zero.

Start(); Starts the timer count from zero. Differs from Restart() in that if not previously stopped or paused, it does not reset the elapsed time to zero.

Pause(); Temporarily stop the timer but keep the elapsed time

Continue(); Resume the timer

Print(msg); Print message appended with the elapsed time in milliseconds. (uses DBG.Writeline)

## Reading the Debug Messages

The following applications let you monitor/listen for debug output on your local system. They capture the debug output from DBG.Writeline(), System.Diagnostics.Debug methods, and native Win32 OutputDebugString() (both previous methods use this low-level Win32 function), so you don't need a real debugger to capture the debug output the applications generate, nor do you need to modify your applications to use non-standard debug output APIs. As such the following applications are generic and work for all applications running on Microsoft Windows.

**In-house DBMon.exe**

Lightweight console application (16KB). Extremely fast, low latency. Works on local system only.

Console Command-line Arguments

Usage: DBMon.exe [/?] [[/i] /f pid [/f pid] ...] [logfile]

/? : Print this message.

/f : filter out specified process ID. May be specified multiple times.

/i : reverse the default filter operation to filter IN instead of OUT.

logfile : Optional name of log file to write output to. The messages are still printed to screen.

Runtime Console Keyboard Functionality

\<ES\> to exit

\<F1\> to display help

\<F2\> to clear the screen (if writing to a logfile a line is written to the file)

\<F3\> to toggle msg filter IN / filter OUT state

\<F4\> to add last displayed processID to msg filter list (see \<F3\>).

**Microsoft/SysInternals DbgView.exe**

[http://technet.microsoft.com/en-us/sysinternals/bb896647](http://technet.microsoft.com/en-us/sysinternals/bb896647)

Full-featured standalone windows application (286KB). Not nearly as fast, high latency. Can capture debug output from multiple remote computers as well as the local computer. See the website for additional information.

## Final Notes

Only one listener application may receive messages. If the app is running from the Visual Studio debugger, only the Visual Studio output window will receive the messages from the app.

Internally, the debug messages are sent/received on a system-wide Win32 named pipe. When a listener application is listening for debug messages and does not respond, all applications writing the debug messages will hang for up to 10 seconds before timing out, for each debug message written. To mitigate this, DBMon was written to be as efficient as possible and it will quietly attempt to set its own process and thread priority higher than normal in order to make sure debug messages are acknowledged as soon as possible (Run DBMon with Administrator privileges!). The only flaw with DBMon is if the \<Pause\> key is hit to temporarily disable console scrolling, incoming debug messages will not acknowledged, causing all applications writing to the debug API to hang. This is a good reason to set the ChuckHill.Diagnostics.DBG.Enabled flag to false during normal operations in our applications.

Another subtle issue is since there is only one listener application listening for all debug messages on the system, processes/threads will tend to serialize if all are writing debug messages simultaneously. This may hide or cause multi-threaded race or deadlock problems. Another reason to set DBG.Enabled flag to false during normal operations.

All in all, one must make judicious use of these debug message API and not use them in place of a real debugger.
