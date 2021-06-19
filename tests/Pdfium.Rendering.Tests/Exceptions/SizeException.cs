namespace Pdfium.Rendering.Tests.Exceptions
{
    public class SizeException : Xunit.Sdk.AssertActualExpectedException
    {
        public SizeException(object expected, object actual, string methodName)
            : base(expected, actual, $"{methodName} Failure")
        { }
    }
}
