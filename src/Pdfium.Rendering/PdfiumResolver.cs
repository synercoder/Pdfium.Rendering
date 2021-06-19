using System.Runtime.InteropServices;

namespace Pdfium.Rendering
{
    /// <summary>
    /// Static class that can be used to hook your own <see cref="DllImportResolver"/> to be used for loading PDFium
    /// </summary>
    public static class PdfiumResolver
    {
        /// <summary>
        /// Hook to use a custom <see cref="DllImportResolver"/> to resolve PDFium
        /// </summary>
        public static DllImportResolver? Resolve;
    }
}
