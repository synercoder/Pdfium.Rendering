using System.IO;
using Xunit;

namespace Pdfium.Rendering.Tests;

public class PageBoxTests
{
    // colored-boxes.pdf has a distinct, nested box for every page box type,
    // each 5mm (14.173228346pt) inside the previous one.
    private const float TOLERANCE = 0.001f;

    [Fact]
    public void ColoredBoxes_ReadsAllPageBoxes()
    {
        using var stream = File.OpenRead("colored-boxes.pdf");
        using var document = new PdfDocument(stream);
        using var page = document.GetPage(1);

        _assertRectangle(new Rectangle(28.346456693f, 28.346456693f, 651.968503937f, 898.582677165f), page.MediaBox);
        _assertRectangle(new Rectangle(42.519685039f, 42.519685039f, 637.795275591f, 884.409448819f), page.CropBox);
        _assertRectangle(new Rectangle(56.692913386f, 56.692913386f, 623.622047244f, 870.236220472f), page.BleedBox);
        _assertRectangle(new Rectangle(70.866141732f, 70.866141732f, 609.448818898f, 856.062992126f), page.TrimBox);
        _assertRectangle(new Rectangle(85.039370079f, 85.039370079f, 595.275590551f, 841.88976378f), page.ArtBox);
    }

    [Fact]
    public void ColoredBoxes_SizeIsCropBoxSize()
    {
        using var stream = File.OpenRead("colored-boxes.pdf");
        using var document = new PdfDocument(stream);
        using var page = document.GetPage(1);

        // CropBox is 595.275590552 x 841.88976378 (A4)
        Assert.Equal(595.275590552f, page.Size.Width, TOLERANCE);
        Assert.Equal(841.88976378f, page.Size.Height, TOLERANCE);
    }

    private static void _assertRectangle(Rectangle expected, Rectangle? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Left, actual.Value.Left, TOLERANCE);
        Assert.Equal(expected.Bottom, actual.Value.Bottom, TOLERANCE);
        Assert.Equal(expected.Right, actual.Value.Right, TOLERANCE);
        Assert.Equal(expected.Top, actual.Value.Top, TOLERANCE);
    }
}
