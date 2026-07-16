using Pdfium.Rendering.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Pdfium.Rendering;

/// <summary>
/// Class representing a PDF document
/// </summary>
public class PdfDocument : IDisposable
{
    private readonly int _streamId;
    private readonly IntPtr _documentPointer;
    private readonly bool _ownsStream;

    // 0 = live, 1 = disposed. Written with Interlocked so that concurrent calls to
    // Dispose cannot both observe "live" and close the same document twice.
    private int _disposed;

    /// <summary>
    /// Construct a <see cref="PdfDocument"/>
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> containing a pdf document</param>
    public PdfDocument(Stream stream)
        : this(stream, null, false)
    { }

    /// <summary>
    /// Construct a <see cref="PdfDocument"/>
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> containing a pdf document</param>
    /// <param name="ownsStream">If passed true, when disposing, the stream will also be disposed.</param>
    public PdfDocument(Stream stream, bool ownsStream)
        : this(stream, null, ownsStream)
    { }

    /// <summary>
    /// Construct a <see cref="PdfDocument"/>
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> containing a pdf document</param>
    /// <param name="password">The password needed to open <paramref name="stream"/> document.</param>
    /// <param name="ownsStream">If passed true, when disposing, the stream will also be disposed.</param>
    public PdfDocument(Stream stream, string? password, bool ownsStream)
    {
        _ = stream ?? throw new ArgumentNullException(nameof(stream));

        _streamId = StreamTracker.Register(stream);

        _documentPointer = SecuredWrapper.FPDF_LoadCustomDocument(stream, password, _streamId);

        PageCount = SecuredWrapper.FPDF_GetPageCount(_documentPointer);

        PageSizes = _getPageSizes();
        _ownsStream = ownsStream;
    }

    /// <summary>
    /// The amount of pages in the document
    /// </summary>
    public int PageCount { get; }

    /// <summary>
    /// Collection of page sizes
    /// </summary>
    public IReadOnlyCollection<Size> PageSizes { get; }

    /// <summary>
    /// Get a <see cref="PdfPage"/> object
    /// </summary>
    /// <param name="pageNumber">The pagenumber of the page to get.</param>
    /// <returns>A <see cref="PdfPage"/> object.</returns>
    public PdfPage GetPage(int pageNumber)
    {
        if (Volatile.Read(ref _disposed) != 0)
            throw new ObjectDisposedException(GetType().Name);

        return new PdfPage(_documentPointer, pageNumber);
    }

    private IReadOnlyCollection<Size> _getPageSizes()
    {
        var result = new List<Size>(PageCount);

        for (int i = 0; i < PageCount; i++)
        {
            result.Add(SecuredWrapper.FPDF_GetPageSizeByIndexF(_documentPointer, i));
        }

        return result;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose this <see cref="PdfDocument"/>
    /// </summary>
    /// <param name="disposing">Boolean indicating if the dispose was called from .Dispose or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        if (disposing)
        {
            // Close the document before releasing the stream. Pdfium reads document data
            // lazily through the FPDF_FILEACCESS callback and may still read while closing,
            // so the stream has to stay registered and open until the document is gone.
            SecuredWrapper.FPDF_CloseDocument(_documentPointer);

            var stream = StreamTracker.Unregister(_streamId);
            if (_ownsStream)
            {
                stream?.Dispose();
            }
        }
    }
}
