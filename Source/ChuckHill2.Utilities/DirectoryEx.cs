using System;
using System.IO;
using System.Text;
using ChuckHill2.Win32;

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
    }
}
