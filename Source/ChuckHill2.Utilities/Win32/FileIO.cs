using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ChuckHill2.Win32
{
    public static partial class NativeMethods
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool DeleteFile(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool RemoveDirectory(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool SetFileAttributes(string lpFileName, FileAttributes attrib);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern FileAttributes GetFileAttributes(string lpFileName);
        [DllImport("Kernel32.dll", SetLastError = true)] public static extern bool CloseHandle(IntPtr hFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "WriteFile", SetLastError = true)]
        public static extern bool WriteFile(IntPtr hFile, String lpBuffer, Int32 nNumberOfBytesToWrite, out Int32 lpNumberOfBytesWritten, IntPtr Overlapped);

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFile(string name, GENERIC DesiredAccess, FILE_SHARE ShareMode, IntPtr SecurityAttributes, FILE_DISPOSITION CreationDisposition, FILE_ATTRIBUTES FlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFile(string name, GENERIC DesiredAccess, FILE_SHARE ShareMode, int SecurityAttributes, FILE_DISPOSITION CreationDisposition, FILE_ATTRIBUTES FlagsAndAttributes, int hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FindClose(IntPtr hFindFile);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public struct WIN32_FIND_DATA
    {
        public FileAttributes dwFileAttributes;
        public ulong ftCreationTime;
        public ulong ftLastAccessTime;
        public ulong ftLastWriteTime;
        public uint nFileSizeHigh;
        public uint nFileSizeLow;
        public uint dwReserved0;
        public uint dwReserved1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public string cAlternateFileName;
    }

    #region CreateFile enums
    [Flags]
    public enum GENERIC
    {
        READ = unchecked((int)0x80000000),
        WRITE = 0x40000000,
        READWRITE = (READ | WRITE),
        EXECUTE = 0x20000000,
        ALL = 0x10000000
    }
    [Flags]
    public enum FILE_SHARE
    {
        NONE = 0,
        READ = 0x00000001,
        WRITE = 0x00000002,
        DELETE = 0x00000004,
        READWRITE = (READ | WRITE)
    }
    public enum FILE_DISPOSITION
    {
        CREATE_NEW = 1,
        CREATE_ALWAYS = 2,
        OPEN_EXISTING = 3,
        OPEN_ALWAYS = 4,
        TRUNCATE_EXISTING = 5
    }
    [Flags]
    public enum FILE_ATTRIBUTES
    {
        WRITE_THROUGH = unchecked((int)0x80000000),
        OVERLAPPED = 0x40000000,
        NO_BUFFERING = 0x20000000,
        RANDOM_ACCESS = 0x10000000,
        SEQUENTIAL_SCAN = 0x08000000,
        DELETE_ON_CLOSE = 0x04000000,
        BACKUP_SEMANTICS = 0x02000000,
        POSIX_SEMANTICS = 0x01000000,
        OPEN_REPARSE_POINT = 0x00200000,
        OPEN_NO_RECALL = 0x00100000,
        FIRST_PIPE_INSTANCE = 0x00080000,

        READONLY = 0x00000001,
        HIDDEN = 0x00000002,
        SYSTEM = 0x00000004,
        DIRECTORY = 0x00000010,
        ARCHIVE = 0x00000020,
        DEVICE = 0x00000040,
        NORMAL = 0x00000080,
        TEMPORARY = 0x00000100,
        SPARSE_FILE = 0x00000200,
        REPARSE_POINT = 0x00000400,
        COMPRESSED = 0x00000800,
        OFFLINE = 0x00001000,
        NOT_CONTENT_INDEXED = 0x00002000,
        ENCRYPTED = 0x00004000,
        VIRTUAL = 0x00010000
    }
    #endregion
}
