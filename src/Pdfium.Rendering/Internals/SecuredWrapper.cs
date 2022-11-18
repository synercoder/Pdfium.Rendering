using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Pdfium.Rendering.Internals
{
    internal static class SecuredWrapper
    {
        private static readonly string _lockObj = string.Intern("d61f1ccb-8b03-41ff-ada3-e2a9f680384a");

        static SecuredWrapper()
        {
            NativeLibrary.SetDllImportResolver(typeof(SecuredWrapper).Assembly, PdfiumResolver.Resolve ?? _importResolver);

            FPDFInitializer.EnsureLoaded();
        }

        private static IntPtr _importResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == ExternalMethods.LIBRARY_NAME)
            {
                var basePath = new FileInfo(typeof(SecuredWrapper).Assembly.Location).Directory?.FullName ?? Environment.CurrentDirectory;
                var path = RuntimeInformation.ProcessArchitecture switch
                {
                    var arch when arch == Architecture.X64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => Path.Combine(basePath, "win-x64", "pdfium.dll"),
                    var arch when arch == Architecture.X86 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => Path.Combine(basePath, "win-x86", "pdfium.dll"),
                    var arch when arch == Architecture.X64 && RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => Path.Combine(basePath, "macos-x64", "libpdfium.dylib"),
                    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => Path.Combine(basePath, "linux", "libpdfium.so"),
                    _ => throw new PlatformNotSupportedException("Platform is not supported by the default loader, set PdfiumResolver.Resolve delegate with a method to load the native Pdfium library.")
                };

                libHandle = NativeLibrary.Load(path);
            }
            return libHandle;
        }

        public static IntPtr FPDF_LoadCustomDocument(Stream input, string? password, int id)
        {
            var getBlock = Marshal.GetFunctionPointerForDelegate(_getBlockDelegate);

            var access = new FPDF_FILEACCESS
            {
                m_FileLen = (uint)input.Length,
                m_GetBlock = getBlock,
                m_Param = (IntPtr)id
            };

            lock (_lockObj)
            {
                return ExternalMethods.FPDF_LoadCustomDocument(access, password);
            }
        }

        public static void FPDF_CloseDocument(IntPtr document)
        {
            lock (_lockObj)
                ExternalMethods.FPDF_CloseDocument(document);
        }

        public static IntPtr FPDF_LoadPage(IntPtr document, int index)
        {
            lock (_lockObj)
                return ExternalMethods.FPDF_LoadPage(document, index);
        }

        public static IntPtr FPDFText_LoadPage(IntPtr page)
        {
            lock (_lockObj)
                return ExternalMethods.FPDFText_LoadPage(page);
        }

        public static Size FPDF_GetPageSizeByIndexF(IntPtr document, int index)
        {
            lock (_lockObj)
            {
                var obj = new FS_SIZEF();

                ExternalMethods.FPDF_GetPageSizeByIndexF(document, index, obj);

                return new Size(obj.width, obj.height);
            }
        }

        public static int FPDF_GetPageCount(IntPtr document)
        {
            lock (_lockObj)
                return ExternalMethods.FPDF_GetPageCount(document);
        }

        public static IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride)
        {
            lock (_lockObj)
                return ExternalMethods.FPDFBitmap_CreateEx(width, height, format, first_scan, stride);
        }

        public static IntPtr FPDFBitmap_Destroy(IntPtr bitmap)
        {
            lock (_lockObj)
                return ExternalMethods.FPDFBitmap_Destroy(bitmap);
        }

        public static void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (_lockObj)
                ExternalMethods.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, color);
        }

        public static void FPDF_RenderPageBitmap(IntPtr bitmapHandle, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF_RenderingFlags flags)
        {
            lock (_lockObj)
                ExternalMethods.FPDF_RenderPageBitmap(bitmapHandle, page, start_x, start_y, size_x, size_y, rotate, (int)flags);
        }

        public static void InitLibrary()
        {
            lock (_lockObj)
                ExternalMethods.FPDF_InitLibrary();
        }

        public static void DestroyLibrary()
        {
            lock (_lockObj)
                ExternalMethods.FPDF_DestroyLibrary();
        }

        public static void FPDFText_ClosePage(IntPtr textPage)
        {
            lock (_lockObj)
                ExternalMethods.FPDFText_ClosePage(textPage);
        }

        public static void FPDF_ClosePage(IntPtr page)
        {
            lock (_lockObj)
                ExternalMethods.FPDF_ClosePage(page);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match FPDF delegate naming")]
        private delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr buffer, uint size);

        private static readonly FPDF_GetBlockDelegate _getBlockDelegate = _fPDF_GetBlock;

        private static int _fPDF_GetBlock(IntPtr param, uint position, IntPtr buffer, uint size)
        {
            var stream = StreamTracker.Get((int)param);
            if (stream == null)
                return 0;
            byte[] managedBuffer = new byte[size];

            stream.Position = position;
            int read = stream.Read(managedBuffer, 0, (int)size);
            if (read != size)
                return 0;

            Marshal.Copy(managedBuffer, 0, buffer, (int)size);
            return 1;
        }
    }
}
