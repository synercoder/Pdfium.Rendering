using SixLabors.ImageSharp;

namespace Pdfium.Rendering.Tests.Exceptions;

public class SizeException : Xunit.Sdk.XunitException
{
    public SizeException(Image expected, Image actual, string methodName)
        : base($"{methodName} Failure: expected size {expected.Width}x{expected.Height}, but actual size is {actual.Width}x{actual.Height}")
    { }
}
