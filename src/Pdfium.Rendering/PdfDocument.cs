using Pdfium.Rendering.Internals;
using System;
using System.Collections.Generic;
using System.IO;

namespace Pdfium.Rendering
{
    /// <summary>
    /// Class representing a PDF document
    /// </summary>
    public class PdfDocument : IDisposable
    {
        private readonly int _streamId;
        private readonly IntPtr _documentPointer;
        private bool _disposed;

        /// <summary>
        /// Construct a <see cref="PdfDocument"/>
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> containing a pdf document</param>
        public PdfDocument(Stream stream)
            : this(stream, null)
        { }

        /// <summary>
        /// Construct a <see cref="PdfDocument"/>
        /// </summary>
        /// <param name="stream"><see cref="Stream"/> containing a pdf document</param>
        /// <param name="password">The password needed to open <paramref name="stream"/> document.</param>
        public PdfDocument(Stream stream, string? password)
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));

            _streamId = StreamTracker.Register(stream);

            _documentPointer = SecuredWrapper.FPDF_LoadCustomDocument(stream, password, _streamId);

            PageCount = SecuredWrapper.FPDF_GetPageCount(_documentPointer);

            PageSizes = _getPageSizes();
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
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            return new PdfPage(_documentPointer, pageNumber);
        }

        private IReadOnlyCollection<Size> _getPageSizes()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

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
            if (!_disposed)
            {
                if (disposing)
                {
                    StreamTracker.Unregister(_streamId);

                    SecuredWrapper.FPDF_CloseDocument(_documentPointer);
                }
                _disposed = true;
            }
        }
    }
}
