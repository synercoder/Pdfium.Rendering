using System;
using System.Runtime.InteropServices;

namespace Pdfium.Rendering.Internals
{
    internal static class ExternalMethods
    {
        public const string LIBRARY_NAME = "pdfium";

        [DllImport(LIBRARY_NAME)]
        public static extern void FPDF_InitLibrary();

        [DllImport(LIBRARY_NAME)]
        public static extern void FPDF_DestroyLibrary();

        [DllImport(LIBRARY_NAME, CharSet = CharSet.Ansi)]
        public static extern IntPtr FPDF_LoadCustomDocument([MarshalAs(UnmanagedType.LPStruct)] FPDF_FILEACCESS pFileAccess, [MarshalAs(UnmanagedType.LPWStr)] string ? password);

        [DllImport(LIBRARY_NAME)]
        public static extern void FPDF_CloseDocument(IntPtr document);

        [DllImport(LIBRARY_NAME)]
        public static extern int FPDF_GetPageCount(IntPtr document);

        [DllImport(LIBRARY_NAME)]
        public static extern bool FPDF_GetPageSizeByIndexF(IntPtr document, int page_index, [MarshalAs(UnmanagedType.LPStruct)] FS_SIZEF size);

        [DllImport(LIBRARY_NAME)]
        public static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

        [DllImport(LIBRARY_NAME)]
        public static extern IntPtr FPDFBitmap_Destroy(IntPtr bitmap);

        [DllImport(LIBRARY_NAME)]
        public static extern void FPDFBitmap_FillRect(IntPtr bitmap, int left, int top, int width, int height, uint color);
        
        [DllImport(LIBRARY_NAME)]
        public static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

        [DllImport(LIBRARY_NAME)]
        public static extern IntPtr FPDF_LoadPage(IntPtr document, int page_index);

        [DllImport(LIBRARY_NAME)]
        public static extern IntPtr FPDFText_LoadPage(IntPtr page);

        [DllImport(LIBRARY_NAME)]
        public static extern void FPDFText_ClosePage(IntPtr text_page);

        [DllImport(LIBRARY_NAME)]
        public static extern void FPDF_ClosePage(IntPtr page);

        [DllImport(LIBRARY_NAME)]
        public static extern bool FPDFPage_GetTrimBox(IntPtr page, out float left, out float bottom, out float right, out float top);

        [DllImport(LIBRARY_NAME)]
        public static extern bool FPDFPage_GetBleedBox(IntPtr page, out float left, out float bottom, out float right, out float top);

        [DllImport(LIBRARY_NAME)]
        public static extern bool FPDFPage_GetCropBox(IntPtr page, out float left, out float bottom, out float right, out float top);

        [DllImport(LIBRARY_NAME)]
        public static extern bool FPDFPage_GetMediaBox(IntPtr page, out float left, out float bottom, out float right, out float top);

        [DllImport(LIBRARY_NAME)]
        public static extern bool FPDFPage_GetArtBox(IntPtr page, out float left, out float bottom, out float right, out float top);
    }
}
