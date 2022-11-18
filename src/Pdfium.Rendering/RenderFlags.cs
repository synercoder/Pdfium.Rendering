using System;

namespace Pdfium.Rendering
{
    [Flags]
    public enum RenderFlags
    {
        RenderAnnotations         = 0b0000_0000_0000_0001,
        OptimizeTextForLcdDisplay = 0b0000_0000_0000_0010,
        NoNativeText              = 0b0000_0000_0000_0100,
        GrayScale                 = 0b0000_0000_0000_1000,
        ForceHalftone             = 0b0000_0100_0000_0000,
        ForPrinting               = 0b0000_1000_0000_0000,
        NoSmoothText              = 0b0001_0000_0000_0000,
        NoSmoothImage             = 0b0010_0000_0000_0000,
        NoSmoothPath              = 0b0100_0000_0000_0000,
    }
}
