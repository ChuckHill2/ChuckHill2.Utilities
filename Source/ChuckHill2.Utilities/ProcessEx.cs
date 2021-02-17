using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ChuckHill2.Extensions;
using ChuckHill2.Logging;

namespace ChuckHill2
{
    /// <summary>
    /// Provides access to local and remote processes and enables you to start and stop local system processes. 
    /// </summary>
    public class ProcessEx
    {
        /// <summary>
        /// Extract this process's command-line parameters in the form of 
        /// "@key=value" and stuff them into a process-specific environment
        /// variable. All other command-line variables are ignored.
        /// This should be one of the first functions called in Program.Main().
        /// This is useful for simulating environment variables for the 
        /// duration of this application only.
        /// </summary>
        public static void SetCommandLineEnvironmentVariables()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg[0] != '@') continue;
                int index = arg.IndexOf('=');
                if (index < 1) continue;

                string key = arg.Substring(1, index - 1).Trim();
                string value = arg.Substring(index + 1).Trim();
                    
                //Is it a file with a list of command-line values?
                //Actual command-line may not be long enough!
                //"@CommandLine=filename" is a pseudo-environment variable.
                if (key.EqualsI("CommandLine") && !value.IsNullOrEmpty())
                {
                    #region [File Parser]
                    try
                    {
                        if (!File.Exists(value))
                        {
                            Log log = new Log("General");
                            log.Warning("Command-line argument file \"{0}\" not found.", value);
                            continue;
                        }
                        var sb = new StringBuilder();
                        string line = null;
                        //line = File.ReadAllText(value).Squeeze();
                        var stream = File.OpenText(value); //read file AND remove '#' comments;
                        while ((line = stream.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.Length == 0) continue;
                            if (line[0] == '#') continue;
                            int i = line.IndexOf('#'); //has a trailing comment?
                            if (i > -1) line = line.Substring(0, i).TrimEnd();
                            sb.Append(line);
                            sb.Append(' ');
                        }
                        line = sb.ToString(); 
                        sb.Length = 0;
                        
                        bool quoted = false;
                        string farg;
                        foreach (char c in line)
                        {
                            if (c == '"') { quoted = !quoted; continue; } //quotes cannot be escaped!
                            if (quoted) { sb.Append(c); continue; }
                            if (c == ' ')
                            {
                                if (sb.Length == 0) continue;
                                farg = sb.ToString();
                                sb.Length = 0;
                                if (farg[0] != '@') continue;
                                index = farg.IndexOf('=');
                                if (index < 1) continue;
                                key = farg.Substring(1, index - 1).Trim();
                                value = farg.Substring(index + 1).Trim();
                                if (value.IsNullOrEmpty()) value = null;
                                Environment.SetEnvironmentVariable(key, value);
                            }
                        }
                        if (sb.Length == 0) continue;
                        farg = sb.ToString();
                        sb.Length = 0;
                        if (farg[0] != '@') continue;
                        index = farg.IndexOf('=');
                        if (index < 1) continue;
                        key = arg.Substring(1, index - 1).Trim();
                        value = arg.Substring(index + 1).Trim();
                        if (value.IsNullOrEmpty()) value = null;
                        Environment.SetEnvironmentVariable(key, value);
                    } 
                    catch(Exception ex)
                    { 
                        Log log = new Log("General");
                        log.Warning(ex, "Error parsing command-line argument file \"{0}\".", value);
                    }
                    continue;
                    #endregion
                }

                if (value.IsNullOrEmpty()) value = null;
                Environment.SetEnvironmentVariable(key,value);
                //Now, how to erase this '@key' variable from the command-Line 
                //so it will be invisible to the rest of the application?
            }
        }

        /// <summary>
        /// Run an executable and wait for it to complete. Throws no exceptions. Any errors are written to 'sbStderr'.
        /// </summary>
        /// <param name="exe">Full path to executable or batch file</param>
        /// <param name="commandLine">Optional command line arguments. Note: args that contain whitespace MUST be quoted!</param>
        /// <param name="cwd">Optional startup working directory. If null, defaults to current working directory.</param>
        /// <param name="timeoutSeconds">Duration the executable is allowed to run before it is killed. If zero, do not wait for completion.</param>
        /// <param name="sbStdout">All console output is captured here. If null, no output is captured.</param>
        /// <param name="sbStderr">All console error output is captured here. If null, errors are redirected to 'sbStdout'.</param>
        /// <returns>True if successful (aka exit code==0)</returns>
        public static bool Exec(string exe, string commandLine, string cwd, int timeoutSeconds, StringBuilder sbStdout=null, StringBuilder sbStderr=null)
        {
            int exitCode = 0; //zero == success
            Process p = null;
            if (string.IsNullOrWhiteSpace(cwd)) cwd = Environment.CurrentDirectory;
            if (sbStderr == null) sbStderr = sbStdout;
            if (timeoutSeconds < 0) timeoutSeconds = 0;

            try
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.FileName = exe;
                if (!string.IsNullOrWhiteSpace(commandLine)) pi.Arguments = commandLine;
                pi.CreateNoWindow = true;
                pi.WindowStyle = ProcessWindowStyle.Hidden;
                pi.UseShellExecute = !Path.GetExtension(exe).EqualsI(".exe");
                pi.WorkingDirectory = cwd;

                p = new Process();
                p.StartInfo = pi;
                if (timeoutSeconds != 0 && sbStdout != null)  //0==nowait
                {
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;

                    p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (e.Data != null) sbStdout.AppendLine(e.Data.Trim()); };
                    p.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (e.Data != null) sbStderr.AppendLine(e.Data.Trim()); };
                }
                p.Start();

                if (sbStdout != null)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }

                if (timeoutSeconds != 0)
                {
                    if (p.WaitForExit(timeoutSeconds * 1000))
                        exitCode = p.ExitCode;
                    else
                    {
                        exitCode = -1;
                        p.Kill();
                        p.WaitForExit((timeoutSeconds / 2) * 1000);
                        sbStderr.AppendFormat("Error: {0} exceeded the specified time limit of {1} seconds. Exec terminated.\r\n", Path.GetFileName(exe), timeoutSeconds);
                    }
                }
            }
            catch (Exception ex) //error in execution of executable.
            {
                if (sbStderr != null) sbStderr.AppendLine($"Error: Exec(\"{exe}{(string.IsNullOrWhiteSpace(commandLine) ? "" : " " + commandLine)}\", \"{cwd}\", {(timeoutSeconds == 0 ? "Infinite" : timeoutSeconds.ToString())} seconds)\r\n{ex.ToString().Trim()}");
                return false;
            }
            finally
            {
                if (p != null)
                {
                    p.Close();
                    p.Dispose();
                    p = null;
                }
            }
            return (exitCode == 0);
        }

        /// <summary>
        /// Run an executable and DO NOT wait for it to complete. Throws no exceptions. Any errors are written to 'sbStderr'.
        /// </summary>
        /// <param name="exe">Full path to executable or batch file</param>
        /// <param name="commandLine">Optional command line arguments. Note: args that contain whitespace MUST be quoted!</param>
        /// <param name="cwd">Optional startup working directory. If null, defaults to current working directory.</param>
        /// <param name="sbStdout">All console output is captured here. If null, no output is captured.</param>
        /// <param name="sbStderr">All console error output is captured here. If null, errors are redirected to 'sbStdout'.</param>
        /// <returns>
        /// Running process object. Null if Exec() itself failed (see sbStderr for message).
        /// Use p.WaitForExit(ms) to wait for completion.
        /// Then use p.ExitCode for the process exit status..
        /// Be sure to Dispose the process object when no longer needed..
        /// </returns>
        public static Process Exec(string exe, string commandLine, string cwd, StringBuilder sbStdout = null, StringBuilder sbStderr = null)
        {
            Process p = null;
            if (string.IsNullOrWhiteSpace(cwd)) cwd = Environment.CurrentDirectory;
            if (sbStderr == null) sbStderr = sbStdout;

            try
            {
                ProcessStartInfo pi = new ProcessStartInfo();
                pi.FileName = exe;
                if (!string.IsNullOrWhiteSpace(commandLine)) pi.Arguments = commandLine;
                pi.CreateNoWindow = true;
                pi.WindowStyle = ProcessWindowStyle.Hidden;
                pi.UseShellExecute = !Path.GetExtension(exe).EqualsI(".exe");
                pi.WorkingDirectory = cwd;

                p = new Process();
                p.StartInfo = pi;
                if (sbStdout != null)
                {
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;

                    p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { if (e.Data != null) sbStdout.AppendLine(e.Data.Trim()); };
                    p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) { if (e.Data != null) sbStderr.AppendLine(e.Data.Trim()); };
                }

                p.Start();

                if (sbStdout != null)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }
            }
            catch (Exception ex) //error in execution of executable.
            {
                if (sbStderr != null) sbStderr.AppendLine($"Error: Exec(\"{exe}{(string.IsNullOrWhiteSpace(commandLine) ? "" : " " + commandLine)}\", \"{cwd}\")\r\n{ex.ToString().Trim()}");
                p = null;
            }

            return p;
        }

        #region public static Process GetParentProcess(int iCurrentPid=0)
        static uint TH32CS_SNAPPROCESS = 2;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        /// <summary>
        /// Get the parent process of the specified process. Useful to determine who started the current process.
        /// </summary>
        /// <param name="iCurrentPid">id of process to get parent of</param>
        /// <returns>Parent process or null if not found.</returns>
        public static Process GetParentProcess(int iCurrentPid=0)
        {
            int iParentPid = 0;
            if (iCurrentPid == 0) iCurrentPid = Process.GetCurrentProcess().Id;
            IntPtr oHnd = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (oHnd == IntPtr.Zero) return null;
            PROCESSENTRY32 oProcInfo = new PROCESSENTRY32();
            oProcInfo.dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(PROCESSENTRY32));
            if (Process32First(oHnd, ref oProcInfo) == false) return null;
            do
            {
                if (iCurrentPid == oProcInfo.th32ProcessID)
                    iParentPid = (int)oProcInfo.th32ParentProcessID;
            }
            while (iParentPid == 0 && Process32Next(oHnd, ref oProcInfo));
            if (iParentPid > 0) return Process.GetProcessById(iParentPid);
            else                return null;
        }
        #endregion

        /// <summary>
        /// Get full path to this running executble;
        /// </summary>
        public static string ExecutablePath
        {
            get
            {
                if (__executablePath == null) __executablePath = Process.GetCurrentProcess().MainModule.FileName;
                return __executablePath;
            }
        }
        private static string __executablePath;

        /// <summary>
        /// Get folder containing this running executable
        /// </summary>
        public static string ExecutableDir { get { return System.IO.Path.GetDirectoryName(ExecutablePath); } }

        #region public static bool IsExecutableAlreadyRunning()
        [DllImport("Kernel32.dll")] private static extern int GetCurrentProcessId();

        /// <summary>
        /// Detect if an instance of this executable is already running.
        /// </summary>
        /// <returns>True if an instance of this process is already running.</returns>
        public static bool IsExecutableAlreadyRunning()
        {
            int[] pids = null;
            string[] pnames = null;
            if (!ProcessList(ref pids, ref pnames)) return true;
            int currentPid = GetCurrentProcessId();
            string currentPName = pnames[Array.IndexOf(pids, currentPid)];
            int kount = 0;
            foreach (string pname in pnames)
            {
                if (pname == currentPName) kount++;
                if (kount > 1) return true;
            }
            return false;

            //On some machines, the following will throw the exception:
            //   System.InvalidOperationException: Process performance counter is disabled, so the requested operation cannot be performed.
            //return (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1);
        }

        /*---------------------------------------------------------------------------------------------------------------------------*
		On some machines the following exception may occur when just about any method in the 'System.Diagnostics.Process' class.

		  System.InvalidOperationException: Process performance counter is disabled, so the requested operation cannot be performed.

		Why does the Process class have a dependency on the performance counter?
		The Process class exposes performance information about processes. In order to get performance information about remote 
		processes, It needs to query performance information on a remote machine. It uses the same code to get performance information 
		about processes on a local machine. That's why the Process class has a dependency on the performance counter. However, this 
		approach has several problems: 

		(1) Performance information is not available to a non-admin account, which is not in the Performance Counter Users Group on 
		Windows Server 2003. So the Process class could not get process performance information in this case. 
		(2) Getting performance data from all the processes on the machine is pretty expensive. The operating system (OS) might 
		load lots of DLLs and it might take seconds to complete. The floppy drive light will be on when the OS tries to find the 
		index for some performance counter. 
		(3) If the performance counter data was corrupted for any reason, the Process class could throw an exception while trying 
		to convert some raw performance information into DateTime. 
		(4) The Process class could not be used to get process information on machines without the process performance counter. 
		Performance counters can be disabled in Windows.

		The good news is the Process class in Visual Studio 2005 (our next release, code-named Whidbey) has changed. The Process 
		class doesn't have a dependency on performance counter information any more (this is only true for local processes).

		The following is a HACK to fix the above problem by accessing the kernel system list directly (much like TaskManager).
		This method will work on all versions of all OS's except Win95 and WinME.  
		See https://www.geoffchappell.com/studies/windows/km/ntoskrnl/api/ex/sysinfo/query.htm
		*---------------------------------------------------------------------------------------------------------------------------*/
        private static int ReadInt32(int p, int byteoffset) { return Marshal.ReadInt32(new IntPtr(p + byteoffset)); }
        private static string PtrToStringUni(int p) { return Marshal.PtrToStringUni(new IntPtr(p)); }
        [DllImport("ntdll.dll",SetLastError = true)] private static extern UInt32 ZwQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int ReturnLength);
        const UInt32 STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
        const int SystemProcessesAndThreadsInformation = 5;

        private static bool ProcessList(ref int[] pids, ref string[] pnames)
        {
            System.Collections.ArrayList namelist = new System.Collections.ArrayList();
            System.Collections.ArrayList idlist = new System.Collections.ArrayList();
            IntPtr pBuffer = IntPtr.Zero;
            int pSysInfo;
            int cbBuffer = 65536;
            int nbytes = 0;

            try
            {
                while (true)  //we don't know how large a buffer we must supply, so we try larger and larger ones until it works.
                {
                    pBuffer = Marshal.AllocHGlobal(cbBuffer);
                    UInt32 status = ZwQuerySystemInformation(SystemProcessesAndThreadsInformation, pBuffer, cbBuffer, ref nbytes);
                    if (status == STATUS_INFO_LENGTH_MISMATCH)
                    {
                        Marshal.FreeHGlobal(pBuffer);
                        pBuffer = IntPtr.Zero;
                        cbBuffer *= 2;
                        continue;
                    }
                    if (status < 0) throw new Win32.Win32Exception("ZwQuerySystemInformation", "ZwQuerySystemInformation Failed.");
                    break;
                }
                pSysInfo = pBuffer.ToInt32();  //ptr arithmetic is a LOT easier when using integers
                while (true)  //step thru packed array of process entries
                {
                    int pid = ReadInt32(pSysInfo, 17 * 4);
                    string pname = PtrToStringUni(ReadInt32(pSysInfo, 15 * 4));
                    if (pname == null) pname = "Idle";  //special case: the idle process (pid==0) has no name
                    namelist.Add(pname);
                    idlist.Add(pid);
                    int NextEntryDelta = ReadInt32(pSysInfo, 0 * 4);
                    if (NextEntryDelta == 0) break; //no more entries
                    pSysInfo += NextEntryDelta;
                }
            }
            //catch (Exception ex) { Log.Local.Write(Log.Severity.Warning, "Error parsing ZwQuerySystemInformation returned process data", ex); return false; }
            finally
            {
                if (pBuffer != IntPtr.Zero) Marshal.FreeHGlobal(pBuffer);
                pids = idlist.ToArray(typeof(int)) as int[];
                pnames = namelist.ToArray(typeof(string)) as string[];
            }
            return true;
        }

        #endregion

        /// <summary>
        /// Get CPU Description
        /// </summary>
        /// <returns>CPU Description</returns>
        public static string GetCPUDescription()
        {
            Microsoft.Win32.RegistryKey key = null;
            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Hardware\\Description\\System\\CentralProcessor\\0", false);
                return key.GetValue("ProcessorNameString", string.Empty).ToString().Trim();
            }
            catch { return string.Empty; }
            finally { if (key != null) key.Close(); }
        }

        /// <summary>
        /// Get CPU speed in MHz
        /// </summary>
        /// <returns>CPU Speed in MHZ</returns>
        public static int GetCPUSpeed()
        {
            Microsoft.Win32.RegistryKey key = null;
            try
            {
                key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Hardware\\Description\\System\\CentralProcessor\\0", false);
                return (int)key.GetValue("~MHz", 0);
            }
            catch { return 0; }
            finally { if (key != null) key.Close(); }
        }

        #region public static int GetTotalMemory()
        struct MEMORYSTATUSEX
        {
            public Int32 dwLength;
            public UInt32 dwMemoryLoad;
            public UInt64 ullTotalPhys;
            public UInt64 ullAvailPhys;
            public UInt64 ullTotalPageFile;
            public UInt64 ullAvailPageFile;
            public UInt64 ullTotalVirtual;
            public UInt64 ullAvailVirtual;
            public UInt64 ullAvailExtendedVirtual;
        }
        [DllImport("kernel32.dll")] private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        /// <summary>
        /// Get snapshot of total system memory used in MB
        /// </summary>
        /// <returns>Total system memory used in MB</returns>
        public static int GetTotalMemory() //in Mb
        {
            MEMORYSTATUSEX ms = new MEMORYSTATUSEX();
            ms.dwLength = Marshal.SizeOf(ms);
            if (!GlobalMemoryStatusEx(ref ms)) return 0;
            return Convert.ToInt32(ms.ullTotalPhys / 1048576);
        }
        #endregion == GetTotalMemory ==
    }
}
