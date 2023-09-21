using Pdfium.Rendering.Internals;
using System;

namespace Pdfium.Rendering;

/// <summary>
/// Class representing a pdf page
/// </summary>
public class PdfPage : IDisposable
{
    private readonly IntPtr _page;
    private readonly IntPtr _textPage;
    private bool _disposed;

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
    public Render RenderToSize(int width, int height, bool transparentBackground = false, RenderFlags renderFlags = default)
    {
        if (width < 1)
            throw new ArgumentOutOfRangeException(nameof(width), "The value for the width parameter can not be less than 1.");
        if (height < 1)
            throw new ArgumentOutOfRangeException(nameof(height), "The value for the height parameter can not be less than 1.");

        const int BYTES_PER_PIXEL = 4;

        var byteArray = new byte[BYTES_PER_PIXEL * width * height];

        using (var pinnedArray = Pinned.Pin(byteArray))
        {
            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = SecuredWrapper.FPDFBitmap_CreateEx(width, height, 4, pinnedArray.AddresOfPin, width * BYTES_PER_PIXEL);

                uint background = transparentBackground
                    ? 0x00FFFFFF
                    : 0xFFFFFFFF;

                SecuredWrapper.FPDFBitmap_FillRect(handle, 0, 0, width, height, background);

                SecuredWrapper.FPDF_RenderPageBitmap(handle, _page, 0, 0, width, height, 0, (FPDF_RenderingFlags)renderFlags);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    SecuredWrapper.FPDFBitmap_Destroy(handle);
            }
        }

        return new Render(byteArray, width, height);
    }

    /// <summary>
    /// Dispose this <see cref="PdfPage"/>
    /// </summary>
    /// <param name="disposing">Boolean indicating if the dispose was called from .Dispose or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                SecuredWrapper.FPDFText_ClosePage(_textPage);
                SecuredWrapper.FPDF_ClosePage(_page);
            }

            _disposed = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
