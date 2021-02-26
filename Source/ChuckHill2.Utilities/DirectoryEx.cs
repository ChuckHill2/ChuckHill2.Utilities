using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ChuckHill2.Win32;

namespace ChuckHill2
{
    /// <summary>
    /// Directory Management Utilities
    /// </summary>
    public static class DirectoryEx
    {
        /// <summary>
        /// Check if specified directory exists and is writable.
        /// </summary>
        /// <param name="dir">Directory/folder to test.</param>
        /// <returns>True if directory writable.</returns>
        public static bool IsWritable(string dir)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dir)) return false;
                if (dir[dir.Length - 1] == '\\') dir = dir.Substring(0, dir.Length - 1);
                string tempfn = string.Format("{0}\\{1:N}.tmp", dir, Guid.NewGuid());
                //Go low-level for efficency
                var hFile = NativeMethods.CreateFile(tempfn, GENERIC.WRITE, FILE_SHARE.READWRITE, 0, FILE_DISPOSITION.CREATE_ALWAYS, FILE_ATTRIBUTES.DELETE_ON_CLOSE | FILE_ATTRIBUTES.NO_BUFFERING, 0);
                if (hFile == NativeMethods.INVALID_HANDLE_VALUE) return false;
                NativeMethods.CloseHandle(hFile);
            }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Delete entire directory tree including all files. Even read-only ones.
        /// NO exceptions will be thrown.
        /// </summary>
        /// <param name="dir">Directory tree to be deleted.</param>
        /// <param name="sb">Optional. Contains a list of files (with error msg) that could not be deleted.</param>
        /// <returns>True if successful, if false, 'sb' contains the files that could not be deleted.</returns>
        public static bool DeleteDirectoryTree(string dir, StringBuilder sb = null)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                if (sb != null) sb.AppendFormat("Directory \"{0}\" not found.", dir ?? "null");
                return false;
            }
            if (!Directory.Exists(dir))
            {
                if (sb != null) sb.AppendFormat("Directory \"{0}\" not found.", dir);
                return false;
            }

            Win32.WIN32_FIND_DATA fd = new Win32.WIN32_FIND_DATA();  //pass as ref (aka ptr) to avoid filling up the stack
            return DeleteDirectoryTree(ref fd, dir, sb);
        }

        private static bool DeleteDirectoryTree(ref Win32.WIN32_FIND_DATA fd, string dir, StringBuilder sb)
        {
            IntPtr hFind = NativeMethods.FindFirstFile(Path.Combine(dir, "*"), out fd);
            if (hFind == NativeMethods.INVALID_HANDLE_VALUE) return false;
            do
            {
                if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                string path = Path.Combine(dir, fd.cFileName);
                if ((fd.dwFileAttributes & FileAttributes.ReadOnly) != 0) NativeMethods.SetFileAttributes(path, fd.dwFileAttributes & ~FileAttributes.ReadOnly);
                if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                {
                    DeleteDirectoryTree(ref fd, path, sb);
                    NativeMethods.RemoveDirectory(path);
                    continue;
                }
                if (!NativeMethods.DeleteFile(path) && sb != null) sb.AppendFormat("{0} : {1}\r\n", path, Win32Exception.GetLastErrorMessage());
            } while (NativeMethods.FindNextFile(hFind, out fd));
            NativeMethods.FindClose(hFind);
            return NativeMethods.RemoveDirectory(dir);
        }

        /// <summary>
        ///    Returns the count of files that match the specified search pattern in the specified
        ///    directory, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <param name="reSearchPattern">
        ///    The regex search string to match against the names of files in path. The parameter cannot
        ///    end in two periods ("..") or contain two periods ("..") followed by System.IO.Path.
        ///    DirectorySeparatorChar or System.IO.Path.AltDirectorySeparatorChar, nor can it
        ///    contain any of the characters in System.IO.Path.InvalidPathChars.
        /// </param>
        /// <param name="searchOption">
        ///    One of the enumeration values that specifies whether the search operation should
        ///    include all subdirectories or only the current directory.
        /// </param>
        /// <returns>
        ///    An array of the full names (including paths) for the files in the specified
        ///    directory that match the specified search pattern and option.
        /// </returns>
        public static int GetFileCount(string path, string reSearchPattern, SearchOption searchOption)
        {
            var fd = new WIN32_FIND_DATA();
            Regex re = new Regex(reSearchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return GetFileCount(ref fd, path, re, searchOption);
        }

        private static int GetFileCount(ref WIN32_FIND_DATA fd, string path, Regex searchPattern, SearchOption searchOption)
        {
            IntPtr hFind = NativeMethods.FindFirstFile(Path.Combine(path, "*"), out fd);
            int kount = 0;
            if (hFind != NativeMethods.INVALID_HANDLE_VALUE)
            {
                do
                {
                    if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                    if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                    {
                        if (searchOption != SearchOption.AllDirectories) continue;
                        kount += GetFileCount(ref fd, Path.Combine(path, fd.cFileName), searchPattern, searchOption);
                        continue;
                    }
                    if (!searchPattern.IsMatch(fd.cFileName)) continue;
                    kount++;
                } while (NativeMethods.FindNextFile(hFind, out fd));
                NativeMethods.FindClose(hFind);
            }
            return kount;
        }

        /// <summary>
        /// Get the most recent date in a collection of files.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <param name="reSearchPattern">
        ///    The regex search string to match against the names of files in path. The parameter cannot
        ///    end in two periods ("..") or contain two periods ("..") followed by System.IO.Path.
        ///    DirectorySeparatorChar or System.IO.Path.AltDirectorySeparatorChar, nor can it
        ///    contain any of the characters in System.IO.Path.InvalidPathChars.
        /// </param>
        /// <param name="searchOption">
        ///    One of the enumeration values that specifies whether the search operation should
        ///    include all subdirectories or only the current directory.
        /// </param>
        /// <returns></returns>
        public static DateTime GetFileMostRecentCreateDate(string path, string reSearchPattern, SearchOption searchOption)
        {
            var fd = new WIN32_FIND_DATA();
            Regex re = new Regex(reSearchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            long ftCreationTime = unchecked((long)GetFileMostRecentCreateDate(ref fd, path, re, searchOption));
            return DateTime.FromFileTime(ftCreationTime);
        }

        private static ulong GetFileMostRecentCreateDate(ref WIN32_FIND_DATA fd, string path, Regex searchPattern, SearchOption searchOption)
        {
            IntPtr hFind = NativeMethods.FindFirstFile(Path.Combine(path, "*"), out fd);
            ulong ftCreationTime = 0;
            if (hFind != NativeMethods.INVALID_HANDLE_VALUE)
            {
                do
                {
                    if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                    if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                    {
                        if (searchOption != SearchOption.AllDirectories) continue;
                        var t = GetFileMostRecentCreateDate(ref fd, Path.Combine(path, fd.cFileName), searchPattern, searchOption);
                        if (t > ftCreationTime) ftCreationTime = t;
                        continue;
                    }
                    if (!searchPattern.IsMatch(fd.cFileName)) continue;
                    if (fd.ftCreationTime > ftCreationTime) ftCreationTime = fd.ftCreationTime;
                } while (NativeMethods.FindNextFile(hFind, out fd));
                NativeMethods.FindClose(hFind);
            }
            return ftCreationTime;
        }
    }
}
