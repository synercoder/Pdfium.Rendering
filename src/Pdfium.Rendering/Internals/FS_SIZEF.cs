using System.Runtime.InteropServices;

namespace Pdfium.Rendering.Internals
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal class FS_SIZEF
    {
        public float width;
        public float height;
    }
#pragma warning restore IDE1006 // Naming Styles
}
