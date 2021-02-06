using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ChuckHill2
{
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
                int hFile = CreateFile(tempfn, GENERIC_WRITE, FILE_SHARE_READWRITE, 0, CREATE_ALWAYS, FILE_FLAG_DELETE_ON_CLOSE | FILE_FLAG_NO_BUFFERING, 0);
                if (hFile <= 0) return false;
                CloseHandle(hFile);
            }
            catch { return false; }
            return true;
        }
        #region Win32 - CreateFile(), CloseHandle()
        private const int GENERIC_WRITE = 0x40000000;
        private const int FILE_SHARE_READWRITE = 0x00000003;
        private const int CREATE_ALWAYS = 2;
        private const int FILE_FLAG_DELETE_ON_CLOSE = 0x04000000;
        private const int FILE_FLAG_NO_BUFFERING = 0x20000000;
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(int hFile);
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Int32 CreateFile(String name, Int32 DesiredAccess, Int32 ShareMode, Int32 SecurityAttributes, Int32 CreationDisposition, Int32 FlagsAndAttributes, Int32 hTemplateFile);
        #endregion

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
            IntPtr hFind = Win32.FindFirstFile(Path.Combine(dir, "*"), out fd);
            if (hFind == Win32.INVALID_HANDLE_VALUE) return false;
            do
            {
                if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                string path = Path.Combine(dir, fd.cFileName);
                if ((fd.dwFileAttributes & FileAttributes.ReadOnly) != 0) Win32.SetFileAttributes(path, fd.dwFileAttributes & ~FileAttributes.ReadOnly);
                if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                {
                    DeleteDirectoryTree(ref fd, path, sb);
                    Win32.RemoveDirectory(path);
                    continue;
                }
                if (!Win32.DeleteFile(path) && sb != null) sb.AppendFormat("{0} : {1}\r\n", path, Win32Exception.GetLastErrorMessage());
            } while (Win32.FindNextFile(hFind, out fd));
            Win32.FindClose(hFind);
            return Win32.RemoveDirectory(dir);
        }
    }
}
