using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace Pdfium.Rendering.TestConsole;

public class Program
{
    public static void Main(string[] args)
    {
        _process("simple.pdf", pageNumber => $"simple_{pageNumber}.png");

        _process("colored-boxes.pdf", pageNumber => $"boxes_{pageNumber}.png");
    }

    private static void _process(string filename, Func<int, string> pageNumberToPngFilename)
    {
        using (var stream = File.OpenRead(filename))
        {
            using (var pdfFile = new PdfDocument(stream))
            {
                Console.WriteLine("Pdf loaded");

                for (int i = 1; i <= pdfFile.PageCount; i++)
                {
                    using (var page = pdfFile.GetPage(i))
                    {
                        var render = page.Render();
                        using (var img = Image.LoadPixelData<Bgra32>(render.Data, render.Width, render.Height))
                        {
                            img.SaveAsPng(pageNumberToPngFilename(i));
                        }
                    }
                }
            }
        }
    }
}
