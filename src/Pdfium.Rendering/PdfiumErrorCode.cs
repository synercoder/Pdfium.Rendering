namespace Pdfium.Rendering;

/// <summary>
/// Error codes reported by pdfium, as returned by FPDF_GetLastError.
/// </summary>
public enum PdfiumErrorCode
{
    /// <summary>
    /// No error.
    /// </summary>
    Success = 0,

    /// <summary>
    /// Unknown error.
    /// </summary>
    Unknown = 1,

    /// <summary>
    /// File not found or could not be opened.
    /// </summary>
    File = 2,

    /// <summary>
    /// File not in PDF format or corrupted.
    /// </summary>
    Format = 3,

    /// <summary>
    /// Password required or incorrect password.
    /// </summary>
    Password = 4,

    /// <summary>
    /// Unsupported security scheme.
    /// </summary>
    Security = 5,

    /// <summary>
    /// Page not found or content error.
    /// </summary>
    Page = 6,
}
