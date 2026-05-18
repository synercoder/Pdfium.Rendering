namespace Pdfium.Rendering;

/// <summary>
/// Value indicating the page rotation.
/// </summary>
public enum PageRotation
{
    /// <summary>
    /// Returned on error.
    /// </summary>
    Error = -1,

    /// <summary>
    /// No rotation.
    /// </summary>
    NoRotation = 0,

    /// <summary>
    /// Rotated 90 degrees clockwise.
    /// </summary>
    Rotate90 = 1,

    /// <summary>
    /// Rotated 180 degrees clockwise.
    /// </summary>
    Rotate180 = 2,

    /// <summary>
    /// Rotated 270 degrees clockwise.
    /// </summary>
    Rotate270 = 3,
}
