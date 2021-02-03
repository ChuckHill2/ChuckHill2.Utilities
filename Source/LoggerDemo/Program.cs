#define TRACE
#define DEBUG

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChuckHill2;
using ChuckHill2.Extensions.Reflection;
using ChuckHill2.Extensions;
using ChuckHill2.Logging;

namespace LoggerDemo
{
    class Program
    {
        private static int timerStart = 0;
        private static int testNumber = -1;

        [STAThread]
        static void Main(string[] args)
        {
            Log.LogInitialize(); //This must be first line so Log will be inialized and recognize and process Debug/Trace API before using the first use.
            Thread.CurrentThread.Name = "MainThread";
            Log log;

            if (args.Length > 0)
            {
                if (args[0] == "SPEEDTEST") { SpeedTestChild(); return; }
                Int32.TryParse(args[0], out testNumber);
                if (testNumber == -1) Usage("Error: First arg (test#) must be an integer.");
            }

            Console.WriteLine($@"
Review the debugger output window for 'TRACE' messages and TestCase #3 messages.
Review CSV output in {Path.ChangeExtension(Process.GetCurrentProcess().MainModule.FileName, ".log")} for 'General' logging messages.");

            //Debug.Fail("Debug.Fail message");   //This will create an assert popup message
            Debug.WriteLine("Debug.WriteLine");
            Debug.WriteLine("Debug.WriteLine-MySource", "MySource");
            Debug.Indent();
            Debug.WriteLine("Indent;Debug.WriteLine;Unindent");
            Debug.Unindent();

            foreach (TraceListener tl in Trace.Listeners)  Console.WriteLine($"TraceListener=\"{tl.Name}\" ({tl.GetType().Name})");
            //Trace.Fail("Trace.Fail");   //This will create an assert popup message
            Trace.TraceError("Trace.TraceError");
            Trace.TraceWarning("Trace.TraceWarning");
            Trace.TraceInformation("Trace.TraceInformation");
            Trace.Write("Trace.Write");  //these will run together as there is no newline
            Trace.Write("Trace.Write");
            Trace.WriteLine("Trace.WriteLine");
            Trace.WriteLine("Trace.WriteLine-MySource", "A message prefix");

            log = new Log("MyEvSource");
            log.Information("LoggerDemo Test");

            //Create an exception so we can capture the first chance exception.
            int d = 1234;
            try { d /= 0; }
            catch { }


            log = new Log("General");
            log.Error("log.Information-General");
            Log.SetAllSeverities(SourceLevels.All);
            log.Information("log.Information-General-All");

            Trace.WriteLine(string.Format("[Instance#{0}, Thread#{1}] ({2:000000}) log.Information(\"This is an Information message\")", 1, 2, 3));
            Trace.WriteLine(string.Format("[Instance#{0}, Thread#{1}] ({2:000000}) log.Information(\"This is an Information message\")", 1, 2, 3));
            Trace.WriteLine(string.Format("[Instance#{0}, Thread#{1}] ({2:000000}) log.Information(\"This is an Information message\")", 1, 2, 3));

            if (testNumber > 0) { TestCases(testNumber); return; }
            while((testNumber = Prompt())!=0)
            {
                TestCases(testNumber);            
            }
        }

        private static void Usage(string fmt, params object[] args)
        {
            if (args.Length > 0) fmt = string.Format(fmt, args);
            Console.WriteLine();
            Console.WriteLine(fmt);
            Console.WriteLine();
            Console.WriteLine("Usage: {0} [test#]", Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName));
            Console.WriteLine("If there are no arguments, this test harness is interactive.");
            Console.WriteLine();
            Environment.Exit(1);
        }

        private static void TestCases(int testCase)
        {
            if (testCase == 1) { SpeedTest(); return; }
            if (testCase == 2) { RedirectorTest(); return; }
            if (testCase == 3) 
            {
                Log log = new Log("Boogers");
                log.Information("BoogerTest #1");
                Log.SetSeverity("Boogers", SourceLevels.Information);
                log.Information("BoogerTest #2");
                return; 
            }
        }

        private static int Prompt()
        {
            int testnumber = -1;
            string answer = "0";
            do
            {
                //Console.Clear();
                Console.WriteLine();
                Console.WriteLine("Available tests:");
                Console.WriteLine("   0. Exit this test harness.");
                Console.WriteLine("   1. Speed Test");
                Console.WriteLine("   2. Redirector Test");
                Console.WriteLine("   3. Change severity for undefined source");
                Console.WriteLine();
                Console.Write("Select test to run: [0] ");
                answer = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(answer)) answer = "0";
            } while (!int.TryParse(answer, out testnumber));
            return testnumber;
        }

        #region Test#1 SpeedTest
        private static void SpeedTest()
        {
            Console.WriteLine("\r\nRunning 3 processes, 3 threads in each process, each thread writing 100,000 messages to LoggerDemo.log\r\n");

            timerStart = Environment.TickCount;
            var sb = new StringBuilder();
            var p1 = ProcessEx.Exec(Process.GetCurrentProcess().MainModule.FileName, "SPEEDTEST", null, sb);
            var p2 = ProcessEx.Exec(Process.GetCurrentProcess().MainModule.FileName, "SPEEDTEST", null, sb);
            var p3 = ProcessEx.Exec(Process.GetCurrentProcess().MainModule.FileName, "SPEEDTEST", null, sb);

            p1.WaitForExit();
            p2.WaitForExit();
            p3.WaitForExit();
            p1.Dispose();
            p2.Dispose();
            p3.Dispose();

            Console.WriteLine(sb.ToString());
            Console.WriteLine("Grand Total Duration={0:0.000} seconds", (Environment.TickCount - timerStart) / 1000.0);
        }
        private static void SpeedTestChild()
        {
            AppDomain.CurrentDomain.ProcessExit += delegate (object sender, EventArgs e) { Console.WriteLine("[PID#{0}] Total Synchronous Duration={1:0.000} seconds", Process.GetCurrentProcess().Id, (Environment.TickCount - timerStart) / 1000.0); };
            int pid = Process.GetCurrentProcess().Id;

            Log.LogInitialize();
            timerStart = Environment.TickCount;

            Thread th1 = new Thread(ThreadSpeedTest);
            th1.IsBackground = true;
            th1.Name = "Thread1";

            Thread th2 = new Thread(ThreadSpeedTest);
            th2.IsBackground = true;
            th2.Name = "Thread2";

            Thread th3 = new Thread(ThreadSpeedTest);
            th3.IsBackground = true;
            th3.Name = "Thread3";

            th1.Start(new object[] { pid, 1 });
            th2.Start(new object[] { pid, 2 });
            th3.Start(new object[] { pid, 3 });

            if (th1.IsAlive) th1.Join();
            if (th2.IsAlive) th2.Join();
            if (th3.IsAlive) th3.Join();

            Console.WriteLine("[PID#{0}] Total Synchronous Duration={1:0.000} seconds", pid, (Environment.TickCount - timerStart) / 1000.0);
        }

        private static void ThreadSpeedTest(Object obj)
        {
            object[] args = (object[])obj;
            int processInstance = (int)args[0];
            int threadInstance = (int)args[1];
            Log log = new Log("General");
            int t1 = Environment.TickCount;
            for (int i = 0; i < 100000; i++)
                //Trace.WriteLine(string.Format("[Instance#{0}, Thread#{1}] ({2:000000}) log.Information(\"This is an Information message\")", processInstance, threadInstance, i));
                log.Information("[PID#{0}, Thread#{1}] ({2:000000}) log.Information(\"This is an Information message\")", processInstance, threadInstance, i);

            Console.WriteLine("[PID#{0}, Thread#{1}] Duration={2:0.000} seconds", processInstance, threadInstance, (Environment.TickCount - t1) / 1000.0);
        }
        #endregion

        #region Test#2 RedirectorTest
        private static void RedirectorTest()
        {
            Log log = new Log("General");

            Log.RedirectorWriter += Log_RedirectorWriter;
            Console.WriteLine("Should see messages on the console window.");
            for(int i=1; i<=3; i++) log.Information("RedirectorTest #{0}", i);
            Thread.Sleep(100); //writing is asynchronous so we need to wait before moving on. otherwise the logging messages and these writeline messages will not be synchronous.
            Console.WriteLine("Should NOT see messages on the console window.");
            Log.RedirectorWriter -= Log_RedirectorWriter;
            Thread.Sleep(100);
            for (int i = 4; i <= 6; i++) log.Information("RedirectorTest #{0}", i);
            Console.WriteLine("Should see messages again on the console window.");
            Log.RedirectorWriter += Log_RedirectorWriter;
            for (int i = 7; i <= 9; i++) log.Information("RedirectorTest #{0}", i);
            Thread.Sleep(100);
            Console.WriteLine("Test Complete.");
        }

        private static void Log_RedirectorWriter(FormatTraceEventCache cache)
        {
            Console.WriteLine("Redirector: Severity={0}, Source={1}, Id={2}, Msg={3}", cache.Severity, cache.SourceName, cache.SourceId, cache.UserMessage);
        }
        #endregion
    }
}
