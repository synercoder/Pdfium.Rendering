using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace Pdfium.Rendering.Internals;

internal static class StreamTracker
{
    private static int _id = 0;
    private static readonly ConcurrentDictionary<int, Stream> _files = new ConcurrentDictionary<int, Stream>();

    public static int Register(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        int id = Interlocked.Increment(ref _id);
        _files.TryAdd(id, stream);
        return id;
    }

    public static Stream Unregister(int id)
    {
        return _files.TryRemove(id, out var stream)
            ? stream
            : throw new ArgumentOutOfRangeException(nameof(id));
    }

    /// <summary>
    /// Look up a registered stream, or null when no stream is registered for <paramref name="id"/>.
    /// </summary>
    /// <remarks>
    /// Returns null rather than throwing because the only caller is the FPDF_FILEACCESS
    /// callback, which is invoked from native code. Throwing there would propagate a
    /// managed exception through a native frame.
    /// </remarks>
    public static Stream? Get(int id)
    {
        return _files.TryGetValue(id, out Stream? stream)
            ? stream
            : null;
    }
}
