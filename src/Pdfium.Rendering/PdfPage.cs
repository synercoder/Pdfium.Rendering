using Pdfium.Rendering.Internals;
using System;

namespace Pdfium.Rendering
{
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
        }

        /// <summary>
        /// The <see cref="Size"/> of this page
        /// </summary>
        public Size Size { get; }

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
        /// <returns>A <see cref="Render"/> of this page.</returns>
        public Render Render(float dpiX = 72, float dpiY = 72, bool transparentBackground = false)
        {
            const int BYTES_PER_PIXEL = 4;
            var (w, h) = Size;

            var width = (int)(w / 72 * dpiX);
            var height = (int)(h / 72 * dpiY);

            var byteArray = new byte[BYTES_PER_PIXEL * width * height];

            using (var pinnedArray = Pinned.Pin(byteArray))
            {
                var handle = SecuredWrapper.FPDFBitmap_CreateEx(width, height, 4, pinnedArray.AddresOfPin, width * BYTES_PER_PIXEL);

                uint background = transparentBackground 
                    ? 0x00FFFFFF 
                    : 0xFFFFFFFF;

                SecuredWrapper.FPDFBitmap_FillRect(handle, 0, 0, width, height, background);

                SecuredWrapper.FPDF_RenderPageBitmap(handle, _page, 0, 0, width, height, 0, 0);
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
}
