using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ChuckHill2.Utilities.Extensions;

namespace ChuckHill2.Utilities
{
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
        public static bool Exec(string exe, string commandLine=null, string cwd=null, int timeoutSeconds=0, StringBuilder sbStdout=null, StringBuilder sbStderr=null)
        {
            int exitCode = 0; //zero == success
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
                if (timeoutSeconds != 0 && sbStdout != null)  //0==nowait
                {
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;

                    p.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (e.Data != null) sbStdout.AppendLine(e.Data.Trim()); };
                    p.ErrorDataReceived += delegate(object sender, DataReceivedEventArgs e) { if (e.Data != null) sbStderr.AppendLine(e.Data.Trim()); };
                }
                p.Start();
                if (timeoutSeconds != 0)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

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
                if (sbStderr!=null) sbStderr.AppendFormat("Error: Exec(\"{0}\", \"{1}\", \"{2}\", {3})\r\n{4}\r\n", exe, commandLine, cwd, timeoutSeconds, ex.ToString().Trim());
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

        #region GetParentProcess - Win32
        static uint TH32CS_SNAPPROCESS = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESSENTRY32
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
        #endregion

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
    }
}
