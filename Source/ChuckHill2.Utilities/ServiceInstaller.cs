using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ChuckHill2.Utilities
{
    //This source file is linked to many projects so we cannot use any 
    //other outside utility functions except for Win32Exception.cs
    public class WinService : IDisposable
    {
        #region -= Win32 =-
        [Flags] public enum ServiceManagerRights
        {
            Connect = 0x0001,
            CreateService = 0x0002,
            EnumerateService = 0x0004,
            Lock = 0x0008,
            QueryLockStatus = 0x0010,
            ModifyBootConfig = 0x0020,
            StandardRightsRequired = 0xF0000,
            AllAccess = (StandardRightsRequired | Connect | CreateService | EnumerateService | Lock | QueryLockStatus | ModifyBootConfig)
        }
        [Flags] public enum ServiceRights
        {
            QueryConfig = 0x1,
            ChangeConfig = 0x2,
            QueryStatus = 0x4,
            EnumerateDependants = 0x8,
            Start = 0x10,
            Stop = 0x20,
            PauseContinue = 0x40,
            Interrogate = 0x80,
            UserDefinedControl = 0x100,
            Delete = 0x00010000,
            StandardRightsRequired = 0xF0000,
            AllAccess = (StandardRightsRequired | QueryConfig | ChangeConfig | QueryStatus | EnumerateDependants | Start | Stop | PauseContinue | Interrogate | UserDefinedControl)
        }
        public enum ServiceBootFlag
        {
            Start = 0x00000000,
            SystemStart = 0x00000001,
            AutoStart = 0x00000002,
            DemandStart = 0x00000003,
            Disabled = 0x00000004,
            DelayedAutoStart = 0x0000FFFF
        }
        public enum ServiceState
        {
            Unknown = -1, //Service state cannot be (has not been) retrieved.
            NotFound = 0, //Unknown service.
            Stopped = 1,  //Service process has exited.
            Starting = 2, //Service startup is in progress.
            Stopping = 3, //Service stopping is in progress
            Running = 4,  //Service has completed startup and is running
            Resuming = 5, //Service is resuming from a paused state.
            Pausing = 6,  //Service is attempting to pause execution.
            Paused = 7    //Service is active but has stopped execution.
        }
        [Flags] enum ServiceType : int
        {
            UNKNOWN = 0x00000000,
            KernelDriver = 0x00000001,
            FileSystemDriver = 0x00000002,
            Win32OwnProcess = 0x00000010,
            Win32ShareProcess = 0x00000020,
            InteractiveProcess = 0x00000100
        }
        public enum ServiceControl
        {
            Stop = 0x00000001,
            Pause = 0x00000002,
            Continue = 0x00000003,
            Interrogate = 0x00000004,
            Shutdown = 0x00000005,
            ParamChange = 0x00000006,
            NetBindAdd = 0x00000007,
            NetBindRemove = 0x00000008,
            NetBindEnable = 0x00000009,
            NetBindDisable = 0x0000000A
        }
        public enum ServiceError
        {
            Ignore = 0x00000000,
            Normal = 0x00000001,
            Severe = 0x00000002,
            Critical = 0x00000003
        }
        [Flags] public enum ServiceControlsAccepted
        {
            UNKNOWN = 0x00000000,
            Stop = 0x00000001,
            PauseContinue = 0x00000002,
            Shutdown = 0x00000004,
            ParamChange = 0x00000008,
            NetBindChange = 0x00000010,
            HardwareProfileChange = 0x00000020,
            PowerEvent = 0x00000040,
            SessionChange = 0x00000080,
            PreShutdown = 0x00000100,
            TimeChange = 0x00000200,
            TriggerEvent = 0x00000400
        }
        private const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        private const int SERVICE_NO_CHANGE = -1;
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(SERVICE_STATUS));
            public ServiceType dwServiceType;
            public ServiceState dwCurrentState;
            public ServiceControlsAccepted dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
        }

        [DllImport("advapi32.dll", SetLastError = true)] private static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, ServiceManagerRights dwDesiredAccess);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceRights dwDesiredAccess);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern IntPtr CreateService(IntPtr hSCManager, string lpServiceName, string lpDisplayName, ServiceRights dwDesiredAccess, int dwServiceType, ServiceBootFlag dwStartType, ServiceError dwErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lp, string lpPassword);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern int CloseServiceHandle(IntPtr hSCObject);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern int QueryServiceStatus(IntPtr hService, out SERVICE_STATUS lpServiceStatus);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern int DeleteService(IntPtr hService);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern int ControlService(IntPtr hService, ServiceControl dwControl, ref SERVICE_STATUS lpServiceStatus);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern int StartService(IntPtr hService, int dwNumServiceArgs, int lpServiceArgVectors);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool ChangeServiceConfig2(IntPtr hService, SERVICE_CONFIG infoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DELAYED_AUTO_START delayedAutoStart);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool ChangeServiceConfig2(IntPtr hService, SERVICE_CONFIG infoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_DESCRIPTION serviceDescription);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool ChangeServiceConfig2(IntPtr hService, SERVICE_CONFIG infoLevel, [MarshalAs(UnmanagedType.Struct)] ref SERVICE_FAILURE_ACTIONS failureActions);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool QueryServiceConfig2(IntPtr hService, SERVICE_CONFIG infoLevel, [MarshalAs(UnmanagedType.Struct)] out SERVICE_DELAYED_AUTO_START delayedAutoStart, int cbBufSize, out int pcbBytesNeeded);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool QueryServiceConfig(IntPtr hService, IntPtr hServiceConfig, int cbBufSize, out int pcbBytesNeeded);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern bool ChangeServiceConfig(IntPtr hService, ServiceType serviceType, ServiceBootFlag startType, ServiceError errorControl, [MarshalAs(UnmanagedType.LPTStr)] string binaryPathName, [MarshalAs(UnmanagedType.LPTStr)] string loadOrderGroup, int tagID, [MarshalAs(UnmanagedType.LPTStr)] string dependencies, [MarshalAs(UnmanagedType.LPTStr)] string startName, [MarshalAs(UnmanagedType.LPTStr)] string password, [MarshalAs(UnmanagedType.LPTStr)] string displayName);

        public enum SERVICE_CONFIG : int { DESCRIPTION = 1, FAILURE_ACTIONS = 2, DELAYED_AUTOSTART = 3 }
        [StructLayout(LayoutKind.Sequential)] public struct SERVICE_DELAYED_AUTO_START { public bool fDelayedAutostart; }
        [StructLayout(LayoutKind.Sequential)] public struct SERVICE_DESCRIPTION { public string lpDescription; }
        [StructLayout(LayoutKind.Sequential)]
        private struct QUERY_SERVICE_CONFIG
        {
            public int dwServiceType;
            public int dwStartType;
            public int dwErrorControl;
            public string lpBinaryPathName;
            public string lpLoadOrderGroup;
            public int dwTagId;
            public string lpDependencies;
            public string lpServiceStartName;
            public string lpDisplayName;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_FAILURE_ACTIONS
        {
            public int    dwResetPeriod; //seconds
            public string lpRebootMsg;
            public string lpCommand;
            //Automatically marshalling an array of structs within a struct doesn't work, so we have to do it ourselves.
            private int    cActions;
            private IntPtr lpsaActions;
            //[MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeParamIndex = 3)]
            //public SC_ACTION[] lpsaActions;

            [DllImport("kernel32.dll")] private static extern void CopyMemory(IntPtr pDst, SC_ACTION[] pSrc, int ByteLen);
            public void CopyToStruct(SC_ACTION[] pSrc) //needed for initializing actions in this struct.
            {
                Dispose();
                int len = Marshal.SizeOf(new SC_ACTION()) * pSrc.Length;
                lpsaActions = Marshal.AllocHGlobal(len);
                CopyMemory(lpsaActions, pSrc, len);
                cActions = pSrc.Length;
            }
            public void Dispose() //needed for memory cleanup created by CopyToStruct()
            {
                if (lpsaActions != IntPtr.Zero) Marshal.FreeHGlobal(lpsaActions);
                lpsaActions = IntPtr.Zero;
                cActions = 0;
            }
            #region DEBUGGING
            public SC_ACTION[] CopyFromStruct() //for debugging only. see QueryServiceConfig2()
            {
                int len = Marshal.SizeOf(new SC_ACTION());
                SC_ACTION[] pSrc = new SC_ACTION[cActions];
                for (int i = 0; i < cActions; i++)
                {
                    pSrc[i] = (SC_ACTION)Marshal.PtrToStructure(Increment(lpsaActions, len * i), typeof(SC_ACTION));
                }
                return pSrc;
            }
            private static IntPtr Increment(IntPtr pointer, Int32 value)
            {
                unchecked
                {
                    switch (IntPtr.Size)
                    {
                        case sizeof(Int32): return (new IntPtr(pointer.ToInt32() + value));
                        default:            return (new IntPtr(pointer.ToInt64() + value));
                    }
                }
            }

            [DllImport("advapi32.dll", SetLastError = true)] private static extern bool QueryServiceConfig2(IntPtr hService, SERVICE_CONFIG infoLevel, IntPtr lpBuffer, int cbBufSize, out int pcbBytesNeeded);
            public static bool GetActions(string ServiceName, out SERVICE_FAILURE_ACTIONS act, out SC_ACTION[] actions)
            {
                IntPtr hService = IntPtr.Zero;
                IntPtr scman = IntPtr.Zero;
                IntPtr service = IntPtr.Zero;
                act = new SERVICE_FAILURE_ACTIONS();
                actions = new SC_ACTION[0];

                try
                {
                    scman = OpenSCManager(ServiceManagerRights.Connect);
                    if (scman == IntPtr.Zero) return false;
                    hService = OpenService(scman, ServiceName, ServiceRights.StandardRightsRequired | ServiceRights.QueryConfig);
                    if (hService == IntPtr.Zero) return false;

                    IntPtr lpBuffer = Marshal.AllocHGlobal(1024);
                    int pcbBytesNeeded = 0;
                    try
                    {
                        bool ok = QueryServiceConfig2(hService, SERVICE_CONFIG.FAILURE_ACTIONS, lpBuffer, 1024, out pcbBytesNeeded);
                        if (!ok) return false;
                        act = (SERVICE_FAILURE_ACTIONS)Marshal.PtrToStructure(lpBuffer, typeof(SERVICE_FAILURE_ACTIONS));
                        actions = act.CopyFromStruct();
                    }
                    finally
                    {
                        if (lpBuffer != IntPtr.Zero) Marshal.FreeHGlobal(lpBuffer);
                    }
                    return true;
                }
                finally
                {
                    if (hService != IntPtr.Zero) CloseServiceHandle(hService);
                    if (scman != IntPtr.Zero) CloseServiceHandle(scman);
                }
            }
            #endregion
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct SC_ACTION
        {
            public SC_ACTION_TYPE Type;
            public int            Delay; //ms
        }
        private enum SC_ACTION_TYPE : int
        {
            NONE = 0, //do nothing. stay dead.
            RESTART = 1, //restart the service
            REBOOT = 2, //reboot the computer
            RUN_COMMAND = 3 //execute program
        }

        #region DEBUGGING
        //Validate Service Failure Actions
        public static void GetActions(string ServiceName)
        {
            SERVICE_FAILURE_ACTIONS act;
            SC_ACTION[] actions;
            bool ok = SERVICE_FAILURE_ACTIONS.GetActions(ServiceName, out act, out actions);
            if (!ok) return;
        }
        #endregion

        #region -= Non-polling Service status; Windows 2008 Server, Vista, Win7, and up ONLY =-
        //Requires some additional special asynchronous handling on our part
        //See: http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/f68fb826-036a-4b9c-81e6-4cbd87931feb
        //See: http://msdn.microsoft.com/en-us/library/ms684276%28VS.85%29.aspx
        private enum ServiceFlags { NotSystemProcessOrNotRunning = 0, SystemProcess = 1 }
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS_PROCESS
        {
            public ServiceType dwServiceType;
            public ServiceState dwCurrentState;
            public ServiceControlsAccepted dwControlsAccepted;
            public uint dwWin32ExitCode;
            public uint dwServiceSpecificExitCode;
            public uint dwCheckPoint;
            public uint dwWaitHint;
            public uint dwProcessId;
            public uint dwServiceFlags;
        }
        [Flags] public enum ServiceNotify
        {
            /// <summary>
            /// Report when the service has been created.
            /// The hService parameter must be a handle to the SCM.
            /// </summary>
            SERVICE_NOTIFY_CREATED = 0x00000080,
            /// <summary>
            /// Report when the service is about to continue.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_CONTINUE_PENDING = 0x00000010,
            /// <summary>
            /// Report when an application has specified the service in a call to the DeleteService function. Your application should close any handles to the service so it can be deleted.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_DELETE_PENDING = 0x00000200,
            /// <summary>
            /// Report when the service has been deleted. An application cannot receive this notification if it has an open handle to the service.
            /// The hService parameter must be a handle to the SCM.
            /// </summary>
            SERVICE_NOTIFY_DELETED = 0x00000100,
            /// <summary>
            /// Report when the service is pausing.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_PAUSE_PENDING = 0x00000020,
            /// <summary>
            /// Report when the service has paused.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_PAUSED = 0x00000040,
            /// <summary>
            /// Report when the service is running.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_RUNNING = 0x00000008,
            /// <summary>
            /// Report when the service is starting.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_START_PENDING = 0x00000002,
            /// <summary>
            /// Report when the service is stopping.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_STOP_PENDING = 0x00000004,
            /// <summary>
            /// Report when the service has stopped.
            /// The hService parameter must be a handle to the service.
            /// </summary>
            SERVICE_NOTIFY_STOPPED = 0x00000001,
        }
        private enum NotificationErrorStatus
        {
            Success = 0,
            ServiceMarkedForDelete = 1,
            ServiceNotifyClientLagging = 2,
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_NOTIFY
        {
            public static readonly int dwVersion = 2;  //The structure version. This member must be SERVICE_NOTIFY_STATUS_CHANGE
            NotifyCallback pfnNotifyCallback;  //A pointer to the callback function.
            IntPtr pContext;  //Any user-defined data to be passed to the callback function.
            NotificationErrorStatus dwNotificationStatus;
            SERVICE_STATUS_PROCESS ServiceStatus;
            ServiceNotify dwNotificationTriggered;
            IntPtr pszServiceNames;  //MULTI_SZ string. MUST be deleted with Marshal.FreeHGlobal()
        }
        private delegate void NotifyCallback(IntPtr pContext);
        [DllImport("advapi32.dll", SetLastError = true)] private static extern int NotifyServiceStatusChange(IntPtr hService, ServiceNotify dwNotifyMask, ref SERVICE_NOTIFY pNotifyBuffer);
        #endregion -= Non-polling Service status; Windows 2008 Server, Vista, Win7, and up ONLY =-
        #endregion -= Win32 =-

        #region static void GrantUserLogOnAsAService(string userName);
        //http://www.morgantechspace.com/2013/11/Set-or-Grant-Logon-As-A-Service-right-to-User.html
        //http://pinvoke.net/default.aspx/advapi32/LsaAddAccountRights.html?diff=y
        public static void GrantUserLogOnAsAService(string userName)
        {
            LsaWrapper.SetRight(userName, "SeServiceLogonRight");
        }
        private static class LsaWrapper
        {
            #region Win32
            [DllImport("advapi32.dll", PreserveSig = true)]
            private static extern int LsaOpenPolicy(
                ref LSA_UNICODE_STRING SystemName,
                ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
                Int32 DesiredAccess,
                out IntPtr PolicyHandle
                );
 
            [DllImport("advapi32.dll", SetLastError = true, PreserveSig = true)]
            private static extern int LsaAddAccountRights(
                IntPtr PolicyHandle,
                IntPtr AccountSid,
                LSA_UNICODE_STRING[] UserRights,
                int CountOfRights);
 
            [DllImport("advapi32")]
            public static extern void FreeSid(IntPtr pSid);
 
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, PreserveSig = true)]
            private static extern bool LookupAccountName(
                string lpSystemName, string lpAccountName,
                IntPtr psid,
                ref int cbsid,
                StringBuilder domainName, ref int cbdomainLength, ref int use);
 
            [DllImport("advapi32.dll")]
            private static extern bool IsValidSid(IntPtr pSid);
 
            [DllImport("advapi32.dll")]
            private static extern int LsaClose(IntPtr ObjectHandle);
 
            [DllImport("kernel32.dll")]
            private static extern int GetLastError();
 
            [DllImport("advapi32.dll")]
            private static extern int LsaNtStatusToWinError(int status);
 
            // define the structures
 
            private enum LSA_AccessPolicy : long
            {
                POLICY_VIEW_LOCAL_INFORMATION = 0x00000001L,
                POLICY_VIEW_AUDIT_INFORMATION = 0x00000002L,
                POLICY_GET_PRIVATE_INFORMATION = 0x00000004L,
                POLICY_TRUST_ADMIN = 0x00000008L,
                POLICY_CREATE_ACCOUNT = 0x00000010L,
                POLICY_CREATE_SECRET = 0x00000020L,
                POLICY_CREATE_PRIVILEGE = 0x00000040L,
                POLICY_SET_DEFAULT_QUOTA_LIMITS = 0x00000080L,
                POLICY_SET_AUDIT_REQUIREMENTS = 0x00000100L,
                POLICY_AUDIT_LOG_ADMIN = 0x00000200L,
                POLICY_SERVER_ADMIN = 0x00000400L,
                POLICY_LOOKUP_NAMES = 0x00000800L,
                POLICY_NOTIFICATION = 0x00001000L
            }
 
            [StructLayout(LayoutKind.Sequential)]
            private struct LSA_OBJECT_ATTRIBUTES
            {
                public int Length;
                public IntPtr RootDirectory;
                public readonly LSA_UNICODE_STRING ObjectName;
                public UInt32 Attributes;
                public IntPtr SecurityDescriptor;
                public IntPtr SecurityQualityOfService;
            }
 
            [StructLayout(LayoutKind.Sequential)]
            private struct LSA_UNICODE_STRING
            {
                public UInt16 Length;
                public UInt16 MaximumLength;
                public IntPtr Buffer;
            }
            #endregion

            /// <summary>
            /// Adds a privilege to an account
            /// </summary>
            /// <param name="accountName">Name of an account - "domain\account" or only "account"</param>
            /// <param name="privilegeName">Name of the privilege</param>
            /// <exception>Win32 error as Win32Exception</exception>
            public static void SetRight(String accountName, String privilegeName)
            {
                int winErrorCode = 0; //contains the last error
 
                //pointer an size for the SID
                IntPtr sid = IntPtr.Zero;
                int sidSize = 0;
                //StringBuilder and size for the domain name
                var domainName = new StringBuilder();
                int nameSize = 0;
                //account-type variable for lookup
                int accountType = 0;
                //initialize a pointer for the policy handle
                IntPtr policyHandle = IntPtr.Zero;
                bool ok;

                //get required buffer size
                Win32Exception.ClearLastError();
                LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);
                if (sidSize==0) throw new Win32Exception("LookupAccountName", String.Format("LookupAccountName failed: Could not find \"{0}\".", accountName));
 
                //allocate buffers
                domainName = new StringBuilder(nameSize);
                sid = Marshal.AllocHGlobal(sidSize);

                try
                {
                    //lookup the SID for the account
                    Win32Exception.ClearLastError();
                    ok = LookupAccountName(String.Empty, accountName, sid, ref sidSize, domainName, ref nameSize, ref accountType);
                    if (!ok) throw new Win32Exception("LookupAccountName", "LookupAccountName failed");

                    //initialize an empty unicode-string
                    var systemName = new LSA_UNICODE_STRING();
                    //combine all policies
                    var access = (int)(
                                            LSA_AccessPolicy.POLICY_AUDIT_LOG_ADMIN |
                                            LSA_AccessPolicy.POLICY_CREATE_ACCOUNT |
                                            LSA_AccessPolicy.POLICY_CREATE_PRIVILEGE |
                                            LSA_AccessPolicy.POLICY_CREATE_SECRET |
                                            LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION |
                                            LSA_AccessPolicy.POLICY_LOOKUP_NAMES |
                                            LSA_AccessPolicy.POLICY_NOTIFICATION |
                                            LSA_AccessPolicy.POLICY_SERVER_ADMIN |
                                            LSA_AccessPolicy.POLICY_SET_AUDIT_REQUIREMENTS |
                                            LSA_AccessPolicy.POLICY_SET_DEFAULT_QUOTA_LIMITS |
                                            LSA_AccessPolicy.POLICY_TRUST_ADMIN |
                                            LSA_AccessPolicy.POLICY_VIEW_AUDIT_INFORMATION |
                                            LSA_AccessPolicy.POLICY_VIEW_LOCAL_INFORMATION
                                        );

                    //these attributes are not used, but LsaOpenPolicy wants them to exists
                    var ObjectAttributes = new LSA_OBJECT_ATTRIBUTES();
                    ObjectAttributes.Length = 0;
                    ObjectAttributes.RootDirectory = IntPtr.Zero;
                    ObjectAttributes.Attributes = 0;
                    ObjectAttributes.SecurityDescriptor = IntPtr.Zero;
                    ObjectAttributes.SecurityQualityOfService = IntPtr.Zero;

                    //get a policy handle
                    int resultPolicy = LsaOpenPolicy(ref systemName, ref ObjectAttributes, access, out policyHandle);
                    winErrorCode = LsaNtStatusToWinError(resultPolicy);
                    if (winErrorCode != 0) throw new Win32Exception("LsaOpenPolicy",winErrorCode, "OpenPolicy failed");
                    //Now that we have the SID an the policy,
                    //we can add rights to the account.

                    //initialize an unicode-string for the privilege name
                    var userRights = new LSA_UNICODE_STRING[1];
                    userRights[0] = new LSA_UNICODE_STRING();
                    userRights[0].Buffer = Marshal.StringToHGlobalUni(privilegeName);
                    userRights[0].Length = (UInt16)(privilegeName.Length * UnicodeEncoding.CharSize);
                    userRights[0].MaximumLength = (UInt16)((privilegeName.Length + 1) * UnicodeEncoding.CharSize);

                    //add the right to the account
                    int res = LsaAddAccountRights(policyHandle, sid, userRights, 1);
                    winErrorCode = LsaNtStatusToWinError(res);
                    if (winErrorCode != 0) throw new Win32Exception("LsaAddAccountRights", winErrorCode, "LsaAddAccountRights failed");
                }
                finally
                {
                    if (policyHandle != IntPtr.Zero) LsaClose(policyHandle);
                    if (sid != IntPtr.Zero) FreeSid(sid);
                }
            }   
        }
        #endregion

        #region -= Static Service Management Methods =-
        /// <summary>
        /// Uninstall the specified service. 
        /// Terminates other processes that have the specified service open so reboot is not required.
        /// 
        /// Exceptions: 
        ///   ChuckHill2.Utilities.Win32Exception:
        ///     OpenSCManager: Could not connect to service control manager.
        ///     OpenService: Service not installed.
        ///     DeleteService: Could not delete service
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns>True if reboot is required anyway.</returns>
        public static bool Uninstall(string ServiceName)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                scman = OpenSCManager(ServiceManagerRights.Connect);
                IntPtr service = OpenService(scman, ServiceName, ServiceRights.StandardRightsRequired | ServiceRights.Stop | ServiceRights.QueryStatus);
                if (service == IntPtr.Zero) throw new Win32Exception("OpenService","Service not installed.");

                //Services.msc opens ALL the service entries. It will cause the service from being 
                //completely uninstalled. It will just be marked for deletion upon reboot.
                //There may be others....
                foreach (Process p in Process.GetProcessesByName("mmc")) p.Kill(); //aka Services.msc

                try
                {
                    Stop(service, ServiceName);  //stop nicely or kill it if it is not nice.
                    Win32Exception.ClearLastError();
                    int ret = DeleteService(service);
                    //int hResult = Marshal.GetLastWin32Error();  //1072==The specified service has been marked for deletion.
                    if (ret == 0) throw new Win32Exception("DeleteService", "Could not delete service.");
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }

            //HACK: Test if reboot is required due to other processes having THIS SCM service open.
            int deleteFlag = 0;
            try
            {
                deleteFlag = (int)Registry.LocalMachine.GetValue(@"SYSTEM\CurrentControlSet\Services\" + ServiceName + @"\DeleteFlag", 0);
            }
            catch { }
            return (deleteFlag == 1);
        }

        /// <summary>
        /// Test if specified service is installed
        /// 
        /// Exceptions: 
        ///   ChuckHill2.Utilities.Win32Exception:
        ///     OpenSCManager: Could not connect to service control manager.
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns>True if installed</returns>
        public static bool IsInstalled(string ServiceName)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                scman = OpenSCManager(ServiceManagerRights.Connect);
                IntPtr service = OpenService(scman, ServiceName, ServiceRights.QueryStatus);
                if (service == IntPtr.Zero) return false;
                CloseServiceHandle(service);

                //HACK: Test if reboot is required due to other processes having THIS SCM service open.
                int deleteFlag = 0;
                try
                {
                    deleteFlag = (int)Registry.LocalMachine.GetValue(@"SYSTEM\CurrentControlSet\Services\" + ServiceName + @"\DeleteFlag", 0);
                }
                catch { }
                return (deleteFlag == 0);
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }
        }

        /// <summary>
        /// Install a service with various parameters.
        /// 
        /// Exceptions: 
        ///   ChuckHill2.Utilities.Win32Exception:
        ///     OpenSCManager: Could not connect to service control manager.
        ///     CreateService: Failed to install service.
        ///     ChangeServiceConfig2: Description not assigned to service.
        ///     ChangeServiceConfig2: DelayAutoStart not enabled.
        ///     ChangeServiceConfig2: SCM Watchdog not enabled.
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <param name="DisplayName"></param>
        /// <param name="executable"></param>
        /// <param name="startIt"></param>
        /// <param name="dependencies"></param>
        /// <param name="runAsAccount"></param>
        /// <param name="runAsPassword"></param>
        /// <param name="description"></param>
        /// <param name="delayAutoStart"></param>
        /// <param name="enableWatchdog"></param>
        public static void Install(string ServiceName, string DisplayName, string executable, bool startIt, string[] dependencies=null, string runAsAccount=null, string runAsPassword=null, string description=null, bool delayAutoStart=false, bool enableWatchdog=false)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                if (runAsAccount.IsNullOrEmpty() || runAsAccount.EqualsI("LocalSystem") || runAsAccount.EqualsI(@"NT AUTHORITY\SYSTEM")) //force to run as LocalSystem (aka NT AUTHORITY)
                {
                    runAsAccount = null;
                    runAsPassword = null;
                }
                if (!runAsAccount.IsNullOrEmpty())
                {
                    runAsAccount = UserAccount.FixSamAccountName(runAsAccount);
                    if (!UserAccount.ValidateCredentials(runAsAccount, runAsPassword)) throw new System.Security.Authentication.InvalidCredentialException(string.Format("Accunt \"{0}\" credentials are invalid.", runAsAccount));
                    GrantUserLogOnAsAService(runAsAccount);
                }

                scman = OpenSCManager(ServiceManagerRights.Connect | ServiceManagerRights.CreateService);
                IntPtr service = OpenService(scman, ServiceName, ServiceRights.QueryStatus | ServiceRights.Start | ServiceRights.ChangeConfig);
                if (service == IntPtr.Zero)
                {
                    StringBuilder sb = null;

                    if (dependencies != null && dependencies.Length > 0)
                    {
                        sb = new StringBuilder();
                        foreach (string s in dependencies)
                        {
                            sb.Append(s);
                            sb.Append('\0');
                        }
                        sb.Append('\0');
                    }

                    service = CreateService(
                        scman, 
                        ServiceName, 
                        DisplayName,
                        ServiceRights.QueryStatus | ServiceRights.Start | ServiceRights.ChangeConfig, 
                        SERVICE_WIN32_OWN_PROCESS, 
                        ServiceBootFlag.AutoStart, 
                        ServiceError.Normal, 
                        executable, 
                        null,  //name of load order group to which this service belongs
                        IntPtr.Zero, //load order index within the above load order group
                        (sb!=null?sb.ToString():null), //list of services-by-name that this service is dependent upon
                        runAsAccount, //account name that this service runs under.
                        runAsPassword); //account password
                }
                if (service == IntPtr.Zero) throw new Win32Exception("CreateService", "Failed to install service.");
                if (!string.IsNullOrEmpty(description))
                {
                    var value = new SERVICE_DESCRIPTION() { lpDescription = description };
                    bool ok = ChangeServiceConfig2(service, SERVICE_CONFIG.DESCRIPTION, ref value);
                    if (!ok) throw new Win32Exception("ChangeServiceConfig2", "Description not assigned to service");
                }
                if (delayAutoStart)
                {
                    var value = new SERVICE_DELAYED_AUTO_START() { fDelayedAutostart = delayAutoStart };
                    bool ok = ChangeServiceConfig2(service, SERVICE_CONFIG.DELAYED_AUTOSTART, ref value);
                    if (!ok) throw new Win32Exception("ChangeServiceConfig2", "DelayAutoStart not enabled");
                }
                if (enableWatchdog)
                {
                    //Rules: if service fails, restart it after 5 minutes. Do this up to 3 times within one day.
                    SERVICE_FAILURE_ACTIONS act = new SERVICE_FAILURE_ACTIONS();
                    act.dwResetPeriod = 86400; //86400 seconds == 1 day
                    act.lpRebootMsg = null;
                    act.lpCommand = null;
                    SC_ACTION[] lpsaActions = new SC_ACTION[4];
                    lpsaActions[0].Delay = 300000; lpsaActions[0].Type = SC_ACTION_TYPE.RESTART; //300000ms == 5 minutes
                    lpsaActions[1].Delay = 300000; lpsaActions[1].Type = SC_ACTION_TYPE.RESTART;
                    lpsaActions[2].Delay = 300000; lpsaActions[2].Type = SC_ACTION_TYPE.RESTART;
                    lpsaActions[3].Delay = 0;      lpsaActions[3].Type = SC_ACTION_TYPE.NONE;
                    try
                    {
                        act.CopyToStruct(lpsaActions);
                        bool ok = ChangeServiceConfig2(service, SERVICE_CONFIG.FAILURE_ACTIONS, ref act);
                        if (!ok) throw new Win32Exception("ChangeServiceConfig2", "SCM Watchdog not enabled");
                    }
                    finally
                    {
                        act.Dispose();
                    }
                }

                try
                {
                    if (startIt) Start(service);
                }
                finally
                {
                    CloseServiceHandle(service);
                }
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }
        }

        /// <summary>
        /// Start the specified service.
        /// 
        /// Exceptions: 
        ///   ChuckHill2.Utilities.Win32Exception:
        ///     OpenSCManager: Could not connect to service control manager.
        ///     OpenService: Could not open service.
        /// </summary>
        /// <param name="ServiceName"></param>
        public static void Start(string ServiceName)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                scman = OpenSCManager(ServiceManagerRights.Connect);
                IntPtr hService = OpenService(scman, ServiceName, ServiceRights.QueryStatus | ServiceRights.Start);
                if (hService == IntPtr.Zero) throw new Win32Exception("OpenService", "Could not open service.");
                try { Start(hService); }
                finally { CloseServiceHandle(hService); }
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }
        }

        /// <summary>
        /// Stop the specified service.
        /// If the service does not stop in a timely fashion, it is KILLed.
        /// 
        /// Exceptions: 
        ///   ChuckHill2.Utilities.Win32Exception:
        ///     OpenSCManager: Could not connect to service control manager.
        ///     OpenService: Could not open service.
        /// </summary>
        /// <param name="ServiceName"></param>
        public static void Stop(string ServiceName)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                scman = OpenSCManager(ServiceManagerRights.Connect);
                IntPtr hService = OpenService(scman, ServiceName, ServiceRights.QueryStatus | ServiceRights.Stop);
                if (hService == IntPtr.Zero) throw new Win32Exception("OpenService", "Could not open service.");
                try { Stop(hService, ServiceName); }
                finally { CloseServiceHandle(hService); }
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }
        }

        public static ServiceBootFlag SetStartupType(string ServiceName, ServiceBootFlag flag)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                scman = OpenSCManager(ServiceManagerRights.Connect);
                IntPtr hService = OpenService(scman, ServiceName, ServiceRights.QueryConfig | ServiceRights.ChangeConfig);
                if (hService == IntPtr.Zero) throw new Win32Exception("StartupType", "Could not open service.");
                try { return SetStartupType(hService, flag); }
                finally { CloseServiceHandle(hService); }
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }
        }

        private static bool Start(IntPtr hService)
        {
            StartService(hService, 0, 0);
            return WaitForServiceStatus(hService, ServiceState.Starting, ServiceState.Running);
        }

        private static void Stop(IntPtr hService, string ServiceName=null)
        {
            SERVICE_STATUS status = new SERVICE_STATUS();
            ControlService(hService, ServiceControl.Stop, ref status);
            WaitForServiceStatus(hService, ServiceState.Stopping, ServiceState.Stopped);
            if (!string.IsNullOrEmpty(ServiceName))
            {
                //HACK: If the service control manager cannot get the unruly service to stop, we just KILL it.
                RegistryKey key = null;
                string path = string.Empty;
                try
                {
                    key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + ServiceName);
                    path = key.GetValue("ImagePath", "").ToString();
                }
                catch { }
                finally { if (key != null) key.Close(); }
                if (string.IsNullOrEmpty(path)) return;
                foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path)))
                {
                    try { if (!p.HasExited) p.Kill(); }
                    catch { }
                }
            }
        }

        private static ServiceBootFlag SetStartupType(IntPtr hService, ServiceBootFlag flag)
        {
            ServiceBootFlag oldFlag = (ServiceBootFlag)(-1);
            SERVICE_DELAYED_AUTO_START das = new SERVICE_DELAYED_AUTO_START();
            int bytesNeeded = 0;
            bool ok = QueryServiceConfig2(hService,SERVICE_CONFIG.DELAYED_AUTOSTART,out das,Marshal.SizeOf(das), out bytesNeeded);
            if (!ok) throw new Win32Exception("QueryServiceConfig2", "Could not query SERVICE_CONFIG.DELAYED_AUTOSTART.");
            if (das.fDelayedAutostart) oldFlag = ServiceBootFlag.DelayedAutoStart;
            else
            {
                IntPtr hServiceConfig = Marshal.AllocHGlobal(4096);
                ok = QueryServiceConfig(hService, hServiceConfig, 4096, out bytesNeeded);
                if (!ok)
                {
                    Marshal.FreeHGlobal(hServiceConfig);
                    throw new Win32Exception("QueryServiceConfig", "Could not query SERVICE_CONFIG.");
                }
                QUERY_SERVICE_CONFIG sc = (QUERY_SERVICE_CONFIG)Marshal.PtrToStructure(hServiceConfig, typeof(QUERY_SERVICE_CONFIG));
                Marshal.FreeHGlobal(hServiceConfig);
                oldFlag = (ServiceBootFlag)sc.dwStartType;
            }
            if (oldFlag == flag) return flag;
            if (flag == ServiceBootFlag.DelayedAutoStart)
            {
                das.fDelayedAutostart = true;
                ok = ChangeServiceConfig2(hService, SERVICE_CONFIG.DELAYED_AUTOSTART, ref das);
                if (!ok) throw new Win32Exception("ChangeServiceConfig2", "Could not set SERVICE_CONFIG.DELAYED_AUTOSTART.");
            }
            else
            {
                ok = ChangeServiceConfig(hService, (ServiceType)SERVICE_NO_CHANGE, flag, (ServiceError)SERVICE_NO_CHANGE, null,null,0,null,null,null,null);
                if (!ok) throw new Win32Exception("ChangeServiceConfig", "Could not set service startup flag.");
            }

            return oldFlag;
        }

        /// <summary>
        /// Get the current state of the specified service.
        /// </summary>
        /// <param name="ServiceName"></param>
        /// <returns>the state of the service</returns>
        public static ServiceState GetStatus(string ServiceName)
        {
            IntPtr scman = IntPtr.Zero;
            try
            {
                scman = OpenSCManager(ServiceManagerRights.Connect);
                IntPtr hService = OpenService(scman, ServiceName, ServiceRights.QueryStatus);
                if (hService == IntPtr.Zero) return ServiceState.NotFound;
                try { return GetStatus(hService); }
                finally { CloseServiceHandle(scman); }
            }
            finally
            {
                if (scman != IntPtr.Zero) CloseServiceHandle(scman);
            }
        }

        private static ServiceState GetStatus(IntPtr hService)
        {
            SERVICE_STATUS ssStatus = new SERVICE_STATUS();
            if (QueryServiceStatus(hService, out ssStatus) == 0) throw new Win32Exception("QueryServiceStatus", "Failed to query service status.");
            return ssStatus.dwCurrentState;
        }

        /// <summary>
        /// Returns true when the service status has been changes from wait status to desired status,
        /// this method waits around 10 seconds for this operation.
        /// </summary>
        /// <param name="hService">The handle to the service</param>
        /// <param name="WaitStatus">The current state of the service</param>
        /// <param name="DesiredStatus">The desired state of the service</param>
        /// <returns>bool if the service has successfully changed states within the allowed timeline</returns>
        private static bool WaitForServiceStatus(IntPtr hService, ServiceState WaitStatus, ServiceState DesiredStatus)
        {
            SERVICE_STATUS ssStatus = new SERVICE_STATUS();
            uint dwOldCheckPoint;
            int dwStartTickCount;

            QueryServiceStatus(hService, out ssStatus);
            if (ssStatus.dwCurrentState == DesiredStatus) return true;
            dwStartTickCount = Environment.TickCount;
            dwOldCheckPoint = ssStatus.dwCheckPoint;

            while (ssStatus.dwCurrentState == WaitStatus)
            {
                // Do not wait longer than the wait hint. A good interval is
                // one tenth the wait hint, but no less than 1 second and no
                // more than 10 seconds.

                uint dwWaitTime = ssStatus.dwWaitHint / 10;

                if (dwWaitTime < 1000) dwWaitTime = 1000;
                else if (dwWaitTime > 10000) dwWaitTime = 10000;

                System.Threading.Thread.Sleep((int)dwWaitTime);

                // Check the status again.

                if (QueryServiceStatus(hService, out ssStatus) == 0) break;

                if (ssStatus.dwCheckPoint > dwOldCheckPoint)
                {
                    // The service is making progress.
                    dwStartTickCount = Environment.TickCount;
                    dwOldCheckPoint = ssStatus.dwCheckPoint;
                }
                else
                {
                    if (Environment.TickCount - dwStartTickCount > ssStatus.dwWaitHint)
                    {
                        // No progress made within the wait hint
                        break;
                    }
                }
            }
            return (ssStatus.dwCurrentState == DesiredStatus);
        }

        private static IntPtr OpenSCManager(ServiceManagerRights Rights)
        {
            IntPtr scman = OpenSCManager(null, null, Rights);
            if (scman == IntPtr.Zero) throw new Win32Exception("OpenSCManager", "Could not connect to service control manager.");
            return scman;
        }
        #endregion -= Static Methods =-

        #region -= [Legacy] (instance) Service State Notification (aka is running or not) =-
        public delegate void StatusHandler(object sender, StatusEventArgs e);
        public class StatusEventArgs : System.EventArgs
        {
            private bool isRunning;
            public bool IsRunning { get { return isRunning; } }
            public StatusEventArgs(bool isRunning) { this.isRunning = isRunning; }
        }
        public event StatusHandler ServiceStatus;

        private string m_serviceName;
        private IntPtr hSCM = IntPtr.Zero;
        private IntPtr hService = IntPtr.Zero;
        private bool m_isRunning = false;  //needed to determine when a state change occurs
        private Thread m_tmr; //Cant use other timers since they queue up all elapsed events across many threads in the threadpool.
        private AutoResetEvent m_exitEvent;
        private int m_pollInterval = 5000;  //default=5 seconds
        private Control m_owner;

        public WinService(Control owner, string serviceName)
        {
            m_serviceName = serviceName;
            m_owner = owner;
            m_exitEvent = new AutoResetEvent(false);
            m_tmr = new Thread(new ThreadStart(ThreadProc));
            m_tmr.Name = "WinService." + m_serviceName;
            m_tmr.IsBackground = true;  //Allow system to throw a ThreadAbortException to exit the thread upon program exit.
            m_tmr.Priority = ThreadPriority.BelowNormal;
            Init();  //'tmr' must be created first
        }

        private void ThreadProc()
        {
            WaitHandle[] handles = new System.Threading.WaitHandle[] { m_exitEvent };
            int pollInterval = m_pollInterval;

            while (true)
            {
                lock (m_tmr)
                {
                    pollInterval = m_pollInterval;
                    try
                    {
                        bool isrunning = Running();
                        if (isrunning != m_isRunning) //state has changed.
                        {
                            m_isRunning = isrunning;
                            if (ServiceStatus != null)
                                m_owner.Invoke(ServiceStatus, this, new StatusEventArgs(m_isRunning));  //execute on same thread as caller
                        }
                    }
                    catch //(Exception ex) 
                    {
                        //DBG.WriteLine("Polling Error: {0}",ex.Message);
                    }
                }
                int index = System.Threading.WaitHandle.WaitAny(handles, pollInterval);
                if (index == 0) break; //exit event triggered
            }
        }

        public bool IsInstalled() { lock (m_tmr) return (hService != IntPtr.Zero); }
        public int PollInterval { get { lock (m_tmr) return m_pollInterval / 1000; } set { lock (m_tmr) m_pollInterval = value * 1000; } }
        public void StartPolling()
        {
            if (hService == IntPtr.Zero) return;
            if (m_tmr.IsAlive) return;  //already polling!
            m_isRunning = !Running(); //initialize to the opposite state so the event will be triggered immediately
            m_tmr.Start();
        }
        public void StopPolling() { if (m_tmr.IsAlive) { m_exitEvent.Set(); m_tmr.Join(500); } }
        public bool IsPolling { get { return m_tmr.IsAlive; } }

        private bool Init()
        {
            lock (m_tmr)
            {
                if (hService != IntPtr.Zero) return true;
                hSCM = OpenSCManager(null, null, ServiceManagerRights.Connect);
                //if (hSCM == IntPtr.Zero) throw new Win32Exception("OpenSCManager", "Could not connect to service control manager.");
                if (hSCM != IntPtr.Zero) hService = OpenService(hSCM, m_serviceName, ServiceRights.QueryStatus | ServiceRights.Stop | ServiceRights.Start);
                //if (hService == IntPtr.Zero) throw new Win32Exception("OpenService", "Could not open service \""+m_serviceName+"\"");
                return (hService != IntPtr.Zero);
            }
        }
        public bool Running()
        {
            lock (m_tmr)
            {
                SERVICE_STATUS ssStatus = new SERVICE_STATUS();
                if (QueryServiceStatus(hService, out ssStatus) == 0)
                {
                    //DBG.WriteLine("ERROR: QueryServiceStatus");
                    return false;
                }
                return (ssStatus.dwCurrentState == ServiceState.Running || ssStatus.dwCurrentState == ServiceState.Starting || ssStatus.dwCurrentState == ServiceState.Resuming);
            }
        }
        public void Stop()
        {
            lock (m_tmr)
            {
                if (!Running()) return;
                Stop(hService, m_serviceName);
            }
        }
        public void Start()
        {
            lock (m_tmr)
            {
                if (Running()) return;
                Start(hService);
            }
        }
        public void Dispose()
        {
            lock (m_tmr)
            {
                if (m_exitEvent != null)
                {
                    m_exitEvent.Set();
                    m_tmr.Join(500);
                    m_exitEvent.Close();
                    m_exitEvent = null;
                }
                if (hService != IntPtr.Zero) { CloseServiceHandle(hService); hService = IntPtr.Zero; }
                if (hSCM != IntPtr.Zero) { CloseServiceHandle(hSCM); hSCM = IntPtr.Zero; }
            }
        }
        #endregion
    }
}

