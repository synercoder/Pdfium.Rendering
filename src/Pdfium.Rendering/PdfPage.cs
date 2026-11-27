using Pdfium.Rendering.Internals;
using System;
using System.Threading;

namespace Pdfium.Rendering;

/// <summary>
/// Class representing a pdf page
/// </summary>
public class PdfPage : IDisposable
{
    private readonly IntPtr _page;
    private readonly IntPtr _textPage;

    // 0 = live, 1 = disposed. Written with Interlocked so that concurrent calls to
    // Dispose cannot both observe "live" and close the same native handles twice.
    private int _disposed;

    internal PdfPage(IntPtr document, int pageNumber)
    {
        PageNumber = pageNumber;
        Size = SecuredWrapper.FPDF_GetPageSizeByIndexF(document, PageNumber - 1);

        _page = SecuredWrapper.FPDF_LoadPage(document, PageNumber - 1);
        _textPage = SecuredWrapper.FPDFText_LoadPage(_page);

        if (SecuredWrapper.FPDFPage_GetMediaBox(_page, out var mediaBox))
            MediaBox = mediaBox;
        if (SecuredWrapper.FPDFPage_GetCropBox(_page, out var cropBox))
            CropBox = cropBox;
        if (SecuredWrapper.FPDFPage_GetBleedBox(_page, out var bleedBox))
            BleedBox = bleedBox;
        if (SecuredWrapper.FPDFPage_GetTrimBox(_page, out var trimBox))
            TrimBox = trimBox;
        if (SecuredWrapper.FPDFPage_GetArtBox(_page, out var artBox))
            ArtBox = artBox;

        PageRotation = SecuredWrapper.FPDFPage_GetRotation(_page);
    }

    /// <summary>
    /// The <see cref="Size"/> of this page
    /// </summary>
    public Size Size { get; }

    /// <summary>
    /// Get "MediaBox" entry from the page dictionary.
    /// </summary>
    public Rectangle? MediaBox { get; }

    /// <summary>
    /// Get "CropBox" entry from the page dictionary.
    /// </summary>
    public Rectangle? CropBox { get; } = null;

    /// <summary>
    /// Get "BleedBox" entry from the page dictionary.
    /// </summary>
    public Rectangle? BleedBox { get; } = null;

    /// <summary>
    /// Get "TrimBox" entry from the page dictionary.
    /// </summary>
    public Rectangle? TrimBox { get; } = null;

    /// <summary>
    /// Get "ArtBox" entry from the page dictionary.
    /// </summary>
    public Rectangle? ArtBox { get; } = null;

    /// <summary>
    /// The number of this page
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// Get the rotation of this page.
    /// </summary>
    public PageRotation PageRotation { get; } = PageRotation.NoRotation;

    /// <summary>
    /// Render this page to <see cref="Render"/>
    /// </summary>
    /// <param name="dpiX">The DPI to use for the x-axis</param>
    /// <param name="dpiY">The DPI to use for the y-axis</param>
    /// <param name="transparentBackground">Boolean to indicate if the background should be rendered as transparent or white.</param>
    /// <param name="renderFlags">Use flags to change the render mode.</param>
    /// <returns>A <see cref="Render"/> of this page.</returns>
    public Render Render(float dpiX = 72, float dpiY = 72, bool transparentBackground = false, RenderFlags renderFlags = default)
    {
        var (w, h) = Size;

        var width = (int)( w / 72 * dpiX );
        var height = (int)( h / 72 * dpiY );

        return RenderToSize(width, height, transparentBackground, renderFlags);
    }

    /// <summary>
    /// Render this page to <see cref="Render"/> with prodivded <paramref name="width"/> &amp; <paramref name="height"/>.
    /// </summary>
    /// <param name="width">The width to render.</param>
    /// <param name="height">The height to render.</param>
    /// <param name="transparentBackground">Boolean to indicate if the background should be rendered as transparent or white.</param>
    /// <param name="renderFlags">Use flags to change the render mode.</param>
    /// <returns>A <see cref="Render"/> of this page.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is less than 1.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this <see cref="PdfPage"/> has been disposed.</exception>
    public Render RenderToSize(int width, int height, bool transparentBackground = false, RenderFlags renderFlags = default)
    {
        return RenderPart(0, 0, width, height, width, height, transparentBackground, renderFlags);
    }

    /// <summary>
    /// Render part of this page to <see cref="Render"/>.
    /// Render target is a bitmap of size <paramref name="bmpWidth"/> x <paramref name="bmpHeight"/>.
    /// The page is rendered into the bitmap at the specified <paramref name="startX"/> and <paramref name="startY"/> coordinates, with the specified <paramref name="pageWidth"/> and <paramref name="pageHeight"/> dimensions.
    /// Any area outside of the bmpWidth x bmpHeight bitmap will be clipped and not rendered.
    /// </summary>
    /// <param name="startX">The x-coordinate, in bitmap pixels, where the left edge of the page is placed. Can be negative to shift the page left, e.g. to render a part further right on the page.</param>
    /// <param name="startY">The y-coordinate, in bitmap pixels, where the top edge of the page is placed. Can be negative to shift the page up, e.g. to render a part further down on the page.</param>
    /// <param name="bmpWidth">The bitmap width to render.</param>
    /// <param name="bmpHeight">The bitmap height to render.</param>
    /// <param name="pageWidth">The page width to render.</param>
    /// <param name="pageHeight">The page height to render.</param>
    /// <param name="transparentBackground">Boolean to indicate if the background should be rendered as transparent or white.</param>
    /// <param name="renderFlags">Use flags to change the render mode.</param>
    /// <returns>A <see cref="Render"/> of this page.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bmpWidth"/>, <paramref name="bmpHeight"/>, <paramref name="pageWidth"/> or <paramref name="pageHeight"/> is less than 1.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this <see cref="PdfPage"/> has been disposed.</exception>
    public Render RenderPart(int startX, int startY, int bmpWidth, int bmpHeight, int pageWidth, int pageHeight, bool transparentBackground = false, RenderFlags renderFlags = default)
    {
        if (Volatile.Read(ref _disposed) != 0)
            throw new ObjectDisposedException(GetType().Name);

        if (pageWidth < 1)
            throw new ArgumentOutOfRangeException(nameof(pageWidth), "The value for the page width parameter can not be less than 1.");
        if (pageHeight < 1)
            throw new ArgumentOutOfRangeException(nameof(pageHeight), "The value for the page height parameter can not be less than 1.");
        if (bmpWidth < 1)
            throw new ArgumentOutOfRangeException(nameof(bmpWidth), "The value for the bitmap width parameter can not be less than 1.");
        if (bmpHeight < 1)
            throw new ArgumentOutOfRangeException(nameof(bmpHeight), "The value for the bitmap height parameter can not be less than 1.");

        const int BYTES_PER_PIXEL = 4;

        var byteArray = new byte[BYTES_PER_PIXEL * bmpWidth * bmpHeight];

        using (var pinnedArray = Pinned.Pin(byteArray))
        {
            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = SecuredWrapper.FPDFBitmap_CreateEx(bmpWidth, bmpHeight, 4, pinnedArray.AddresOfPin, bmpWidth * BYTES_PER_PIXEL);

                // Always render onto a fully transparent backdrop. Pdfium treats the
                // pixels the bitmap is pre-filled with as the page's initial backdrop.
                // Filling with an opaque colour (e.g. white) means page-level blend
                // modes and transparency groups composite against that colour, which
                // for backdrop-dependent blend modes (Color/Luminosity/Hue/Saturation)
                // can collapse the whole page to the backdrop colour (a blank page).
                // A transparent backdrop gives the isolated-group semantics that
                // conformant viewers use, after which we composite over the requested
                // background ourselves.
                SecuredWrapper.FPDFBitmap_FillRect(handle, 0, 0, bmpWidth, bmpHeight, 0x00000000);

                SecuredWrapper.FPDF_RenderPageBitmap(handle, _page, startX, startY, pageWidth, pageHeight, 0, (FPDF_RenderingFlags)renderFlags);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    SecuredWrapper.FPDFBitmap_Destroy(handle);
            }
        }

        if (!transparentBackground)
        {
            // Pdfium BGRA bitmaps use straight (non-premultiplied) alpha; composite
            // over an opaque white background: out = (channel * alpha + 255 * (255 - alpha)) / 255.
            // Opaque pixels are already correct, which is the common case.
            for (int i = 0; i < byteArray.Length; i += BYTES_PER_PIXEL)
            {
                int alpha = byteArray[i + 3];
                if (alpha == 255)
                    continue;

                int white = ( 255 * ( 255 - alpha ) ) + 127;
                byteArray[i] = (byte)( ( ( byteArray[i] * alpha ) + white ) / 255 );
                byteArray[i + 1] = (byte)( ( ( byteArray[i + 1] * alpha ) + white ) / 255 );
                byteArray[i + 2] = (byte)( ( ( byteArray[i + 2] * alpha ) + white ) / 255 );
                byteArray[i + 3] = 255;
            }
        }

        return new Render(byteArray, bmpWidth, bmpHeight);
    }

    /// <summary>
    /// Dispose this <see cref="PdfPage"/>
    /// </summary>
    /// <param name="disposing">Boolean indicating if the dispose was called from .Dispose or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        if (disposing)
        {
            SecuredWrapper.FPDFText_ClosePage(_textPage);
            SecuredWrapper.FPDF_ClosePage(_page);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
