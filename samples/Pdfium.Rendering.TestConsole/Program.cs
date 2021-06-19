using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace Pdfium.Rendering.TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var stream = File.OpenRead("simple.pdf"))
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
                                img.SaveAsPng($"page_{i}.png");
                            }
                        }
                    }
                }
            }
        }
    }
}