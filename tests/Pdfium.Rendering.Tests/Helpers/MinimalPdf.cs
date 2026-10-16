using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Pdfium.Rendering.Tests.Helpers;

/// <summary>
/// Builds tiny single-page PDFs in memory so rendering edge cases can be tested
/// without shipping (possibly confidential) real-world files.
/// </summary>
public static class MinimalPdf
{
    /// <summary>
    /// Create a 100x100pt page with the given content stream.
    /// </summary>
    /// <param name="content">The page content stream.</param>
    /// <param name="extGStates">ExtGState dictionaries keyed by resource name, e.g. <c>GS0</c> =&gt; <c>&lt;&lt; /BM /Color &gt;&gt;</c>.</param>
    /// <param name="pageTransparencyGroup">Whether the page has a <c>/Group &lt;&lt; /S /Transparency &gt;&gt;</c> entry.</param>
    public static MemoryStream Create(string content, IReadOnlyDictionary<string, string>? extGStates = null, bool pageTransparencyGroup = false)
    {
        var gs = new StringBuilder();
        if (extGStates != null)
            foreach (var (name, dict) in extGStates)
                gs.Append('/').Append(name).Append(' ').Append(dict).Append(' ');

        var group = pageTransparencyGroup ? "/Group << /Type /Group /S /Transparency /CS /DeviceRGB >> " : string.Empty;

        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 100 100] {group}/Resources << /ExtGState << {gs}>> >> /Contents 4 0 R >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream",
        };

        var pdf = new StringBuilder("%PDF-1.7\n");
        var offsets = new List<int>();
        for (int i = 0; i < objects.Length; i++)
        {
            offsets.Add(pdf.Length);
            pdf.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }

        int xref = pdf.Length;
        pdf.Append("xref\n0 ").Append(objects.Length + 1).Append("\n0000000000 65535 f \n");
        foreach (var offset in offsets)
            pdf.Append(offset.ToString("D10")).Append(" 00000 n \n");
        pdf.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF\n");

        return new MemoryStream(Encoding.ASCII.GetBytes(pdf.ToString()));
    }
}
