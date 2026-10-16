using Pdfium.Rendering.Tests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace Pdfium.Rendering.Tests;

/// <summary>
/// Tests how a rendered page is composited onto its background, using synthetic PDFs.
/// </summary>
public class CompositingTests
{
    private const string FULL_PAGE_RECT = "0 0 100 100 re f";

    [Theory]
    [InlineData("Color", false)]
    [InlineData("Color", true)]
    [InlineData("Hue", false)]
    [InlineData("Hue", true)]
    [InlineData("Saturation", false)]
    [InlineData("Saturation", true)]
    [InlineData("Luminosity", false)]
    [InlineData("Luminosity", true)]
    public void BackdropDependentBlendMode_OnEmptyPage_RendersSourceColor(string blendMode, bool pageTransparencyGroup)
    {
        // Against an opaque white backdrop these blend modes turn red into white (Color, Hue,
        // Saturation) or grey (Luminosity). On an empty page the backdrop is transparent, so the
        // blend mode has no effect and the red fill must be visible as-is.
        var (r, g, b, a) = _renderCenterPixel(
            $"1 0 0 rg /GS0 gs {FULL_PAGE_RECT}",
            new Dictionary<string, string> { ["GS0"] = $"<< /Type /ExtGState /BM /{blendMode} >>" },
            pageTransparencyGroup);

        _assertPixel((255, 0, 0, 255), (r, g, b, a));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SemiTransparentFill_IsCompositedOverWhite(bool pageTransparencyGroup)
    {
        // 50% grey at 25% opacity over white: 128 * 0.25 + 255 * 0.75 = 223.
        var pixel = _renderCenterPixel(
            $"0.5 g /GS0 gs {FULL_PAGE_RECT}",
            new Dictionary<string, string> { ["GS0"] = "<< /Type /ExtGState /ca 0.25 >>" },
            pageTransparencyGroup);

        _assertPixel((223, 223, 223, 255), pixel);
    }

    [Fact]
    public void SemiTransparentFill_WithBackdropDependentBlendMode_IsCompositedOverWhite()
    {
        // (0.2, 0.4, 0.6) = (51, 102, 153) at 50% opacity with /BM /Color on an empty page:
        // the blend mode has no effect, then 50% over white gives (153, 178, 204).
        var pixel = _renderCenterPixel(
            $"0.2 0.4 0.6 rg /GS0 gs {FULL_PAGE_RECT}",
            new Dictionary<string, string> { ["GS0"] = "<< /Type /ExtGState /BM /Color /ca 0.5 >>" });

        _assertPixel((153, 178, 204, 255), pixel);
    }

    [Fact]
    public void SemiTransparentFill_WithTransparentBackground_ReturnsStraightAlpha()
    {
        var pixel = _renderCenterPixel(
            $"0.5 g /GS0 gs {FULL_PAGE_RECT}",
            new Dictionary<string, string> { ["GS0"] = "<< /Type /ExtGState /ca 0.25 >>" },
            transparentBackground: true);

        _assertPixel((128, 128, 128, 64), pixel);
    }

    [Fact]
    public void EmptyPage_RendersWhite()
    {
        var pixel = _renderCenterPixel(string.Empty);

        _assertPixel((255, 255, 255, 255), pixel);
    }

    private static (int R, int G, int B, int A) _renderCenterPixel(string content, IReadOnlyDictionary<string, string>? extGStates = null, bool pageTransparencyGroup = false, bool transparentBackground = false)
    {
        using var stream = MinimalPdf.Create(content, extGStates, pageTransparencyGroup);
        using var document = new PdfDocument(stream);
        using var page = document.GetPage(1);

        var render = page.Render(72, 72, transparentBackground);
        var data = render.Data;
        int i = ((render.Height / 2 * render.Width) + (render.Width / 2)) * 4;

        // Render data is BGRA
        return (data[i + 2], data[i + 1], data[i], data[i + 3]);
    }

    private static void _assertPixel((int R, int G, int B, int A) expected, (int R, int G, int B, int A) actual)
    {
        const int TOLERANCE = 2;

        bool equal = Math.Abs(expected.R - actual.R) <= TOLERANCE
            && Math.Abs(expected.G - actual.G) <= TOLERANCE
            && Math.Abs(expected.B - actual.B) <= TOLERANCE
            && Math.Abs(expected.A - actual.A) <= TOLERANCE;

        Assert.True(equal, $"Expected RGBA {expected} (±{TOLERANCE}), but got {actual}.");
    }
}
