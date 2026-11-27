using Pdfium.Rendering.Tests.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using Xunit;

namespace Pdfium.Rendering.Tests;

public class RenderTests
{
    [Theory]
    [InlineData("simple.pdf", 1, 72, "compare-files/page_1.png")]
    [InlineData("simple.pdf", 2, 72, "compare-files/page_2.png")]
    [InlineData("simple.pdf", 3, 72, "compare-files/page_3.png")]
    [InlineData("simple.pdf", 4, 72, "compare-files/page_4.png")]
    [InlineData("colored-boxes.pdf", 1, 72, "compare-files/colored-boxes_page_1.png")]
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

    [Fact]
    public void RenderPart_BottomLeftQuarter()
    {
        using (var stream = File.OpenRead("simple.pdf"))
        using (var pdfFile = new PdfDocument(stream))
        using (var page = pdfFile.GetPage(1))
        {
            var (w, h) = page.Size;
            int pageWidth = (int)w;
            int pageHeight = (int)h;

            // Bitmap covers the bottom-left quarter; shift the page up so its
            // bottom half lands inside the bitmap. The rest is clipped by Pdfium.
            int bmpWidth = pageWidth / 2;
            int bmpHeight = pageHeight - ( pageHeight / 2 );
            int startY = -( pageHeight / 2 );

            var render = page.RenderPart(0, startY, bmpWidth, bmpHeight, pageWidth, pageHeight);

            using (var img = Image.LoadPixelData<Bgra32>(render.Data, render.Width, render.Height))
            using (var compareImage = Image.Load("compare-files/page_1_bottom_left.png"))
            {
                ImageAssert.VisualEquals(compareImage, img, 0.0001);
            }
        }
    }
}
