using SixLabors.ImageSharp;

namespace Pdfium.Rendering.Tests.Exceptions;

public class VisualException : Xunit.Sdk.XunitException
{
    public VisualException(Image expected, Image actual, double difference, string methodName)
        : base($"{methodName} Failure, actual is {difference * 100}% different")
    {
        Difference = difference;
        ExpectedImage = expected;
        ActualImage = actual;
    }

    public double Difference { get; }

    public Image ExpectedImage { get; }
    public Image ActualImage { get; }
}
