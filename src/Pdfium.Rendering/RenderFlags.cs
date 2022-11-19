using System;

namespace Pdfium.Rendering
{
    /// <summary>
    /// Rendering flags to change the way pages are rendered.
    /// </summary>
    [Flags]
    public enum RenderFlags
    {
        /// <summary>
        /// Set if annotations are to be rendered.
        /// </summary>
        RenderAnnotations         = 0b0000_0000_0000_0001,
        /// <summary>
        /// Set if using text rendering optimized for LCD display. This flag will only
        /// take effect if anti-aliasing is enabled for text.
        /// </summary>
        OptimizeTextForLcdDisplay = 0b0000_0000_0000_0010,
        /// <summary>
        /// Don't use the native text output available on some platforms
        /// </summary>
        NoNativeText              = 0b0000_0000_0000_0100,
        /// <summary>
        /// Grayscale output.
        /// </summary>
        GrayScale                 = 0b0000_0000_0000_1000,
        /// <summary>
        /// Always use halftone for image stretching.
        /// </summary>
        ForceHalftone             = 0b0000_0100_0000_0000,
        /// <summary>
        /// Render for printing.
        /// </summary>
        ForPrinting               = 0b0000_1000_0000_0000,
        /// <summary>
        /// Set to disable anti-aliasing on text. This flag will also disable LCD
        /// optimization for text rendering.
        /// </summary>
        NoSmoothText              = 0b0001_0000_0000_0000,
        /// <summary>
        /// Set to disable anti-aliasing on images.
        /// </summary>
        NoSmoothImage             = 0b0010_0000_0000_0000,
        /// <summary>
        /// Set to disable anti-aliasing on paths.
        /// </summary>
        NoSmoothPath              = 0b0100_0000_0000_0000,
    }
}
