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

    public static Stream Get(int id)
    {
        return _files.TryGetValue(id, out Stream? stream)
            ? stream
            : throw new ArgumentOutOfRangeException(nameof(id));
    }
}
