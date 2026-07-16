using System;

namespace Pdfium.Rendering;

/// <summary>
/// Thrown when pdfium reports an error.
/// </summary>
public class PdfiumException : Exception
{
    /// <summary>
    /// Construct a <see cref="PdfiumException"/>
    /// </summary>
    /// <param name="errorCode">The <see cref="PdfiumErrorCode"/> reported by pdfium.</param>
    public PdfiumException(PdfiumErrorCode errorCode)
        : this(errorCode, _messageFor(errorCode))
    { }

    /// <summary>
    /// Construct a <see cref="PdfiumException"/>
    /// </summary>
    /// <param name="errorCode">The <see cref="PdfiumErrorCode"/> reported by pdfium.</param>
    /// <param name="message">The message that describes the error.</param>
    public PdfiumException(PdfiumErrorCode errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// The <see cref="PdfiumErrorCode"/> reported by pdfium.
    /// </summary>
    public PdfiumErrorCode ErrorCode { get; }

    private static string _messageFor(PdfiumErrorCode errorCode)
        => errorCode switch
        {
            PdfiumErrorCode.Success => "Pdfium reported success.",
            PdfiumErrorCode.File => "The file could not be found or could not be opened.",
            PdfiumErrorCode.Format => "The file is not a PDF document, or is corrupted.",
            PdfiumErrorCode.Password => "A password is required to open this document, or the supplied password is incorrect.",
            PdfiumErrorCode.Security => "The document uses an unsupported security scheme.",
            PdfiumErrorCode.Page => "The page could not be found, or its content could not be read.",
            _ => "Pdfium reported an unknown error.",
        };
}
