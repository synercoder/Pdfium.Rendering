using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pdfium.Rendering.Tests.Exceptions
{
    public class VisualException : Xunit.Sdk.AssertActualExpectedException
    {
        public VisualException(Image expected, Image actual, double difference, string methodName)
            : base(expected, actual, $"{methodName} Failure, actual is {difference * 100}% different")
        {
            Difference = difference;
            ExpectedImage = expected;
            ActualImage = actual;
        }

        public double Difference { get; }

        public Image ExpectedImage { get; }
        public Image ActualImage { get; }
    }
}
