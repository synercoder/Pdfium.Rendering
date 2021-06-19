using System;
using System.Runtime.InteropServices;

namespace Pdfium.Rendering.Internals
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal class FPDF_FILEACCESS
    {
        public uint m_FileLen;
        public IntPtr m_GetBlock;
        public IntPtr m_Param;
    }
#pragma warning restore IDE1006 // Naming Styles
}
