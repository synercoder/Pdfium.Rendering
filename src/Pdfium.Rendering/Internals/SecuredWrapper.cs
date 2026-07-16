using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Pdfium.Rendering.Internals;

/// <summary>
/// Serialises every call into pdfium behind a single process-wide lock.
/// </summary>
/// <remarks>
/// <para>
/// This lock is required and must not be removed or narrowed to a per-document
/// or per-thread scope. Pdfium is not thread-safe and places the burden of
/// serialisation on the embedder. From the note above FPDF_InitLibrary in
/// pdfium's public/fpdfview.h:
/// </para>
/// <para>
/// "NOTE: None of the PDFium APIs are thread-safe. They expect to be called
/// from a single thread. Barring that, embedders are required to ensure (via a
/// mutex or similar) that only a single PDFium call can be made at a time."
/// </para>
/// <para>
/// Holding a document per thread is NOT sufficient. Rendering any page reaches
/// through the global CFX_GEModule into one process-wide CFX_FontMgr, which
/// mutates two std::map font caches and drives a single shared FreeType
/// FT_Library. Two threads rendering unrelated documents therefore race on that
/// shared state, which corrupts the heap rather than failing cleanly. The
/// failure is timing-dependent and largely disappears once the font cache is
/// warm, so removing this lock tends to look correct under test and break in
/// production.
/// </para>
/// <para>
/// The consequence is that pdfium work does not scale across threads within a
/// process. The upstream-recommended way to render in parallel is multiple
/// processes, each with its own FPDF_InitLibrary. See the threading section of
/// the README.
/// </para>
/// <para>
/// The lock is taken on an interned string so that every copy of this assembly
/// in the process (for example under separate AssemblyLoadContexts) shares one
/// lock, matching the fact that they share one native pdfium library.
/// </para>
/// </remarks>
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

            var filename = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "pdfium.dll"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? "libpdfium.so"
                : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? "libpdfium.dylib"
                : "";

            var rid = RuntimeInformation.RuntimeIdentifier switch
            {
                "linux-musl-x64" => "linux-musl-x64",
                "linux-musl-x86" => "linux-musl-x86",
                _ => RuntimeInformation.ProcessArchitecture switch
                {
                    var arch when arch == Architecture.X64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-x64",
                    var arch when arch == Architecture.X86 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-x86",
                    var arch when arch == Architecture.Arm64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win-arm64",

                    var arch when arch == Architecture.X64 && RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "osx-x64",
                    var arch when arch == Architecture.Arm64 && RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "osx-arm64",

                    var arch when arch == Architecture.Arm && RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux-arm",
                    var arch when arch == Architecture.Arm64 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux-arm64",
                    var arch when arch == Architecture.X64 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux-x64",
                    var arch when arch == Architecture.X86 && RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux-x86",

                    _ => RuntimeInformation.RuntimeIdentifier
                }
            };

            // Try to directly lookup native assembly
            if (!string.IsNullOrWhiteSpace(filename) && !string.IsNullOrWhiteSpace(rid))
            {
                var path = Path.Combine(basePath, "runtimes", rid, "native", filename);

                if (File.Exists(path))
                {
                    libHandle = NativeLibrary.Load(path);
                    return libHandle;
                }
            }

            // Try to find first assembly that matches correct filename
            if (!string.IsNullOrWhiteSpace(filename))
            {
                var path = Directory.EnumerateFiles(basePath, filename, SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    libHandle = NativeLibrary.Load(path);
                    return libHandle;
                }
            }

            // Try to find any native assembly with a pdfium name
            var possibleFileNames = new string[]
            {
                "pdfium.dll",
                "libpdfium.so",
                "libpdfium.dylib"
            };
            foreach (var possibleName in possibleFileNames)
            {
                var path = Directory.EnumerateFiles(basePath, filename, SearchOption.AllDirectories).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    libHandle = NativeLibrary.Load(path);
                    return libHandle;
                }
            }
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

    public static PageRotation FPDFPage_GetRotation(IntPtr page)
    {
        lock (_lockObj)
            return (PageRotation)ExternalMethods.FPDFPage_GetRotation(page);
    }

    public static void FPDF_ClosePage(IntPtr page)
    {
        lock (_lockObj)
            ExternalMethods.FPDF_ClosePage(page);
    }

    public static bool FPDFPage_GetTrimBox(IntPtr page, out Rectangle rectangle)
    {
        lock (_lockObj)
        {
            if (ExternalMethods.FPDFPage_GetTrimBox(page, out var left, out var bottom, out var right, out var top))
            {
                rectangle = new Rectangle(left, bottom, right, top);
                return true;
            }
        }
        rectangle = new Rectangle(0, 0);
        return false;
    }

    public static bool FPDFPage_GetCropBox(IntPtr page, out Rectangle rectangle)
    {
        lock (_lockObj)
        {
            if (ExternalMethods.FPDFPage_GetCropBox(page, out var left, out var bottom, out var right, out var top))
            {
                rectangle = new Rectangle(left, bottom, right, top);
                return true;
            }
        }
        rectangle = new Rectangle(0, 0);
        return false;
    }

    public static bool FPDFPage_GetMediaBox(IntPtr page, out Rectangle rectangle)
    {
        lock (_lockObj)
        {
            if (ExternalMethods.FPDFPage_GetMediaBox(page, out var left, out var bottom, out var right, out var top))
            {
                rectangle = new Rectangle(left, bottom, right, top);
                return true;
            }
        }
        rectangle = new Rectangle(0, 0);
        return false;
    }

    public static bool FPDFPage_GetBleedBox(IntPtr page, out Rectangle rectangle)
    {
        lock (_lockObj)
        {
            if (ExternalMethods.FPDFPage_GetBleedBox(page, out var left, out var bottom, out var right, out var top))
            {
                rectangle = new Rectangle(left, bottom, right, top);
                return true;
            }
        }
        rectangle = new Rectangle(0, 0);
        return false;
    }

    public static bool FPDFPage_GetArtBox(IntPtr page, out Rectangle rectangle)
    {
        lock (_lockObj)
        {
            if (ExternalMethods.FPDFPage_GetArtBox(page, out var left, out var bottom, out var right, out var top))
            {
                rectangle = new Rectangle(left, bottom, right, top);
                return true;
            }
        }
        rectangle = new Rectangle(0, 0);
        return false;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match FPDF delegate naming")]
    private delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr buffer, uint size);

    private static readonly FPDF_GetBlockDelegate _getBlockDelegate = _fPDF_GetBlock;

    // Invoked by pdfium from native code, both during FPDF_LoadCustomDocument and
    // lazily while pages are loaded and rendered. Exceptions must not escape into
    // the native frame, so all failures are reported to pdfium as a zero return.
    private static int _fPDF_GetBlock(IntPtr param, uint position, IntPtr buffer, uint size)
    {
        try
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
        catch
        {
            return 0;
        }
    }
}
