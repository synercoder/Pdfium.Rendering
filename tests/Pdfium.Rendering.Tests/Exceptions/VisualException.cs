using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pdfium.Rendering.Tests.Exceptions
{
    public class VisualException : Xunit.Sdk.AssertActualExpectedException
    {
        public VisualException(Image<Bgra32> expected, Image<Bgra32> actual, double difference, string methodName)
            : base(expected, actual, $"{methodName} Failure, actual is {difference.ToString("P")} different")
        {
            Difference = difference;
            ExpectedImage = expected;
            ActualImage = actual;
        }

        public double Difference { get; }

        public Image<Bgra32> ExpectedImage { get; }
        public Image<Bgra32> ActualImage { get; }
    }
}
