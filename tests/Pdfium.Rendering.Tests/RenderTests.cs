using Pdfium.Rendering.Tests.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using Xunit;

namespace Pdfium.Rendering.Tests
{
    public class RenderTests
    {
        [Theory]
        [InlineData("simple.pdf", 1, 72, "compare-files/page_1.png")]
        [InlineData("simple.pdf", 2, 72, "compare-files/page_2.png")]
        [InlineData("simple.pdf", 3, 72, "compare-files/page_3.png")]
        [InlineData("simple.pdf", 4, 72, "compare-files/page_4.png")]
        public void RenderPage(string filePath, int pageNumber, int dpi, string compareFile)
        {
            using (var stream = File.OpenRead(filePath))
            using (var pdfFile = new PdfDocument(stream))
            {
                using (var page = pdfFile.GetPage(pageNumber))
                {
                    var render = page.Render(dpi, dpi);
                    using (var img = Image.LoadPixelData<Bgra32>(render.Data, render.Width, render.Height))
                    using (var compareImage = Image.Load(compareFile))
                    {
                        ImageAssert.VisualEquals(compareImage, img, 0.0001);
                    }
                }
            }
        }
    }
}
