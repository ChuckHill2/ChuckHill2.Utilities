using System;
using System.Runtime.InteropServices;
using ChuckHill2.Utilities.Extensions;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Base class to a custom-crafted COM interface class. This enables the developer to put a friendlier face on a instantiated COM object. 
    /// Also for some COM objects, the automatically generated interop is incomplete, so this would be the only means.
    /// Does not support COM interfaces with '[MethodImpl(MethodImplOptions.InternalCall)]' attribute.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// @code{.cs}
    ///    [Guid("CBE74C73-621A-424E-9189-AD270494FF26"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    ///    internal interface IMyComponent
    ///    {
    ///        int GetIds(out int kount, out IntPtr Ids);
    ///        int WriteImage(
    ///            [MarshalAs(UnmanagedType.BStr)] string bstrId,
    ///            [MarshalAs(UnmanagedType.BStr)] string bstrPath,
    ///            [MarshalAs(UnmanagedType.BStr)] string bstrImagePath,
    ///            int numImageType,
    ///            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeParamIndex = 3)] int[] imageTypes,
    ///            int type);
    ///        int ExportResult(
    ///            [MarshalAs(UnmanagedType.BStr)] string bstrId,
    ///            [MarshalAs(UnmanagedType.BStr)] string bstrPath,
    ///            int type);
    ///        int Close();
    ///    }
    ///    [Guid("CBE74C74-621A-424E-9189-AD270494FF26")]  // CLSID
    ///    public class MyComponent : ComObject
    ///    {
    ///        private IMyComponent ii = null;
    ///        public MyComponent() : base()
    ///        {
    ///            ii = base.Unknown as IMyComponent;
    ///            if (ii==null) { throw new COMException("IUnknown doesn't support interface IMyComponent",-1); }
    ///        }
    ///        ~MyComponent() { this.Dispose(); }
    ///        public override void Dispose() { ii = null; base.Dispose(); }
    ///        public string[] GetIds()
    ///        {
    ///            int kount=0;
    ///            IntPtr pstrIds;
    ///            int hresult = ii.GetIds(out kount, out pstrIds);
    ///            if (FAILED(hresult)) throw new COMException("MyComponent.GetIds", hresult);
    ///            string[] ids = new string[kount];
    ///            for(int i=0; i ‹ ids.Length; i++)
    ///            {
    ///                IntPtr psz = Marshal.ReadIntPtr(pstrIds, i*4);
    ///                ids[i] = Marshal.PtrToStringBSTR(psz);
    ///                Marshal.FreeBSTR(psz);
    ///            }
    ///            Marshal.FreeCoTaskMem(pstrIds);
    ///            return ids;
    ///        }
    ///        public void WriteImage(string bstrId, string bstrPath, string bstrImagePath, int[] imageTypes, int type)
    ///        {
    ///            int hresult = ii.WriteImage(bstrId, bstrPath, bstrImagePath, imageTypes.Length, imageTypes, type);
    ///            if (FAILED(hresult)) throw new COMException("MyComponent.WriteImage", hresult);
    ///        }
    ///        public void ExportResult(string bstrId, string bstrPath, int type)
    ///        {
    ///            int hresult = ii.ExportResult(bstrId, bstrPath, type);
    ///            if (FAILED(hresult)) throw new COMException("MyComponent.ExportResult", hresult);
    ///        }
    ///        public void Close()
    ///        {
    ///            int hresult = ii.Close();
    ///            if (FAILED(hresult)) throw new COMException("MyComponent.Close", hresult);
    ///        }
    ///    }
    ///    @endcode
    /// </remarks>
    public abstract class ComObject : IDisposable
    {
        private object m_object = null;
        private int m_hResult = unchecked((int)0x80000006);  //E_HANDLE = Invalid handle
        private const uint CLSCTX_INPROC_SERVER = 1;
        private static Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        [DllImport("ole32.dll")] private static extern int CoCreateInstance(ref Guid guid, 
            [MarshalAs(UnmanagedType.IUnknown)] object inner, 
            uint context, 
            ref Guid uuid, 
            [MarshalAs(UnmanagedType.IUnknown)] out object pointer);

        /// <summary>
        /// Instantiate a COM component. Assumes that the parent class is decorated with the 
        /// GuidAttribute which consists of the ClassID COM object identifier (clsid).
        /// </summary>
        public ComObject()
        {
            Guid clsid = new Guid(GetType().Attribute<GuidAttribute>());
            m_hResult = CoCreateInstance(ref clsid, null, CLSCTX_INPROC_SERVER, ref IID_IUnknown, out m_object);
        }

        /// <summary>
        /// Wrap a previously instantiated COM object to access an alternate interface.
        /// </summary>
        /// <param name="iUnknown"></param>
        public ComObject(object iUnknown)
        {
            if (!Marshal.IsComObject(iUnknown)) throw new COMException("iUnknown is not a COM object.", m_hResult);
            m_object = iUnknown;
            m_hResult = 0;
        }

        /// <summary>
        /// Provides a generic test for success on any status value. Non-negative numbers indicate success.
        /// </summary>
        /// <param name="hResult">COM return value to test</param>
        /// <returns>True if successful</returns>
        public static bool SUCCEEDED(int hResult) { return (hResult >= 0); }
        /// <summary>
        /// Provides a generic test for failure on any status value. Negative numbers indicate failure.
        /// </summary>
        /// <param name="hResult">COM return value to test</param>
        /// <returns>True if failed</returns>
        public static bool FAILED(int hResult) { return (hResult < 0); }

        public object Unknown { get { return m_object; } }
        public int HResult { get { return m_hResult; } }
        public bool Loaded { get { return m_hResult == 0; } }

        ~ComObject() { Dispose(); }
        /// <summary>
        /// Decrement COM usage count and invalidate this object
        /// </summary>
        public virtual void Dispose()
        {
            if (m_object == null) return;
            int count = Marshal.ReleaseComObject(m_object);
            m_object = null;
        }

        /// <summary>
        /// Imediately unloads all COM DLLs that are no longer in use.
        /// </summary>
        public static void FlushAll()
        {
            CoFreeUnusedLibraries();
            //CoFreeUnusedLibrariesEx(0,0);  //better, but it does not always exist in older versions of Windows2000
        }
        [DllImport("ole32.dll")] private static extern void CoFreeUnusedLibraries();
        //[DllImport("ole32.dll")] private static extern void CoFreeUnusedLibrariesEx(int dwUnloadDelay, int dwReserved);
   }
}
