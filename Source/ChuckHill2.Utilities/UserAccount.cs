//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="UserAccount.cs" company="Chuck Hill">
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
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using ChuckHill2.Extensions;
using Microsoft.Win32;
using ChuckHill2.Logging;
using ChuckHill2.Win32;

namespace ChuckHill2
{
    /// <summary>
    /// Common Windows user/logon account and permissions API
    /// </summary>
    public static class UserAccount
    {
        static Log LOG = new Log("Installer");
        #region Win32
        private enum EXTENDED_NAME_FORMAT
        {
            Unknown = 0,
            FullyQualifiedDN = 1,
            SamCompatible = 2,
            Display = 3,
            UniqueId = 6,
            Canonical = 7,
            UserPrincipal = 8,
            CanonicalEx = 9,
            ServicePrincipal = 10,
            DnsDomain = 12
        }
        [DllImport("secur32.dll", SetLastError = true)]
        private static extern bool TranslateName(string accountName, EXTENDED_NAME_FORMAT accountNameFormat, EXTENDED_NAME_FORMAT desiredNameFormat, StringBuilder translatedName, ref int bufsize);
        [DllImport("secur32.dll", SetLastError = true)]
        private static extern bool GetUserNameEx(EXTENDED_NAME_FORMAT accountNameFormat, StringBuilder accountName, ref int bufsize);
        #endregion

        /// <summary>
        /// Translate User Principal Account name (username@domain.com) into Old-style NetBios account name (domain\username)
        /// </summary>
        /// <param name="accountName">UserPrincipal account name to translate or NULL to get current account name</param>
        /// <returns>Old-style NetBios account name (domain\username)</returns>
        public static string Translate2SAM(string accountName = null)
        {
            StringBuilder sb = new StringBuilder(256);
            int capacity = sb.Capacity;

            if (accountName.IsNullOrEmpty())
            {
                if (GetUserNameEx(EXTENDED_NAME_FORMAT.SamCompatible, sb, ref capacity)) return sb.ToString();
                return string.Empty;
            }
            if (accountName.EqualsI("LocalSystem")) return @"NT AUTHORITY\SYSTEM"; //special case...
            if (accountName.EqualsI(@"NT AUTHORITY\SYSTEM")) return @"NT AUTHORITY\SYSTEM"; //special case...
            accountName = FixSamAccountName(accountName);
            if (!accountName.Contains("@")) return accountName; //were done.

            if (TranslateName(accountName, EXTENDED_NAME_FORMAT.UserPrincipal, EXTENDED_NAME_FORMAT.SamCompatible, sb, ref capacity)) return sb.ToString();
            //OK, try the hard way...
            var items = accountName.Split('@');
            string username=null, domain=null;
            if (items.Length > 0) username = items[0];
            if (items.Length > 1)
            {
                items = items[1].Split('.');
                if (items.Length > 0) domain = items[0].ToUpper();
            }
            if (username.IsNullOrEmpty() || domain.IsNullOrEmpty()) return string.Empty;
            return string.Format("{0}\\{1}", domain, username);
        }

        /// <summary>
        /// Translate Old-style NetBios account name (domain\username) into User Principal Account name (username@domain.com).
        /// </summary>
        /// <param name="accountName">UserPrincipal account name to translate or NULL to get current account name</param>
        /// <returns>
        /// User-Principal user account name or zero-length string if this is a local user (non-domain) account.
        /// </returns>
        public static string Translate2UPN(string accountName = null)
        {
            StringBuilder sb = new StringBuilder(256);
            int capacity = sb.Capacity;
            if (accountName.IsNullOrEmpty())
            {
                if (GetUserNameEx(EXTENDED_NAME_FORMAT.UserPrincipal, sb, ref capacity)) return sb.ToString();
                return string.Empty;
            }
            if (accountName.Contains('@')) return accountName;
            accountName = FixSamAccountName(accountName);
            if (TranslateName(accountName, EXTENDED_NAME_FORMAT.SamCompatible, EXTENDED_NAME_FORMAT.UserPrincipal, sb, ref capacity)) return sb.ToString();
            //Note: parsing string may not create a true translation.  DOMAIN\username =? username@domain.com || username@domain.gov || username@domain.company.com 
            return string.Empty;
        }

        /// <summary>
        /// Get the spacified service logon/run-as user account. May be in UPN or SAM formats.
        /// </summary>
        /// <param name="serviceName">Name of service to get user account from.</param>
        /// <returns>user account name.</returns>
        public static string GetInstalledServiceRunAs(string serviceName)
        {
            try
            {
                string value = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\" + serviceName, "ObjectName", null) as String;
                if (value.IsNullOrEmpty()) return string.Empty;
                return Translate2SAM(value);
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// Just verify Windows username logon exists and password works. 
        /// </summary>
        /// <param name="username">Username may be in either UPN or SAM formats.</param>
        /// <param name="password"></param>
        /// <returns>True if successful</returns>
        public static bool ValidateCredentials(string username, string password)
        {
            username = username.TrimEx();
            password = password.TrimEx();

            if (username.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                throw new ArgumentNullException("UserAccount.ValidateUser() username and/or password is empty.");
            }
            string domain = "";
            username = FixSamAccountName(username);
            string fullName = username; //for logging.
            try
            {
                if (username.Contains("\\"))
                {
                    string[] items = username.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items != null && items.Length > 0) domain = items[0];
                    if (items != null && items.Length > 1) username = items[1];
                }
                else if (username.Contains("@"))
                {
                    string[] items = username.Split(new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries);
                    if (items != null && items.Length > 0) username = items[0];
                    if (items != null && items.Length > 1) domain = items[1];
                }

                if (domain.EqualsI(Dns.GetHostName()))
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Machine);
                    return context.ValidateCredentials(username, password);
                }
                else
                {
                    PrincipalContext context = new PrincipalContext(ContextType.Domain, domain);
                    return context.ValidateCredentials(username, password, ContextOptions.Negotiate);
                }
            }
            catch (Exception ex)
            {
                throw ex.PrefixMessage(string.Format("UserAccount.ValidateCredentials(\"{0}\", \"*****\")", fullName));
            }
        }

        /// <summary>
        /// Verify Windows username logon exists, password works, 
        /// AND belongs to the local Administrators group. 
        /// </summary>
        /// <param name="username">Username may be in either UPN or SAM formats.</param>
        /// <param name="password"></param>
        /// <returns>True if successful</returns>
        public static bool ValidateAdminCredentials(string username, string password)
        {
            bool ok = false;
            try
            {
                using (var iu = new ImpersonateUser(username, password))
                {
                    ok = UserAccount.IsAdministrator();
                }
            }
            catch (Win32Exception ex) //ImpersonateUser uses LogonUser() Win32 API.
            {
                LOG.Error(ex, "");
                //Not fatal. Login failed -> Validation failed, thus returns false.
            }
            catch (Exception ex)
            {
                LOG.Error(ex, "");
                throw ex.PrefixMessage(string.Format("UserAccount.ValidateAdminCredentials(\"{0}\", \"*****\")", username));
            }
            return ok;
        }

        #region Win32 GetTokenInformation()
        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);

        /// <summary>
        /// Passed to <see cref="GetTokenInformation"/> to specify what
        /// information about the token to return.
        /// </summary>
        enum TokenInformationClass
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUiAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        /// <summary>
        /// The elevation type for a user token.
        /// </summary>
        enum TokenElevationType
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }
        #endregion

        /// <summary>
        /// Test if windows user account belongs to the Administrators group on this machine.
        /// Warning: This will fail if you are validating a domain user account while you 
        ///          are logged in a local machine or alternate domain user account.
        ///          Use ValidateAdminCredentials() instead.
        /// </summary>
        /// <param name="username">Username may be in either UPN or SAM formats. If NULL, assumes current logged in self.</param>
        /// <returns></returns>
        private static bool IsAdministrator(string username=null)
        {
            bool isAdmin = false;

            if (username.IsNullOrEmpty())
            {
                #region Test Current User
                //http://www.davidmoore.info/blog/2011/06/20/how-to-check-if-the-current-user-is-an-administrator-even-if-uac-is-on/
                var identity = WindowsIdentity.GetCurrent();
                if (identity == null) throw new InvalidOperationException("Couldn't get the current user identity");
                var principal = new WindowsPrincipal(identity);

                // Check if this user has the Administrator role. If they do, return immediately.
                // If UAC is on, and the process is not elevated, then this will actually return false.
                if (principal.IsInRole(WindowsBuiltInRole.Administrator)) return true;

                // If we're not running in Vista onwards, we don't have to worry about checking for UAC.
                if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version.Major < 6)
                {
                    // Operating system does not support UAC; skipping elevation check.
                    return false;
                }

                int tokenInfLength = Marshal.SizeOf(typeof(int));
                IntPtr tokenInformation = Marshal.AllocHGlobal(tokenInfLength);

                try
                {
                    var token = identity.Token;
                    var result = GetTokenInformation(token, TokenInformationClass.TokenElevationType, tokenInformation, tokenInfLength, out tokenInfLength);

                    if (!result)
                    {
                        var exception = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                        throw new InvalidOperationException("Couldn't get token information", exception);
                    }

                    var elevationType = (TokenElevationType)Marshal.ReadInt32(tokenInformation);

                    switch (elevationType)
                    {
                        case TokenElevationType.TokenElevationTypeDefault:
                            // TokenElevationTypeDefault - User is not using a split token, so they cannot elevate.
                            return false;
                        case TokenElevationType.TokenElevationTypeFull:
                            // TokenElevationTypeFull - User has a split token, and the process is running elevated. Assuming they're an administrator.
                            return true;
                        case TokenElevationType.TokenElevationTypeLimited:
                            // TokenElevationTypeLimited - User has a split token, but the process is not running elevated. Assuming they're an administrator.
                            return true;
                        default:
                            // Unknown token elevation type.
                            return false;
                    }
                }
                finally
                {
                    if (tokenInformation != IntPtr.Zero) Marshal.FreeHGlobal(tokenInformation);
                }
                #endregion
            }

            if (username.IsNullOrEmpty()) username = WindowsIdentity.GetCurrent().Name;
            else username = FixSamAccountName(username);
            try
            {
                if (username.Contains('\\'))
                {
                    PrincipalContext ctx = null;
                    UserPrincipal up = null;
                    var items = username.Split('\\');
                    if (items[0].EqualsI(Dns.GetHostName()))
                    {
                        ctx = new PrincipalContext(ContextType.Machine);
                        up = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, username);
                        isAdmin = up.IsMemberOf(ctx, IdentityType.Name, "Administrators");
                        return isAdmin;
                    }
                    ctx = new PrincipalContext(ContextType.Domain, items[0], null, ContextOptions.Negotiate, null, null);
                    up = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, username);
                    isAdmin = (up != null && up.IsMemberOf(new PrincipalContext(ContextType.Machine), IdentityType.Name, "Administrators"));
                    return isAdmin;
                }

                WindowsIdentity ident = new WindowsIdentity(username);
                isAdmin = new WindowsPrincipal(ident).IsInRole(WindowsBuiltInRole.Administrator);
                return isAdmin;
            }
            catch //(Exception ex)
            {
                return false; //If we get this far, then IsMemberOf() or IsInRole() failed because both require administrative privileges to execute!
                //throw ex.PrefixMessage(string.Format("IsUserAdmin(\"{0}\")", username));
            }
        }

        /// <summary>
        /// Convert implicit SAM user account names into explicit names. Any UPN names are returned unmodified.
        /// Examples: 
        ///   "OMCL764-1EFHS28\user1"  (local computer name)
        ///   "localhost\user1"
        ///   "127.0.0.1\user1"
        ///   "10.116.241.140\user1"  (local IP address)
        ///   "\user1"
        ///   ".\user1"
        ///   "user1"
        /// all equal to "OMCL764-1EFHS28\user1".
        /// Non-local SAM user account names are passed unmmodified.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        internal static string FixSamAccountName(string username)
        {
            //The only local username that Win32 CreateService(), LookupAccountName() and 
            //ValidateCredentials() recognizes is "computername\username".
            //All other flavors of user account name are passed unmodified.

            if (username.EqualsI("LocalSystem")) return @"NT AUTHORITY\SYSTEM"; //special case...
            if (username.EqualsI(@"NT AUTHORITY\SYSTEM")) return @"NT AUTHORITY\SYSTEM"; //special case...

            string fullUsername = username;
            if (username.IsNullOrEmpty()) return username; //nothing to do
            if (username.Contains('@')) return username; //ignore user-principal style account names aka myname@mycompany.com
            try
            {
                string domain = string.Empty;
                if (username.Contains('\\'))   // SAM username "mydomain\myname"
                {
                    if (username[0] == '\\' || username[0] == '/') username = username.Substring(1);  // local user "\myname"
                    else if (username[0] == '.' && (username[1] == '\\' || username[1] == '/')) username = username.Substring(2); // local user ".\myname"
                    else
                    {
                        string[] items = username.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                        if (items != null && items.Length > 0) domain = items[0];
                        if (items != null && items.Length > 1) username = items[1];
                        if (IsLocalIpAddress(domain)) domain = string.Empty;
                    }
                }
                if (domain.Length == 0) domain = Dns.GetHostName(); //must be a local user "myname"
                return string.Format("{0}\\{1}", domain.ToUpper(), username);
            }
            catch(Exception ex)
            {
                throw ex.PrefixMessage(string.Format("FixSamAccountName(\"{0}\")", fullUsername));
            }
        }

        /// <summary>
        /// Test if the specified hostname/ipv4 or ipv6 address belongs to this machine.
        /// </summary>
        /// <param name="hostname">Hostname or ip address to check</param>
        /// <returns>True/False</returns>
        internal static bool IsLocalIpAddress(string hostname)
        {
            try
            {
                string localHostName = Dns.GetHostName();
                if (hostname.IsNullOrEmpty()) return false;
                if (hostname.EqualsI(localHostName)) return true;
                if (hostname == "127.0.0.1") return true;
                if (hostname.EqualsI("localhost")) return true;
                if (hostname.EqualsI("(local)")) return true; //handle connection string "DataSource" value
                if (hostname == ".") return true;             //handle connection string "DataSource" value

                IPAddress[] hostIPs = Dns.GetHostAddresses(hostname);
                IPAddress[] localIPs = Dns.GetHostAddresses(localHostName);
                foreach (IPAddress hostIP in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    foreach (IPAddress localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    /// Temporarily Impersonate another Windows User.
    /// To use:
    /// var iu = new ImpersonateUser("myUserAccountName",myPassword");
    ///     [do work]
    /// iu.Dispose();
    /// **OR**
    /// using(var iu = new ImpersonateUser("myUserAccountName",myPassword"))
    /// {
    ///     [do work]
    /// }
    /// 
    /// </summary>
    [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
    public class ImpersonateUser : IDisposable
    {
        #region Win32
        // Use the unmanaged LogonUser function to get the user token for
        // the specified user, domain, and password.
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        // Passing this parameter causes LogonUser to create a primary token.
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUser(
        String lpszUsername,
        String lpszDomain,
        String lpszPassword,
        int dwLogonType,
        int dwLogonProvider,
        ref IntPtr phToken);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);
        #endregion

        private IntPtr tokenHandle = IntPtr.Zero;
        private WindowsImpersonationContext impersonatedUser = null;

        /// <summary>
        /// Constructor to impersonate a windows user
        /// </summary>
        /// <param name="userName">windows account with optional domain</param>
        /// <param name="password"></param>
        public ImpersonateUser(string userName, string password)
        {
            userName = UserAccount.Translate2SAM(userName);
            string domainName = string.Empty;
            string[] items = userName.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (items != null && items.Length > 0) domainName = items[0];
            if (items != null && items.Length > 1) userName = items[1];

            //Impersonating NT AUTHORITY doesn't work for me. Someone else says it does. This is their examples..
            //LogonUser(L"LocalService", L"NT AUTHORITY", NULL, LOGON32_LOGON_SERVICE, LOGON32_PROVIDER_DEFAULT, &hToken)
            //LogonUser(L"NetworkService", L"NT AUTHORITY", NULL, LOGON32_LOGON_SERVICE, LOGON32_PROVIDER_DEFAULT, &hToken)

            bool ok = LogonUser(userName, domainName, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref tokenHandle);
            if (!ok) throw new Win32Exception("LogonUser", string.Format("LogonUser(\"{0}\",\"{1}\",\"*****\") failed", userName, domainName));
            WindowsIdentity newId = new WindowsIdentity(tokenHandle);
            impersonatedUser = newId.Impersonate();
        }

        /// <summary>
        /// Stops impersonation and cleans up
        /// </summary>
        public void Dispose()
        {
            if (impersonatedUser != null) impersonatedUser.Undo();
            if (tokenHandle != IntPtr.Zero) CloseHandle(tokenHandle);
            tokenHandle = IntPtr.Zero;
            if (impersonatedUser != null) impersonatedUser.Dispose();
            impersonatedUser = null;
        }

        /// <summary>
        /// Impersonate a windows logon user only for the duration of the specified action.
        /// </summary>
        /// <param name="userName">windows account with optional domain</param>
        /// <param name="password"></param>
        /// <param name="runAs"></param>
        public static void Run(string userName, string password, Action runAs)
        {
            using (var iu = new ImpersonateUser(userName, password))
            {
                runAs();
            }
        }
    }

}
